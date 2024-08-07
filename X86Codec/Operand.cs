﻿using System;

namespace X86Codec;

/// <summary>
/// Represents an operand of an instruction.
/// </summary>
public abstract class Operand
{
    /// <summary>
    /// Gets or sets a user-defined token associated with the operand.
    /// This property is not used by X86Codec assembly.
    /// </summary>
    public object Tag { get; set; }

    /// <summary>
    /// Gets the range that is fixable, relative to the beginning of the
    /// instruction. If this operand does not contain a fixable part,
    /// returns an empty range.
    /// </summary>
    public abstract Location FixableLocation { get; }

    /// <summary>
    /// Represents a location within an instruction, expressed as an
    /// offset to the beginning of the instruction.
    /// </summary>
    public struct Location
    {
        public byte StartOffset { get; private set; }
        public byte Length { get; private set; }

        public Location(byte startOffset, byte length)
            : this()
        {
            StartOffset = startOffset;
            Length = length;
        }
    }

    /// <summary>
    /// Wraps a value of type T with its location within the instruction.
    /// </summary>
    /// <typeparam name="T">Type of the value to wrap.</typeparam>
    /// <remarks>
    /// The location is expressed as a byte offset to the beginning of
    /// the instruction. Depending on the type of operand, the location
    /// may be used in the following ways:
    /// 
    /// Operand Type            Property        Example
    /// ---------------------------------------------------------------
    /// Register                (not used)      XOR   AX, AX
    /// Memory with disp        displacement    LEA   AX, [BX+4]
    /// Memory w/o disp         (not used)      MOV   AX, [BX+BI]
    /// Relative                Offset          JMP   +4
    /// Immediate (explicit)    Immediate       MOV   AX, 20h
    /// Immediate (implicit)    (not used)      SHL   AX, 1
    /// Pointer                 Segment,Offset  CALLF 2920:7654
    /// </remarks>
    public struct LocationAware<T>
    {
        public Location Location { get; private set; }
        public T Value { get; private set; }

        public LocationAware(Location location, T value)
            : this()
        {
            this.Location = location;
            this.Value = value;
        }

        public LocationAware(T value)
            : this()
        {
            this.Value = value;
        }
    }
}

/// <summary>
/// Represents an immediate operand. An immediate may take 8, 16, or 32
/// bits, and is always sign-extended to 32 bits and stored internally.
/// </summary>
public class ImmediateOperand : Operand
{
    readonly LocationAware<int> immediate;
    readonly CpuSize size;

    public LocationAware<int> Immediate => immediate;

    public CpuSize Size => size;

    public ImmediateOperand(LocationAware<int> immediate, CpuSize size)
    {
        this.immediate = immediate;
        this.size = size;
    }

    public ImmediateOperand(int value, CpuSize size)
    {
        this.immediate = new LocationAware<int>(value);
        this.size = size;
    }

    public override Location FixableLocation => immediate.Location;

    public override string ToString() => size switch
    {
        CpuSize.Use8Bit => $"0x{(byte)immediate.Value:X2}",
        CpuSize.Use16Bit => $"0x{(ushort)immediate.Value:X4}",
        _ => $"0x{(uint)immediate.Value:X8}",
    };
}

/// <summary>
/// Represents a register operand.
/// </summary>
public class RegisterOperand : Operand
{
    readonly Register register;

    public Register Register => register;

    public RegisterOperand(Register register) => this.register = register;

    public override Operand.Location FixableLocation => new Location();

    public override string ToString() => register.ToString();
}

/// <summary>
/// Represents a memory address operand of the form
/// [segment:base+index*scaling+displacement].
/// </summary>
public class MemoryOperand : Operand
{
    public CpuSize Size { get; set; } // size of the operand in bytes
    public Register Segment { get; set; }
    public Register Base { get; set; }
    public Register Index { get; set; }

    /// <summary>
    /// Gets or sets the scaling factor. Must be one of 1, 2, 4.
    /// </summary>
    public byte Scaling { get; set; }

    // sign-extended, but should wrap around 0xFFFF.
    public LocationAware<int> Displacement { get; set; }

    public MemoryOperand() => this.Scaling = 1;

    public override Operand.Location FixableLocation => this.Displacement.Location;

    /// <summary>
    /// Converts the operand to a string using the default formatter.
    /// </summary>
    public override string ToString() => InstructionFormatter.Default.FormatOperand(this);
}

/// <summary>
/// Represents an address as a relative offset to EIP.
/// </summary>
public class RelativeOperand(Operand.LocationAware<int> offset) : Operand
{
    public LocationAware<int> Offset { get; private set; } = offset;

    public override Operand.Location FixableLocation => Offset.Location;

    public override string ToString() => Offset.Value.ToString("+#;-#");
}

public class PointerOperand(Operand.LocationAware<UInt16> segment, Operand.LocationAware<UInt32> offset) : Operand
{
    public LocationAware<UInt16> Segment { get; private set; } = segment;
    public LocationAware<UInt32> Offset { get; private set; } = offset;

    public override Operand.Location FixableLocation => new(
                Offset.Location.StartOffset,
                (byte)(Offset.Location.Length + Segment.Location.Length));

    public override string ToString() => $"{Segment.Value:X4}:{Offset.Value:X4}";
}
