using System;
using System.ComponentModel;
using X86Codec;

namespace Disassembler;
/// <summary>
/// Represents a target (typically a jump target) that is a symbol that
/// must be resolved at link-time, or an address with a known label.
/// </summary>
/// <remarks>
/// Examples:
/// mov ax, seg CODE    ; the second operand is the frame number of the
///                     ; segment named "CODE"
/// mov ax, seg DGROUP  ; the second operand is the frame number of the
///                     ; group named "DGROUP"
/// call strcpy         ; the symbol is the entry point offset of an
///                     ; subroutine
/// callf malloc        ; the symbol is the entry point seg:off of a
///                     ; far proc
/// jmp [bx+_Table]     ; the symbol is the offset of a jump table
/// </remarks>
public class SymbolicTarget
{
    /// <summary>
    /// Gets or sets the referent of the target. The target is specified
    /// by referent + displacement.
    /// </summary>
    public IAddressReferent Referent { get; set; }

    /// <summary>
    /// Gets or sets the displacement of the target relative to the
    /// referent.
    /// </summary>
    public UInt32 Displacement { get; set; }

    public override string ToString() => Displacement == 0 ? Referent.Label : $"{Referent.Label}+{Displacement:X4}";

#if false
    public override string ToString()
    {
        switch (Method)
        {
            case FixupTargetMethod.Absolute:
                return string.Format("{0:X4}:{1:X4}", IndexOrFrame, Displacement);
            case FixupTargetMethod.SegmentPlusDisplacement:
                return string.Format("SEG({0})+{1:X}H", IndexOrFrame, Displacement);
            case FixupTargetMethod.GroupPlusDisplacement:
                return string.Format("GRP({0})+{1:X}H", IndexOrFrame, Displacement);
            case FixupTargetMethod.ExternalPlusDisplacement:
                return string.Format("EXT({0})+{1:X}H", IndexOrFrame, Displacement);
            case FixupTargetMethod.SegmentWithoutDisplacement:
                return string.Format("SEG({0})", IndexOrFrame);
            case FixupTargetMethod.GroupWithoutDisplacement:
                return string.Format("GRP({0})", IndexOrFrame);
            case FixupTargetMethod.ExternalWithoutDisplacement:
                return string.Format("EXT({0})", IndexOrFrame);
            default:
                return "(invalid)";
        }
    }
#endif
}

/// <summary>
/// Represents an object of which part is fixed up.
/// </summary>
public interface IFixedSource
{
    SymbolicTarget GetFixedTarget();
}

#if false
/// <summary>
/// Represents a relative operand (used in branch/call/jump instructions)
/// where the target is the offset of a symbol (typicaly defined in the
/// same segment as the instruction).
/// </summary>
public class SymbolicRelativeOperand : RelativeOperand
{
    public SymbolicTarget Target { get; private set; }
    
    public SymbolicRelativeOperand(SymbolicTarget target)
        
    {
        this.Target = target;
    }

    public override string ToString()
    {
        // We should take an argument to specify whether to return
        // html.
#if true
        return string.Format("<a href=\"somewhere\">{0}</a>", Target.ToString());
#else
        return Target.TargetName;
#endif
    }
}
#endif

#if false
public class SymbolicImmediateOperand : ImmediateOperand
{
    public SymbolicTarget Target { get; private set; }

    public SymbolicImmediateOperand(ImmediateOperand opr, SymbolicTarget target)
        : base(opr.Immediate, opr.Size)
    {
        this.Target = target;
    }
}
#endif

public class SymbolicMemoryOperand : MemoryOperand, IFixedSource
{
    public SymbolicTarget Target { get; private set; }

    public SymbolicMemoryOperand(MemoryOperand opr, SymbolicTarget target)
    {
        base.Base = opr.Base;
        base.Displacement = opr.Displacement;
        base.Index = opr.Index;
        base.Scaling = opr.Scaling;
        base.Segment = opr.Segment;
        base.Size = opr.Size;

        this.Target = target;
    }

    public SymbolicTarget GetFixedTarget() => this.Target;
}

#if false
public class SymbolicPointerOperand :Operand
{
    public SymbolicTarget Target { get; private set; }

    public SymbolicPointerOperand(SymbolicTarget target)
    {
        this.Target = target;
    }

    public override string ToString()
    {
        // We should take an argument to specify whether to return
        // html.
#if true
        return string.Format("<a href=\"somewhere\">{0}</a>", Target.ToString());
#else
        return Target.TargetName;
#endif
    }
}
#endif

public class SourceAwareRelativeOperand(RelativeOperand opr, Address source) : RelativeOperand(opr.Offset)
{
    readonly Address source = source;

    public Address Source => source;

    public Address Target => new Address(source.Segment, (UInt16)(source.Offset + base.Offset.Value));

    public override string ToString() => Target.Offset.ToString("X4");
}

public class SymbolicInstructionFormatter : InstructionFormatter
{
    public override string FormatOperand(Operand operand) 
        => operand is SourceAwareRelativeOperand operand1 &&
            operand.Tag == null
            ? FormatOperand(operand1)
            : base.FormatOperand(operand);

    protected override string FormatFixableLocation(Operand operand) => operand.FixableLocation.Length switch
    {
        > 0 when operand.Tag is SymbolicTarget target => string.Format(
            "<a href=\"somewhere\">{0}</a>",
            target),
        _ => base.FormatFixableLocation(operand)
    };

    public virtual string FormatOperand(SourceAwareRelativeOperand operand) => $"<a href=\"somewhere\">{operand.Target.Offset:X4}</a>";

    public override string FormatInstruction(Instruction instruction)
    {
        var s = base.FormatInstruction(instruction);

        // Make "interesting" instructions bold.
        return instruction.Operation switch
        {
            //case Operation.CALL:
            //case Operation.CALLF:
            //    return string.Format("<b>{0}</b>", s);
            _ => s,
        };
    }

    public override string FormatMnemonic(Operation operation)
    {
        var attribute = operation.GetAttribute<DescriptionAttribute>();
        if (attribute != null)
        {
            string description = attribute.Description;
            return string.Format("<span title=\"{2}: {0}\">{1}</span>",
                attribute.Description.EscapeXml(),
                operation.ToString().ToLowerInvariant(),
                operation);
        }
        return base.FormatMnemonic(operation);
    }
}
