using System;
using System.Collections.Generic;

namespace Disassembler;

public class ExecutableImage : BinaryImage
{
    private readonly byte[] bytes;
    private readonly int[] relocatableLocations;
    private readonly Address entryPoint;

    // Maps a frame number (before relocation) to a segment id.
    private readonly SortedList<UInt16, int> mapFrameToSegment = [];

#if false
    /// <summary>
    /// Creates an executable image with only one segment and no
    /// relocation information. This is used with COM file images.
    /// </summary>
    /// <param name="bytes"></param>
    public ExecutableImage(byte[] bytes)
    {
        if (bytes == null)
            throw new ArgumentNullException(nameof(bytes）);
        if (bytes.Length > 0x10000)
            throw new ArgumentException("Image must not exceed 64KB.");

        this.bytes = bytes;
        this.attrs = new ByteAttribute[bytes.Length];

        // Initialize segmentation info.
        this.relocatableLocations = new int[0];
        //this.segments.Add(0, new DummySegment(null, 0)); // TBD: should be 1000?
    }
#endif

    public ExecutableImage(MZFile file)
    {
        if (file == null)
            throw new ArgumentNullException(nameof(file));

        this.bytes = file.Image;

        // Store relocatable locations for future use.
        List<int> relocs = [];
        foreach (var location in file.RelocatableLocations)
        {
            var index = location.Segment * 16 + location.Offset;
            if (index >= 0 && index < bytes.Length - 1)
                relocs.Add(index);
        }
        relocs.Sort();
        this.relocatableLocations = [.. relocs];

        // Guess segmentation info from the segment values to be
        // relocated. For example, if a relocatable location contains the
        // word 0x1790, it means that 0x1790 will be a segment that will
        // be accessed some time during the program's execution.
        //
        // Although the SEG:OFF addresses of the relocatable locations
        // themselves also provide clue about the program's segmentation,
        // it is less important because they are not directly referenced
        // in the program. Therefore we ignore them for the moment.
        foreach (var index in relocatableLocations)
        {
            var frame = BitConverter.ToUInt16(bytes, index);
            mapFrameToSegment[frame] = -1;
        }
        mapFrameToSegment[file.EntryPoint.Segment] = -1;

        // Create a dummy segment for each of the guessed segments.
        // Initially, we set segment.OffsetCoverage to an empty range to
        // indicate that we have no knowledge about the start and end
        // offset of each segment.
        for (int i = 0; i < mapFrameToSegment.Count; i++)
        {
            var frameNumber = mapFrameToSegment.Keys[i];
            var segment = new ExecutableSegment(this, i, frameNumber);

            // Compute offset bounds for this segment.
            // The lower bound is off course zero.
            // The upper bound is 15 bytes into the next segment.
            var startIndex = frameNumber * 16;
            if (startIndex < bytes.Length)
            {
                var offsetLowerBound = 0;
                var offsetUpperBound = Math.Min(bytes.Length - startIndex, 0x10000);
                if (i < mapFrameToSegment.Count - 1)
                {
                    offsetUpperBound = Math.Min(
                        offsetUpperBound, 
                        mapFrameToSegment.Keys[i + 1] * 16 + 15 - startIndex);
                }
                segment.SetOffsetBounds(
                    new IntRange(offsetLowerBound, offsetUpperBound));
            }

            mapFrameToSegment[frameNumber] = i;
            base.Segments.Add(segment);
        }

        this.entryPoint = new Address(
            MapFrameToSegment(file.EntryPoint.Segment), file.EntryPoint.Offset);
    }

#if false
    public int Length
    {
        get { return bytes.Length; }
    }
#endif

    public int[] RelocatableLocations => relocatableLocations;

    public Address EntryPoint => this.entryPoint;

    public int MapFrameToSegment(UInt16 frameNumber) => mapFrameToSegment[frameNumber];

    public bool IsAddressRelocatable(Address address)
    {
        var index = ToLinearAddress(address);
        return Array.BinarySearch(relocatableLocations, index) >= 0;
    }

    public override string FormatAddress(Address address) => !IsAddressValid(address)
            ? throw new ArgumentOutOfRangeException("address")
            : $"{((ExecutableSegment)base.Segments[address.Segment]).Frame:X4}:{address.Offset:X4}";

    private ExecutableSegment GetSegment(int index) => (ExecutableSegment)base.Segments[index];

    private int ToLinearAddress(Address address)
    {
        var segment = (ExecutableSegment)base.Segments[address.Segment];
        return segment.Frame * 16 + address.Offset;
    }

    protected override void OnBytesAnalyzed(Address startAddress, Address endAddress)
    {
        if (startAddress.Segment != endAddress.Segment)
            throw new ArgumentException();

        ExtendSegmentCoverage(startAddress.Segment, startAddress.Offset, endAddress.Offset);
        base.OnBytesAnalyzed(startAddress, endAddress);
    }

    private void ExtendSegmentCoverage(int segmentIndex, int startOffset, int endOffset)
    {
        var segment = GetSegment(segmentIndex);
        if (segment.OffsetCoverage.IsSupersetOf(new IntRange(startOffset, endOffset)))
            return;

        // Extend the segment's offset coverage.
        if (segment.OffsetCoverage.IsEmpty)
        {
            segment.OffsetCoverage = new IntRange(startOffset, endOffset);
        }
        else
        {
            segment.OffsetCoverage = new IntRange(
                Math.Min(segment.OffsetCoverage.Begin, startOffset),
                Math.Max(segment.OffsetCoverage.End, endOffset));
        }

        // Shrink the offset bounds of its neighboring segments.
        if (segmentIndex > 0)
        {
            var segBefore = GetSegment(segmentIndex - 1);
            var numBytesOverlap = 
                (segBefore.Frame * 16 + segBefore.OffsetBounds.End) -
                (segment.Frame * 16 + segment.OffsetCoverage.Begin);
            if (numBytesOverlap > 0)
            {
                segBefore.SetOffsetBounds(new IntRange(
                    segBefore.OffsetBounds.Begin,
                    segBefore.OffsetBounds.End - numBytesOverlap));
            }
        }
        if (segmentIndex < base.Segments.Count - 1)
        {
            var segAfter = GetSegment(segmentIndex + 1);
            var numBytesOverlap =
                (segment.Frame * 16 + segment.OffsetCoverage.End) -
                (segAfter.Frame * 16 + segAfter.OffsetBounds.Begin);
            if (numBytesOverlap > 0)
            {
                segAfter.SetOffsetBounds(new IntRange(
                    segAfter.OffsetBounds.Begin + numBytesOverlap,
                    segAfter.OffsetBounds.End));
            }
        }
    }

#if false
    public byte[] Data
    {
        get { return bytes; }
    }
#endif

    public override ArraySegment<byte> GetBytes(Address address, int count)
    {
        if (!IsAddressValid(address))
            throw new ArgumentOutOfRangeException("address");

        var index = ToLinearAddress(address);
        return new ArraySegment<byte>(bytes, index, count);
    }
}

/// <summary>
/// Contains information about a segment in an executable file.
/// </summary>
public class ExecutableSegment(ExecutableImage image, int id, UInt16 frameNumber) : Segment
{
    private readonly ExecutableImage image = image;

    public int Id { get; } = id;

    public string Name => Frame.ToString("X4");

    public IntRange OffsetBounds { get; private set; }

    internal void SetOffsetBounds(IntRange bounds)
    {
        this.OffsetBounds = bounds;
    }

    /// <summary>
    /// Gets or sets the range of offsets that are analyzed.
    /// </summary>
    public IntRange OffsetCoverage { get; set; }

    /// <summary>
    /// Gets the frame number of the canonical frame of this segment,
    /// relative to the beginning of the executable image.
    /// </summary>
    public UInt16 Frame { get; } = frameNumber;

    public override string ToString() 
        => $"seg{Id:000}: {Frame:X4}:{OffsetBounds.Begin:X4}-{Frame:X4}:{OffsetBounds.End - 1:X4}";
}
