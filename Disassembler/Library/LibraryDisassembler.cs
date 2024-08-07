﻿using System;
using System.Linq;
using X86Codec;

namespace Disassembler;

/// <summary>
/// Implements a specialized disassembler to analyze object library.
/// An object library contains additional symbol information, which is
/// helpful for binary analysis.
/// </summary>
public class LibraryDisassembler(ObjectLibrary library) : DisassemblerBase(library.Image)
{
    public override Assembly Assembly => library;

    protected override void GenerateProcedures()
    {
        // Enumerate the defined names, and assign names to the procedures.
        foreach (ObjectModule module in library.Modules.Cast<ObjectModule>())
        {
            foreach (DefinedSymbol symbol in module.DefinedNames)
            {
                if (symbol.BaseSegment != null)
                {
                    var address = new Address(symbol.BaseSegment.Id, (int)symbol.Offset);
                    if (image.IsAddressValid(address))
                    {
                        var b = image[address];
                        if (b.Type == ByteType.Code && b.IsLeadByte)
                        {
                            var proc = Procedures.Find(address);
                            if (proc == null)
                            {
                                proc = CreateProcedure(address);
                                Procedures.Add(proc);
                            }
                            proc.Name = symbol.Name;
                        }
                    }
                }
            }
        }
    }

    protected override Instruction DecodeInstruction(Address address)
    {
        var instruction = base.DecodeInstruction(address);
        if (instruction == null)
            return instruction;

        // Find the first fixup that covers the instruction. If no
        // fix-up covers the instruction, find the closest fix-up
        // that comes after.
        FixupCollection fixups = library.Image.GetSegment(address.Segment).Segment.Fixups;
        int fixupIndex = fixups.BinarySearch(address.Offset);

        // If there's a fixup right at the beginning of the instruction,
        // it is likely that the location is actually data, unless the
        // fixup is a floating point emulator which use a trick to change
        // the opcode.
        if (fixupIndex >= 0 && fixups[fixupIndex].StartIndex == address.Offset)
        {
            if (!IsFloatingPointEmulatorFixup(fixups[fixupIndex]))
            {
                AddError(address, ErrorCode.BrokenFixup,
                    "Cannot decode instruction at a fix-up location: {0}",
                    fixups[fixupIndex]);
                return null;
            }
        }

        if (fixupIndex < 0)
            fixupIndex = ~fixupIndex;

        for (int i = 0; i < instruction.Operands.Length; i++)
        {
            if (fixupIndex >= fixups.Count) // no more fixups
                break;

            Fixup fixup = fixups[fixupIndex];
            if (fixup.StartIndex >= address.Offset + instruction.EncodedLength) // past end
                break;

            Operand operand = instruction.Operands[i];
            if (operand.FixableLocation.Length > 0)
            {
                int start = address.Offset + operand.FixableLocation.StartOffset;
                int end = start + operand.FixableLocation.Length;

                if (fixup.StartIndex >= end)
                    continue;

                if (fixup.StartIndex != start || fixup.EndIndex != end)
                {
                    // throw new BrokenFixupException(fixup);
                    if (IsFloatingPointEmulatorFixup(fixup))
                    {
                        AddError(new Address(address.Segment, fixup.StartIndex),
                            ErrorCode.FixupDiscarded,
                            "Floating point emulator fix-up discarded: {0}", fixup);
                    }
                    else
                    {
                        AddError(new Address(address.Segment, fixup.StartIndex),
                            ErrorCode.BrokenFixup, "Broken fix-up: {0}", fixup);
                    }
                    continue;
                }

                instruction.Operands[i].Tag = fixup.Target;
                ++fixupIndex;
            }
        }

        if (fixupIndex < fixups.Count)
        {
            Fixup fixup = fixups[fixupIndex];
            if (fixup.StartIndex < address.Offset + instruction.EncodedLength)
            {
                if (IsFloatingPointEmulatorFixup(fixup))
                {
                    AddError(new Address(address.Segment, fixup.StartIndex),
                        ErrorCode.FixupDiscarded,
                        "Floating point emulator fix-up discarded: {0}", fixup);
                }
                else
                {
                    AddError(new Address(address.Segment, fixup.StartIndex),
                        ErrorCode.BrokenFixup, "Broken fix-up: {0}", fixup);
                }
            }
        }
        return instruction;
    }

    private bool IsFloatingPointEmulatorFixup(Fixup fixup) 
        => fixup.Target.Referent is ExternalSymbol symbol && symbol.Name switch
    {
        "FIARQQ" or "FICRQQ" or "FIDRQQ" or "FIERQQ" or "FISRQQ" or "FIWRQQ" or "FJARQQ" or "FJCRQQ" or "FJSRQQ" => true,
        _ => false,
    };

    private Address ResolveSymbolicTarget(SymbolicTarget symbolicTarget)
    {
        Address referentAddress = symbolicTarget.Referent.Resolve();
        if (referentAddress == Address.Invalid)
        {
            //AddError(start, ErrorCode.UnresolvedTarget,
            //    "Cannot resolve target: {0}.", symbolicTarget);
            return Address.Invalid;
        }
        Address symbolicAddress = referentAddress + (int)symbolicTarget.Displacement;
        return symbolicAddress;
    }

    protected override Address ResolveFlowInstructionTarget(RelativeOperand operand)
    {
        SymbolicTarget symbolicTarget = operand.Tag as SymbolicTarget;
        if (symbolicTarget != null)
        {
            Address symbolicAddress = ResolveSymbolicTarget(symbolicTarget);
            if (symbolicAddress != Address.Invalid)
            {
                Address target = symbolicAddress + operand.Offset.Value;
                return new Address(target.Segment, (UInt16)target.Offset);
            }
            return Address.Invalid;
        }
        return base.ResolveFlowInstructionTarget(operand);
    }

    protected override Address ResolveFlowInstructionTarget(PointerOperand operand)
    {
        if (operand.Tag is SymbolicTarget symbolicTarget)
        {
            Address symbolicAddress = ResolveSymbolicTarget(symbolicTarget);
            return symbolicAddress;
        }
        return base.ResolveFlowInstructionTarget(operand);
    }

    public override void Analyze()
    {
        foreach (ObjectModule module in library.Modules.Cast<ObjectModule>())
        {
            foreach (DefinedSymbol symbol in module.DefinedNames)
            {
                if (symbol.BaseSegment == null)
                    continue;
                if (!symbol.BaseSegment.Class.EndsWith("CODE"))
                    continue;

                // TODO: do not disassemble if the symbol is obviously
                // a data item.
                int iFixup = symbol.BaseSegment.Fixups.BinarySearch((int)symbol.Offset);
                if (iFixup >= 0 && symbol.BaseSegment.Fixups[iFixup].StartIndex
                    == (int)symbol.Offset) // likely a data item
                {
                    continue;
                }

                Address entryPoint = new Address(
                    symbol.BaseSegment.Id, (int)symbol.Offset);
                GenerateBasicBlocks(entryPoint, XRefType.UserSpecified);
            }
        }

        GenerateControlFlowGraph();
        GenerateProcedures();
        AddBasicBlocksToProcedures();
    }

#if false
    public static void Disassemble(ObjectLibrary library, Address entryPoint)
    {
        LibraryDisassembler dasm = new LibraryDisassembler(library);
        dasm.Analyze(entryPoint);
    }
#endif
}
