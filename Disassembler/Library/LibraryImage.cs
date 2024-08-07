using System;

namespace Disassembler;

public class LibraryImage : BinaryImage
{
    public LibraryImage()
    {
    }

    public LibrarySegment GetSegment(int index)
    {
        return (LibrarySegment)Segments[index];
    }

    public override ArraySegment<byte> GetBytes(Address address, int count)
    {
        if (!IsAddressValid(address))
            throw new ArgumentOutOfRangeException("address");

        LibrarySegment seg = GetSegment(address.Segment);
        return new ArraySegment<byte>(seg.Data, address.Offset, count);
    }
    
    public override string FormatAddress(Address address)
    {
        if (address.Segment < 0 || address.Segment >= Segments.Count)
            throw new ArgumentOutOfRangeException("address");

        return string.Format("{0}+{1:X4}",
            Segments[address.Segment].Name,
            address.Offset);
    }
}

public class LibrarySegment : Segment
{
    readonly LogicalSegment segment;
    readonly ByteAttribute[] attrs;

    public LibrarySegment(LogicalSegment segment)
    {
        this.segment = segment;
        this.attrs = new ByteAttribute[segment.Data.Length];
    }

    public LogicalSegment Segment => segment;

    public int Length => segment.Data.Length;

    public ByteAttribute[] ByteAttributes => attrs;

    public byte[] Data => segment.Data;

    int Segment.Id => segment.Id;

    string Segment.Name => segment.FullName;

    Range<int> Segment.OffsetBounds => new(0, segment.Length);
}
