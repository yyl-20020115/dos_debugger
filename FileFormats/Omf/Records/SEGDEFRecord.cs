using System;
using System.IO;

namespace FileFormats.Omf.Records;

public class SEGDEFRecord : Record
{
    public SegmentDefinition Definition { get; private set; }

    public SEGDEFRecord(RecordReader reader, RecordContext context)
        : base(reader, context)
    {
        SegmentDefinition def = new();

        // Read the record.
        byte acbp = reader.ReadByte();
        def.Alignment = GetAlignment(acbp);
        def.Combination = GetCombination(acbp);
        def.IsUse32 = GetUse32(acbp);

        if (def.Alignment == SegmentAlignment.Absolute)
        {
            def.Frame = reader.ReadUInt16();
            def.Offset = reader.ReadByte();
        }

        UInt32 storedLength=reader.ReadUInt16Or32();
        def.Length = GetLength(acbp, storedLength, reader.RecordNumber);

        UInt16 segmentNameIndex = reader.ReadIndex();
        if (segmentNameIndex > context.Names.Count)
            throw new InvalidDataException("SegmentNameIndex is out of range.");
        if (segmentNameIndex > 0)
            def.SegmentName = context.Names[segmentNameIndex - 1];

        UInt16 classNameIndex = reader.ReadIndex();
        if (classNameIndex > context.Names.Count)
            throw new InvalidDataException("ClassNameIndex is out of range.");
        if (classNameIndex > 0)
            def.ClassName = context.Names[classNameIndex - 1];

        UInt16 overlayNameIndex = reader.ReadIndex();
        if (overlayNameIndex > context.Names.Count)
            throw new InvalidDataException("OverlayNameIndex is out of range.");
        if (overlayNameIndex > 0)
            def.OverlayName = context.Names[overlayNameIndex - 1];

        def.Data = new byte[def.Length];
        //def.Fixups = new List<FixupDefinition>();

        this.Definition = def;
        context.Segments.Add(def);
    }

    private static bool GetUse32(byte acbp) => (acbp & 0x01) != 0;

    private static SegmentAlignment GetAlignment(byte acbp)
    {
        int alignment = acbp >> 5;
        return alignment switch
        {
            0 => SegmentAlignment.Absolute,
            1 => SegmentAlignment.Byte,
            2 => SegmentAlignment.Word,
            3 => SegmentAlignment.Paragraph,
            4 => SegmentAlignment.Page,
            5 => SegmentAlignment.DWord,
            _ => throw new InvalidDataException("Unsupported segment alignment: " + alignment),
        };
    }

    private static SegmentCombination GetCombination(byte acbp)
    {
        int combination = (acbp >> 2) & 7;
        return combination switch
        {
            0 => SegmentCombination.Private,
            2 or 4 or 7 => SegmentCombination.Public,
            5 => SegmentCombination.Stack,
            6 => SegmentCombination.Common,
            _ => throw new InvalidDataException("Unsupported segment combination: " + combination),
        };
    }

    private static long GetLength(byte acbp, UInt32 storedLength, RecordNumber recordNumber) 
        => (acbp & 0x02) != 0 ? recordNumber == RecordNumber.SEGDEF32 ? 0x100000000L : 0x10000 : storedLength;
}
