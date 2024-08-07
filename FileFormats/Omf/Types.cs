﻿using System;
using System.Collections.Generic;

namespace FileFormats.Omf;

public class SegmentDefinition
{
    public string SegmentName { get; set; }
    public string ClassName { get; set; }
    public string OverlayName { get; set; }

    public SegmentAlignment Alignment { get; set; }
    public UInt16 Frame { get; set; }
    public byte Offset { get; set; }

    public SegmentCombination Combination { get; set; }
    public bool IsUse32 { get; set; }
    public long Length { get; set; }

    public byte[] Data;
    public List<FixupDefinition> Fixups = [];
}

public enum SegmentAlignment : int
{
    /// <summary>The alignment is irrelevant or default.</summary>
    Absolute = 0,

    /// <summary>Align on byte boundary.</summary>
    Byte = 1,

    /// <summary>Align on word boundary.</summary>
    Word = 2,

    /// <summary>Align on dword boundary.</summary>
    DWord = 4,

    /// <summary>Align on qword boundary.</summary>
    QWord = 8,

    /// <summary>Align on paragraph (16-byte) boundary.</summary>
    Paragraph = 16,

    /// <summary>Align on page (256-byte) boundary.</summary>
    Page = 256,
}

/// <summary>
/// Specifies how the linker should combine two segments with the same
/// name and class.
/// </summary>
public enum SegmentCombination : byte
{
    /// <summary>
    /// Do not combine the segment with segments from other modules, even
    /// if they have the same segment name and class name.
    /// </summary>
    Private,

    /// <summary>
    /// Concatenates all segments having the same name (and class name)
    /// to form a single, contiguous segment (subject to alignment
    /// requirements). This may leave a gap in between two segments.
    /// </summary>
    Public,

    /// <summary>
    /// Combine by overlapping the two segments. They either have the same
    /// start address or the same end address. The combine segment size is
    /// the larger size of the two.
    /// </summary>
    Common,

    /// <summary>
    /// Concatenates all segments having the same name (and class name)
    /// and causes the operating system to set SS:00 to the bottom and
    /// SS:SP to the top of the resulting segment.
    /// </summary>
    Stack,
}

public class GroupDefinition
{
    public string Name;
    public readonly List<SegmentDefinition> Segments =
        [];
}

public class NameDefinition
{
    public string Name;
    public UInt16 TypeIndex;
    internal Records.RecordNumber DefinedBy;
}

public class ExternalNameDefinition : NameDefinition
{
}

public class PublicNameDefinition : NameDefinition
{
    public bool IsLocal;
    public GroupDefinition BaseGroup;
    public SegmentDefinition BaseSegment;
    public UInt16 BaseFrame;
    public int Offset;
}

public class CommunalNameDefinition : ExternalNameDefinition
{
    public byte DataType;
    public UInt32 ElementCount;
    public UInt32 ElementSize;
}

public class AliasDefinition
{
    public string AliasName;
    public string SubstituteName;
}
