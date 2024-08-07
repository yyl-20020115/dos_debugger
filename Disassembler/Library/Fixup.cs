using System;
using System.Collections.Generic;
using System.ComponentModel;
using X86Codec;
using FileFormats.Omf;

namespace Disassembler;

/// <summary>
/// Contains information about a fix-up to be applied to a given range of
/// bytes in a binary image.
/// </summary>
[TypeConverter(typeof(ExpandableObjectConverter))]
public class Fixup(int offset = 0)
{
    /// <summary>
    /// Gets or sets the start index to apply the fix-up, relative to the
    /// beginning of an image.
    /// </summary>
    public int StartIndex { get; internal set; } = offset;

    public int EndIndex => StartIndex + Length;

#if false
    /// <summary>
    /// Gets the location to fix up.
    /// </summary>
    public Range<int> Location
    {
        get { return location; }
    }
#endif

    /// <summary>
    /// Gets or sets the type of data to fix in that location.
    /// </summary>
    public FixupLocationType LocationType { get; internal set; }

    /// <summary>
    /// Gets the number of bytes to fix.
    /// </summary>
    public int Length => GetLengthFromLocationType(LocationType);

    /// <summary>
    /// Gets or sets the fix-up mode.
    /// </summary>
    public FixupMode Mode { get; internal set; }

    /// <summary>
    /// Gets or sets the fix-up target.
    /// </summary>
    public SymbolicTarget Target { get; internal set; }

    /// <summary>
    /// Gets or sets the target frame relative to which to apply the
    /// fix up.
    /// </summary>
    public FixupFrame Frame { get; internal set; }

    public override string ToString()
        => $"[{StartIndex:X4},{EndIndex:X4}) => {LocationType} of {Target}";

    /// <summary>
    /// Gets the number of bytes to fix up. This is inferred from the
    /// LocationType property.
    /// </summary>
    private static int GetLengthFromLocationType(FixupLocationType type) => type switch
    {
        FixupLocationType.LowByte => 1,
        FixupLocationType.Offset or FixupLocationType.Base => 2,
        FixupLocationType.Pointer => 4,
        FixupLocationType.Unknown => 0,
        _ => 0,
    };
}

/// <summary>
/// Specifies the type of data to fix up in that location.
/// </summary>
public enum FixupLocationType : byte
{
    /// <summary>The fixup location type is unknown.</summary>
    Unknown,

    /// <summary>
    /// 8-bit displacement or low byte of 16-bit offset.
    /// </summary>
    LowByte,

    /// <summary>16-bit offset.</summary>
    Offset,

    /// <summary>16-bit base.</summary>
    Base,

    /// <summary>32-bit pointer (16-bit base:16-bit offset).</summary>
    Pointer,
}

public class BrokenFixupException(Fixup fixup) : InvalidInstructionException
{
    public Fixup Fixup { get; private set; } = fixup;
}

/// <summary>
/// Provides methods to access the fixups defined on an image chunk.
/// </summary>
public class FixupCollection : IList<Fixup>
{
    readonly List<Fixup> fixups = [];

    public FixupCollection() { }

    /// <summary>
    /// Gets or sets the name for this fixup collection.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Finds the fixup associated with the given position. If no
    /// fixup is found, find the first one that comes after that
    /// position.
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    public int BinarySearch(int offset)
    {
        // fixups.BinarySearch(offset, CompareFixupWithOffset);
        var k = fixups.BinarySearch(new Fixup(offset), new FixupComparer());
        while (k > 0 && CompareFixupWithOffset(fixups[k - 1], offset) == 0)
            k--;
        return k;
    }
    private class FixupComparer : IComparer<Fixup>
    {
        public int Compare(Fixup x, Fixup y)
            => x.StartIndex > y.StartIndex ? 1 : x.EndIndex > y.StartIndex ? 0 : -1;
    }
    private static int CompareFixupWithOffset(Fixup fixup, int offset) 
        => fixup.StartIndex > offset ? 1 : fixup.EndIndex > offset ? 0 : -1;

    public int IndexOf(Fixup item) => throw new NotSupportedException();

    public void Insert(int index, Fixup item) => throw new NotSupportedException();

    public void RemoveAt(int index) => throw new NotSupportedException();

    public Fixup this[int index]
    {
        get => fixups[index];
        set => throw new NotSupportedException();
    }

    public void Add(Fixup fixup)
    {
        if (fixup == null)
            throw new ArgumentNullException("fixup");

        int k = BinarySearch(fixup.StartIndex);
        if (k >= 0) // already exists
        {
            WarnOverlap(fixups[k], fixup);
            return;
        }

        k = ~k;
        if (k > 0 && fixups[k - 1].EndIndex > fixup.StartIndex)
        {
            WarnOverlap(fixups[k - 1], fixup);
            return;
        }
        if (k < fixups.Count && fixup.EndIndex > fixups[k].StartIndex)
        {
            WarnOverlap(fixups[k], fixup);
            return;
        }
        fixups.Insert(k, fixup);
    }

    private void WarnOverlap(Fixup existing, Fixup newone)
    {
        System.Diagnostics.Debug.WriteLine(string.Format(
            "FixupCollection({2}): Overlaps with an existing fixup: existing={0}, new={1}.",
            existing, newone, this.Name));
    }

    public void Clear() => throw new NotSupportedException();

    public bool Contains(Fixup item) => throw new NotSupportedException();

    public void CopyTo(Fixup[] array, int arrayIndex) => fixups.CopyTo(array, arrayIndex);

    public int Count => fixups.Count;

    public bool IsReadOnly => false;

    public bool Remove(Fixup item) => throw new NotSupportedException();

    public IEnumerator<Fixup> GetEnumerator() => fixups.GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
