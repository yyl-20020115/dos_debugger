using System;

namespace Disassembler;

public class Range<T>(T Begin, T End) where T : struct
{
    public readonly T Begin = Begin;
    public readonly T End = End;

    public bool IsEmpty => object.Equals(this.Begin, this.End);

    public bool Contains(T address)
    {
        throw new NotImplementedException();
    }

    public bool IsSupersetOf(Range<int> range)
    {
        throw new NotImplementedException();
    }
}