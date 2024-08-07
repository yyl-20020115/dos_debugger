﻿using System;
using System.Collections.Generic;
using X86Codec;

namespace Disassembler;

/// <summary>
/// Provides methods to disassemble and analyze 16-bit x86 binary code.
/// The class is trying to be decoupled from how the binary code is 
/// stored; therefore, it must be subclassed to provide implementation
/// of important methods.
/// </summary>
public abstract class DisassemblerBase(BinaryImage image)
{
    protected readonly BinaryImage image = image ?? throw new ArgumentNullException(nameof(image));

    /// <summary>
    /// Gets the assembly being analyzed. This property must be overriden
    /// by a derived class.
    /// </summary>
    public abstract Assembly Assembly { get; }

    protected XRefCollection CrossReferences => image.CrossReferences;

    protected BasicBlockCollection BasicBlocks => image.BasicBlocks;

    protected ProcedureCollection Procedures => image.Procedures;

    protected ErrorCollection Errors => image.Errors;

    public abstract void Analyze();

    /// <summary>
    /// Analyzes code starting from the given location. That location
    /// should be the entry point of a procedure, or otherwise the
    /// analysis may not work correctly.
    /// </summary>
    /// <param name="entryPoint">Specifies the location to start analysis.
    /// This location is relative to the beginning of the image.</param>
    /// <param name="entryType">Type of entry, should usually be JMP or
    /// CALL.</param>
    public virtual void Analyze(Address entryPoint, XRefType entryType)
    {
        GenerateBasicBlocks(entryPoint, entryType);
        GenerateControlFlowGraph();
        GenerateProcedures();
        AddBasicBlocksToProcedures();
    }

    /// <summary>
    /// Analyzes code starting from the given location, and create basic
    /// blocks iteratively.
    /// </summary>
    public void GenerateBasicBlocks(Address entryPoint, XRefType entryType)
    {
        var address = entryPoint;

        // Maintain a queue of basic block entry points to analyze. At
        // the beginning, only the user-specified entry point is in the
        // queue. As we encounter b/c/j instructions during the course
        // of analysis, we push the target addresses to the queue of
        // entry points to be analyzed later.
        PriorityQueue<XRef> xrefQueue =
            new(XRef.CompareByPriority);

        // Maintain a list of all procedure calls (with known target)
        // encountered during the analysis. After we finish analyzing
        // all the basic blocks, we update the list of procedures.
        // List<XRef> xrefCalls = new List<XRef>();

        // Create a a dummy xref entry using the user-supplied starting
        // address.
        xrefQueue.Enqueue(new (
            type: entryType,
            source: Address.Invalid,
            target: entryPoint
        ));

        // Analyze each cross reference in order of their priority.
        // In particular, if the xref is an indexed jump, we delay its
        // processing until we have processed all other types of xrefs.
        // This reduces the chance that we process past the end of a
        // jump table.
        while (!xrefQueue.IsEmpty)
        {
            var entry = xrefQueue.Dequeue();

            // Handle jump table entry, whose Target == Invalid.
            if (entry.Type == XRefType.NearIndexedJump)
            {
                System.Diagnostics.Debug.Assert(entry.Target == Address.Invalid);

                // Fill the Target field to make it a static xref.
                entry = ProcessJumpTableEntry(entry, xrefQueue);
                if (entry == null) // end of jump table
                    continue;
            }

            // Skip other dynamic xrefs.
            if (entry.Target == Address.Invalid)
            {
                CrossReferences.Add(entry);
                continue;
            }

            // Process the basic block starting at the target address.
            var block = AnalyzeBasicBlock(entry, xrefQueue);
            if (block != null)
            {
                //int count = block.Length;
                //int baseOffset = PointerToOffset(entry.Target);
                //proc.CodeRange.AddInterval(baseOffset, baseOffset + count);
                //proc.ByteRange.AddInterval(baseOffset, baseOffset + count);
                //for (int j = 0; j < count; j++)
                //{
                //    image[baseOffset + j].Procedure = proc;
                //}
#if false
                proc.AddBasicBlock(block);
#endif
            }
            CrossReferences.Add(entry);
        }
    }

    private BasicBlock AnalyzeBasicBlock(XRef entry, PriorityQueue<XRef> xrefQueue) => throw new NotImplementedException();

    private XRef ProcessJumpTableEntry(XRef entry, PriorityQueue<XRef> xrefQueue) => throw new NotImplementedException();

    /// <summary>
    /// Generates control flow graph from existing xrefs.
    /// </summary>
    protected virtual void GenerateControlFlowGraph()
    {
        foreach (var xref in CrossReferences)
        {
            // Skip xrefs with unknown source (e.g. user-specified entry
            // point) or target (e.g. dynamic call or jump).
            if (xref.Source == Address.Invalid ||
                xref.Target == Address.Invalid)
                continue;
            if (xref.Type == XRefType.NearCall ||
                xref.Type == XRefType.FarCall)
                continue;

            // Find the basic blocks that owns the source location
            // and target location.
            var sourceBlock = BasicBlocks.Find(xref.Source);
            var targetBlock = BasicBlocks.Find(xref.Target);
#if true
            if (sourceBlock == null || targetBlock == null)
            {
                System.Diagnostics.Debug.WriteLine("Cannot find block.");
                continue;
            }
#else
            System.Diagnostics.Debug.Assert(sourceBlock != null);
            System.Diagnostics.Debug.Assert(targetBlock != null);
#endif
            // Create a directed edge from the source basic block to
            // the target basic block.
            BasicBlocks.ControlFlowGraph.AddEdge(
                sourceBlock, targetBlock, xref);
        }
    }

    protected virtual void GenerateProcedures()
    {
        foreach (var xref in CrossReferences)
        {
            var entryPoint = xref.Target;
            if (entryPoint == Address.Invalid)
                continue;
            var callType =
                (xref.Type == XRefType.NearCall) ? CallType.Near :
                (xref.Type == XRefType.FarCall) ? CallType.Far : CallType.Unknown;
            if (callType == CallType.Unknown)
                continue;

            // Create a procedure at this entry point if none exists.
            var proc = Procedures.Find(entryPoint);
            if (proc == null)
            {
                proc = CreateProcedure(entryPoint);
                Procedures.Add(proc);
            }

            // Check the calling type against the procedure's signature.
            if (callType == CallType.Near && proc.ReturnType != ReturnType.Near ||
                callType == CallType.Far && proc.ReturnType != ReturnType.Far)
            {
                AddError(entryPoint, ErrorCode.InconsistentCall,
                    @"Procedure {0} is defined as ""{1}"" but called as ""{2}"".",
                    entryPoint, proc.ReturnType, callType);
            }
        }
    }

    /// <summary>
    /// Creates a procedure with the given entry point.
    /// </summary>
    /// <returns></returns>
    protected virtual Procedure CreateProcedure(Address entryPoint)
    {
        // If there is already a procedure defined at the given entry
        // point, return that procedure.
        var proc = Procedures.Find(entryPoint);
        if (proc != null)
            return proc;

        // Create a procedure at the entry point. The entry point must be
        // be the first byte of a basic block, or otherwise some flow
        // analysis error must have occurred. On the other hand, note
        // that multiple procedures may share one or more basic blocks
        // as part of their implementation.
        proc = new Procedure(entryPoint);
        AddBasicBlocksToProcedure(proc);
        //proc.Name = "TBD";

        // To determine the call type of the procedure, examine the 
        // features of the basic blocks.
        CodeFeatures features = proc.Features;
#if false
        foreach (BasicBlock block in proc.BasicBlocks)
        {
            features |= block.Features;
        }
#endif

        var callFeatures = features & (
            CodeFeatures.HasRETN | CodeFeatures.HasRETF | CodeFeatures.HasIRET);
        switch (callFeatures)
        {
            case CodeFeatures.HasRETN:
                proc.ReturnType |= ReturnType.Near;
                break;
            case CodeFeatures.HasRETF:
                proc.ReturnType |= ReturnType.Far;
                break;
            case CodeFeatures.HasIRET:
                proc.ReturnType |= ReturnType.Interrupt;
                break;
            case CodeFeatures.None:
                AddError(entryPoint, ErrorCode.InconsistentCall,
                    "Procedure at entry point {0} does not contain a RET/RETF/IRET instruction.",
                    entryPoint);
                break;
            default:
                AddError(entryPoint, ErrorCode.InconsistentCall,
                    "Procedure at entry point {0} contains inconsistent return instructions: {1}.",
                    entryPoint, callFeatures);
                break;
        }
        return proc;
    }

    private void GenerateCallGraph()
    {
#if false
        foreach (XRef xref in program.CrossReferences)
        {
            LogicalAddress entryPoint = xref.Target;
            CallType callType =
                (xref.Type == XRefType.NearCall) ?
                CallType.Near : CallType.Far;

            // If there is already a procedure defined at that entry
            // point, perform some sanity checks.
            // TBD: should check and emit a message if two procedures
            // are defined at the same ResolvedAddress but with different
            // logical address.
            Procedure proc = program.Procedures.Find(entryPoint);
            if (proc != null)
            {
                if (proc.CallType != callType)
                {
                    AddError(entryPoint, ErrorCode.InconsistentCall,
                        "Procedure at entry point {0} has inconsistent call type.",
                        entryPoint);
                }
                // add call graph
                continue;
            }

            // Create a procedure at the entry point. The entry point must
            // be the first byte of a basic block, or otherwise some flow
            // analysis error must have occurred. On the other hand, note
            // that multiple procedures may share one or more basic blocks
            // as part of their implementation.
            proc = new Procedure(entryPoint);
            proc.Name = "TBD";
            proc.CallType = callType;

            program.Procedures.Add(proc);
        }
#endif
    }

    /// <summary>
    /// Adds all basic blocks, starting from the procedure's entry point,
    /// to the procedure's owning blocks. Note that multiple procedures
    /// may share one or more basic blocks.
    /// </summary>
    /// <param name="proc"></param>
    /// <param name="xrefs"></param>
    protected virtual void AddBasicBlocksToProcedures()
    {
        foreach (var proc in Procedures)
        {
            AddBasicBlocksToProcedure(proc);
        }
    }

    /// <summary>
    /// Adds all basic blocks, starting from the procedure's entry point,
    /// to the procedure's list of owning blocks. Note that multiple
    /// procedures may share one or more basic blocks.
    /// </summary>
    /// <param name="proc"></param>
    protected virtual bool AddBasicBlocksToProcedure(Procedure proc)
    {
        // TODO: introduce ProcedureAlias, so that we don't need to
        // analyze the same procedure twice.
        var block = BasicBlocks.Find(proc.EntryPoint);
        if (block == null)
            return false;

        Stack<BasicBlock> queue = new();
        queue.Push(block);

        while (queue.Count > 0)
        {
            var parent = queue.Pop();
            if (!proc.BasicBlocks.Contains(parent))
            {
                proc.AddBasicBlock(parent);
                foreach (var child in BasicBlocks.ControlFlowGraph.GetSuccessors(parent))
                {
                    queue.Push(child);
                }
            }
        }
        return true;
    }

    /// <summary>
    /// Fills the Target of an IndexedJump xref heuristically by plugging
    /// in the jump target stored in DataLocation and performing various
    /// sanity checks.
    /// </summary>
    /// <param name="entry">A xref of type IndexedJump whose Target field
    /// is Invalid.</param>
    /// <param name="xrefs">Collection to add a new dynamic IndexedJump
    /// xref to, if any.</param>
    /// <returns>The updated xref, or null if the jump table ends.</returns>
    private XRef ProcessJumpTableEntry(XRef entry, ICollection<XRef> xrefs)
    {
#if true
        return null;
#else
        System.Diagnostics.Debug.Assert(
            entry.Type == XRefType.NearIndexedJump &&
            entry.Target == LogicalAddress.Invalid,
            "Entry must be NearIndexedJump with unknown target");

        // Verify that the location that supposedly stores the jump table
        // entry is not analyzed as anything else. If it is, it indicates
        // that the jump table ends here.
        LinearPointer b = entry.DataLocation.LinearAddress;
        if (image[b].Type != ByteType.Unknown ||
            image[b + 1].Type != ByteType.Unknown)
            return null;

        // If the data location looks like in another segment, stop.
        if (image.LargestSegmentThatStartsBefore(b)
            > entry.Source.Segment)
        {
            return null;
        }

        // TBD: it's always a problem if CS:IP wraps. We need a more
        // general way to detect and fix it. For this particular case,
        // we need to check that the jump target is within the space
        // of this segment.
        if (entry.DataLocation.Offset >= 0xFFFE)
        {
            AddError(entry.DataLocation, ErrorCategory.Error,
                "Jump table is too big (jumped from {0}).",
                entry.Source);
            return null;
        }

        // Find the target address of the jump table entry.
        ushort jumpOffset = image.GetUInt16(b);
        Pointer jumpTarget = new Pointer(entry.Source.Segment, jumpOffset);

        // Check that the target address looks valid. If it doesn't, it
        // probably indicates that the jump table ends here.
        if (!image.IsAddressValid(jumpTarget.LinearAddress))
            return null;

        // If the jump target is outside the range of the current segment
        // but inside the range of a later segment, it likely indicates
        // that the jump table ends here.
        // TBD: this heuristic is kind of a hack... we should do better.
#if true
        if (image.LargestSegmentThatStartsBefore(jumpTarget.LinearAddress)
            > entry.Source.Segment)
        {
            return null;
        }
#endif

        // BUG: We really do need to check that the destination
        // is valid. If not, we should stop immediately.
        if (!(image[jumpTarget].Type == ByteType.Unknown ||
              image[jumpTarget].Type == ByteType.Code &&
              image[jumpTarget].IsLeadByte))
            return null;

        // ...

        // Mark DataLocation as data and add it to the owning procedure's
        // byte range.
        Piece piece = image.CreatePiece(
            entry.DataLocation, entry.DataLocation + 2, ByteType.Data);
        Procedure proc = image[entry.Source].Procedure;
        proc.AddDataBlock(piece.StartAddress, piece.EndAddress);

        // Add a dynamic xref from the JMP instruction to the next jump
        // table entry.
        xrefs.Add(new XRef(
            type: XRefType.NearIndexedJump,
            source: entry.Source,
            target: Pointer.Invalid,
            dataLocation: entry.DataLocation + 2
        ));

        // Return the updated xref with Target field filled.
        return new XRef(
            type: XRefType.NearIndexedJump,
            source: entry.Source,
            target: jumpTarget,
            dataLocation: entry.DataLocation
        );
#endif
    }

    /// <summary>
    /// Gets the image associated with the segment specified by its id.
    /// </summary>
    /// <param name="segmentId">Id of the segment to resolve.</param>
    /// <returns>The image associated with the given segment, or null if
    /// the segment id is invalid.</returns>
    //protected abstract ImageChunk ResolveSegment(int segmentId);

    #region Flow Analysis Methods

    /// <summary>
    /// Analyzes a contiguous sequence of instructions that form a basic
    /// block. A basic block terminates as soon as any of the following
    /// conditions is true:
    /// - An analysis error occurs
    /// - An block terminating instructions: RET, RETF, IRET, HLT.
    /// - A b/c/j instruction: Jcc, JMP, JMPF, LOOPcc.
    /// </summary>
    /// <param name="start">Address to begin analysis.</param>
    /// <param name="xrefs">Collection to add xrefs to.</param>
    /// <returns>
    /// A new BasicBlock if one was created during the analysis.
    /// If no new BasicBlocks are created, or if an existing block was
    /// split into two, returns null.
    /// </returns>
    // TODO: should be roll-back the entire basic block if we 
    // encounters an error on our way? maybe not.
    protected virtual BasicBlock AnalyzeBasicBlock(XRef start, ICollection<XRef> xrefs)
    {
        Address ip = start.Target; // instruction pointer

        if (!image.IsAddressValid(ip))
        {
            AddError(ip, ErrorCode.OutOfImage,
               "XRef target is outside of the image (referred from {0})",
               start.Source);
            return null;
        }

        // Check if the entry address is already analyzed.
        ByteAttribute b = image[ip];
        if (b.Type != ByteType.Unknown)
        {
            // Fail if we ran into data or padding while expecting code.
            if (b.Type != ByteType.Code)
            {
                AddError(ip, ErrorCode.RanIntoData,
                    "XRef target is in the middle of data (referred from {0})",
                    start.Source);
                return null;
            }

            // Now the byte was previously analyzed as code. We must have
            // already created a basic block that contains this byte.
            BasicBlock block = BasicBlocks.Find(ip);
            System.Diagnostics.Debug.Assert(block != null);
            
            // If the existing block starts at this address, we're done.
            if (block.Location == ip)
                return null;

            // TBD: recover the following in some way...
#if false
                if (image[b.BasicBlock.StartAddress].Address.Segment != pos.Segment)
                {
                    AddError(pos, ErrorCategory.Error,
                        "Ran into the middle of a block [{0},{1}) from another segment " +
                        "when processing block {2} referred from {3}",
                        b.BasicBlock.StartAddress, b.BasicBlock.EndAddress,
                        start.Target, start.Source);
                    return null;
                }
#endif

            // Now split the existing basic block into two. This requires
            // that the cut-off point is at instruction boundary.
            if (!b.IsLeadByte)
            {
                AddError(ip, ErrorCode.RanIntoCode,
                    "XRef target is in the middle of an instruction (referred from {0})",
                    start.Source);
                return null;
            }
            BasicBlock[] subBlocks = BasicBlocks.SplitBasicBlock(block, ip, image);

            // Create a xref from the previous block to this block.
            XRef xref = CreateFallThroughXRef(GetLastInstructionInBasicBlock(subBlocks[0]), ip);
            xrefs.Add(xref);

            return null;
        }
        // TODO: Move the above into a separate procedure.

        // Analyze each instruction in sequence until we encounter
        // analyzed code, flow instruction, or an error condition.
        BasicBlockType blockType = BasicBlockType.Unknown;
        while (true)
        {
            // Decode an instruction at this location.
            Address instructionStart = ip;
            Instruction insn = CreateInstruction(ip, start);
            if (insn == null)
            {
                AddError(ip, ErrorCode.BrokenBasicBlock,
                    "Basic block ended prematurally because of invalid instruction.");
                blockType = BasicBlockType.Broken;
                break;
            }
            Address instructionEnd = ip + insn.EncodedLength;

            // Advance the instruction pointer.
            ip = instructionEnd;

            // Determine whether this instruction affects control flow.
            XRefType flowType = GetFlowInstructionType(insn.Operation);

            if (flowType != XRefType.None)
            {
                // Creates an active cross reference if necessary.
                if (NeedsActiveXRef(flowType))
                {
                    XRef xref = CreateFlowXRef(flowType, instructionStart, insn);
                    if (xref != null)
                        xrefs.Add(xref);
                }

                // Creates a fall-through cross reference if necessary.
                if (CanFallThrough(flowType))
                {
                    XRef xref = CreateFallThroughXRef(instructionStart, instructionEnd);
                    xrefs.Add(xref);
                }

                // Terminate the block.
                blockType = GetBasicBlockType(flowType);
                break;
            }

            // If the new location is already analyzed as code, create a
            // control-flow edge from the previous block to the existing
            // block, and we are done.
            if (!image.IsAddressValid(ip))
            {
                blockType = BasicBlockType.Broken;
                break;
            }
            if (image[ip].Type == ByteType.Code)
            {
                System.Diagnostics.Debug.Assert(image[ip].IsLeadByte);

                XRef xref = CreateFallThroughXRef(instructionStart, instructionEnd);
                xrefs.Add(xref);
                blockType = BasicBlockType.FallThrough;
                break;
            }
        }

        // Create a basic block unless we failed on the first instruction.
        if (ip.Offset > start.Target.Offset)
        {
            BasicBlock block = new BasicBlock(start.Target, ip, blockType, image);
            BasicBlocks.Add(block);
        }
        return null;
    }

    /// <summary>
    /// Gets the type of a control-flow affecting instruction. If the
    /// instruction does not affects control flow, returns None.
    /// </summary>
    /// <param name="operation">The operation to examine.</param>
    /// <returns></returns>
    protected XRefType GetFlowInstructionType(Operation operation) => operation switch
    {
        Operation.JO or Operation.JNO or Operation.JB or Operation.JAE or Operation.JE or Operation.JNE or Operation.JBE or Operation.JA or Operation.JS or Operation.JNS or Operation.JP or Operation.JNP or Operation.JL or Operation.JGE or Operation.JLE or Operation.JG or Operation.JCXZ or Operation.LOOP or Operation.LOOPZ or Operation.LOOPNZ => XRefType.ConditionalJump,
        Operation.JMP => XRefType.NearJump,
        Operation.JMPF => XRefType.FarJump,
        Operation.CALL => XRefType.NearCall,
        Operation.CALLF => XRefType.FarCall,
        Operation.RET => XRefType.NearReturn,
        Operation.RETF => XRefType.FarReturn,
        Operation.INT or Operation.INTO => XRefType.Interrupt,
        Operation.IRET => XRefType.InterruptReturn,
        Operation.HLT => XRefType.Halt,
        _ => XRefType.None,
    };

    protected virtual BasicBlockType GetBasicBlockType(XRefType flowType) => flowType switch
    {
        XRefType.ConditionalJump => BasicBlockType.Branch,
        XRefType.NearJump or XRefType.FarJump or XRefType.NearIndexedJump => BasicBlockType.Jump,
        XRefType.Interrupt or XRefType.NearCall or XRefType.FarCall => BasicBlockType.Call,
        XRefType.NearReturn or XRefType.FarReturn or XRefType.InterruptReturn => BasicBlockType.Return,
        XRefType.Halt => BasicBlockType.Halt,
        _ => BasicBlockType.Unknown,
    };

    /// <summary>
    /// Determines whether the given cross reference may possibly fall
    /// through to a "default" flow path such as "branch not taken" or
    /// "return from call".
    /// </summary>
    /// <param name="xrefType">Type of cross reference.</param>
    /// <returns></returns>
    /// <remarks>
    /// This method assumes that the program is well-formed, such that
    /// a "fall-through" path is always possible to be taken.
    ///
    /// In particular, if the instruction is a conditional jump, we
    /// assume that the condition may be true or false, so that both
    /// "jump" and "no jump" is a reachable branch. If the code is
    /// malformed such that one of the branches will never execute, the
    /// analysis may not work correctly.
    ///
    /// Likewise, if the instruction is a procedure call, we assume that
    /// the procedure being called will eventually return. If the
    /// procedure never returns, the analysis may not work correctly.
    /// </remarks>
    protected virtual bool CanFallThrough(XRefType xrefType) => xrefType switch
    {
        XRefType.FarJump or XRefType.NearJump or XRefType.NearIndexedJump or XRefType.FarReturn or XRefType.NearReturn or XRefType.InterruptReturn or XRefType.Halt => false,
        _ => true,
    };

    protected virtual XRef CreateFallThroughXRef(Address source, Address target) => new (
            type: XRefType.FallThrough,
            source: source,
            target: target
            );

    protected virtual bool NeedsActiveXRef(XRefType flowType) => flowType switch
    {
        XRefType.ConditionalJump or XRefType.FarCall or XRefType.NearCall or XRefType.FarJump or XRefType.NearJump or XRefType.NearIndexedJump => true,
        _ => false,
    };

    protected virtual XRef CreateFlowXRef(
        XRefType xrefType, Address source, Instruction instruction) => xrefType switch
        {
            XRefType.ConditionalJump or XRefType.FarJump or XRefType.NearJump or XRefType.FarCall or XRefType.NearCall => CreateBranchJumpCallXRef(xrefType, source, instruction),
            XRefType.NearIndexedJump => throw new NotImplementedException(),
            _ => null,
        };

    protected virtual XRef CreateBranchJumpCallXRef(
        XRefType type, Address source, Instruction instruction)
    {
        Address target = ResolveFlowInstructionTarget(instruction.Operands[0]);
        
        if (target == Address.Invalid)
        {
            AddError(source, ErrorCode.DynamicTarget,
                "Cannot determine the target of {0} instruction.", instruction.Operation);
        }
        return new XRef(
            type: type,
            source: source,
            target: target
        );
    }

    // TODO: add CurrentInstruction, CurrentLocation member variables
    // to be used by derived classes.
    protected virtual Address ResolveFlowInstructionTarget(Operand operand)
    {
        if (operand is RelativeOperand) // near jump/call to relative address
            return ResolveFlowInstructionTarget((RelativeOperand)operand);
        if (operand is PointerOperand) // far jump/call to absolute address
            return ResolveFlowInstructionTarget((PointerOperand)operand);
        if (operand is MemoryOperand) // indirect call/jump
            return ResolveFlowInstructionTarget((MemoryOperand)operand);
        return Address.Invalid;
    }

    protected virtual Address ResolveFlowInstructionTarget(RelativeOperand operand)
    {
        return ((SourceAwareRelativeOperand)operand).Target;
    }

    protected virtual Address ResolveFlowInstructionTarget(PointerOperand operand)
    {
        // Since there's no mapping from absolute frame number to
        // a segment, the base implementation always returns Invalid.
        return Address.Invalid;
    }

    protected virtual Address ResolveFlowInstructionTarget(MemoryOperand operand)
    {
#if false
        // TODO: handle symbolic target.
            MemoryOperand opr = (MemoryOperand)instruction.Operands[0];

            // Handle static near jump table. We recognize a jump table 
            // heuristically if the instruction looks like the following:
            //
            //   jmpn word ptr cs:[bx+3782h] 
            //
            // That is, it meets the requirements that
            //   - the instruction is JMPN
            //   - the jump target is a word-ptr memory location
            //   - the memory location has CS prefix
            //   - a base register (e.g. bx) specifies the entry index
            //
            // Note that a malformed executable may create a jump table
            // not conforming to the above rules, or create a non-jump 
            // table that conforms to the above rules. We do not deal with
            // these cases for the moment.
            if (instruction.Operation == Operation.JMP &&
                opr.Size == CpuSize.Use16Bit &&
                opr.Segment == Register.CS &&
                opr.Base != Register.None &&
                opr.Index == Register.None)
            {
#if false
                return new XRef(
                    type: XRefType.NearIndexedJump,
                    source: start,
                    target: Pointer.Invalid,
                    dataLocation: new Pointer(start.Segment, (UInt16)opr.Displacement.Value)
                );
#else
                return new XRef(
                    type: XRefType.NearJump,
                    source: start,
                    target: Address.Invalid
                    );
#endif
            }
#endif
        return Address.Invalid;
    }

    #endregion

    #region Instruction Decoding Methods

    /// <summary>
    /// Creates an instruction starting at the given address. If the
    /// decoded instruction covers bytes that are already analyzed,
    /// returns null.
    /// </summary>
    /// <param name="image"></param>
    /// <param name="address"></param>
    /// <returns></returns>
    protected virtual Instruction CreateInstruction(Address address, XRef entry)
    {
        Instruction instruction = DecodeInstruction(address);
        if (instruction == null)
            return null;

        // Check that the bytes covered by the decoded instruction are
        // unanalyzed.
        if (!image.CheckByteType(address, address + instruction.EncodedLength, ByteType.Unknown))
        {
            AddError(address, ErrorCode.OverlappingInstruction,
                "Ran into the middle of code when processing block {0} referred from {1}",
                entry.Target, entry.Source);
            return null;
        }

        // Create a code piece for this instruction.
        image.UpdateByteType(address, address + instruction.EncodedLength, ByteType.Code);
        image.Instructions.Add(address, instruction);

        // Return the decoded instruction.
        return instruction;
    }

    /// <summary>
    /// Decodes an instruction at the given offset, applying associated
    /// fix-up information if present.
    /// </summary>
    /// <returns>The decoded instruction.</returns>
    /// <remarks>
    /// When overridden, the method must guarantee that the instruction
    /// is contained in the image, and not into another segment.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">If offset refers to
    /// a location outside of the image.</exception>
    protected virtual Instruction DecodeInstruction(Address address)
    {
        if (!image.IsAddressValid(address))
            throw new ArgumentOutOfRangeException("address");

        Instruction instruction ;
        try
        {
            instruction = X86Codec.Decoder.Decode(
                image.GetBytes(address), CpuMode.RealAddressMode);
        }
        catch (Exception ex)
        {
            AddError(address, ErrorCode.InvalidInstruction, "Bad instruction: {0}", ex.Message);
            return null;
        }

        MakeRelativeOperandSourceAware(instruction, address);

        return instruction;
    }

    /// <summary>
    /// Replaces any RelativeOperand with SourceAwareRelativeOperand.
    /// </summary>
    /// <param name="instruction"></param>
    // TODO: make SourceAwareRelativeOperand.Target a dummy
    // SymbolicTarget, so that we can handle them consistently.
    protected static void MakeRelativeOperandSourceAware(
        Instruction instruction, Address instructionStart)
    {
        for (int i = 0; i < instruction.Operands.Length; i++)
        {
            if (instruction.Operands[i] is RelativeOperand &&
                instruction.Operands[i].Tag == null)
            {
                instruction.Operands[i] = new SourceAwareRelativeOperand(
                    (RelativeOperand)instruction.Operands[i],
                    instructionStart + instruction.EncodedLength);
            }
        }
    }

    #endregion

    private Address GetLastInstructionInBasicBlock(BasicBlock block)
    {
        var ip = block.Bounds.End - 1;
        while (!image[ip].IsLeadByte)
            ip = ip - 1;
        return ip;
    }

    protected void AddError(
        Address location, ErrorCode errorCode,
        string format, params object[] args)
    {
        Errors.Add(new Error(location, errorCode, string.Format(format, args)));
    }
}
