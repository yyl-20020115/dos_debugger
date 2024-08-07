using System;

namespace Disassembler;

public class LibraryImage : BinaryImage
{
    public LibraryImage() { }

    public LibrarySegment GetSegment(int index) => (LibrarySegment)Segments[index];

    public override ArraySegment<byte> GetBytes(Address address, int count)
    {
        if (!IsAddressValid(address))
            throw new ArgumentOutOfRangeException("address");

        LibrarySegment seg = GetSegment(address.Segment);
        return new ArraySegment<byte>(seg.Data, address.Offset, count);
    }
    
    public override string FormatAddress(Address address)
    {
        return address.Segment < 0 || address.Segment >= Segments.Count
            ? throw new ArgumentOutOfRangeException("address")
            : $"{Segments[address.Segment].Name}+{address.Offset:X4}";
    }
}

public class LibrarySegment(LogicalSegment segment) : Segment
{
    readonly LogicalSegment segment = segment;
    readonly ByteAttribute[] attrs = new ByteAttribute[segment.Data.Length];

    public LogicalSegment Segment => segment;

    public int Length => segment.Data.Length;

    public ByteAttribute[] ByteAttributes => attrs;

    public byte[] Data => segment.Data;

    int Segment.Id => segment.Id;

    string Segment.Name => segment.FullName;

    IntRange Segment.OffsetBounds => new(0, segment.Length);
}
