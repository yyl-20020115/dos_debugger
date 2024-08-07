namespace Disassembler;

public abstract class Range<T>(T Begin, T End) where T : struct
{
    public readonly T Begin = Begin;
    public readonly T End = End;

    public bool IsEmpty => object.Equals(this.Begin, this.End);

    public abstract bool Contains(T address);

    public abstract bool IsSupersetOf(Range<T> range);
}

public class IntRange(int Begin,int End) : Range<int>(Begin, End)
{
    public override bool Contains(int address) => address >= this.Begin && address < this.End;
    public override bool IsSupersetOf(Range<int> range) => this.Begin >= range.Begin && this.End <= range.End;
}
public class AddressRange(Address Begin,Address End) : Range<Address>(Begin, End)
{
    public override bool Contains(Address address) 
        => this.Begin.Offset <= address.Offset && this.End.Offset >= address.Offset;
    public override bool IsSupersetOf(Range<Address> range)
        => this.Begin.Offset <= range.Begin.Offset && this.End.Offset >= range.End.Offset;
}