using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace FileFormats.Omf.Records;

public enum RecordNumber : byte
{
    None = 0,

    /// <summary>Translator Header Record</summary>
    THEADR = 0x80,

    /// <summary>Library Module Header Record</summary>
    LHEADR = 0x82,

    /// <summary>Comment Record (with extensions)</summary>
    COMENT = 0x88,

    /// <summary>Module End Record (16-bit)</summary>
    MODEND = 0x8A,

    /// <summary>Module End Record (32-bit)</summary>
    MODEND32 = 0x8B,

    /// <summary>External Names Definition Record</summary>
    EXTDEF = 0x8C,

    /// <summary>Public Names Definition Record (16-bit)</summary>
    PUBDEF = 0x90,

    /// <summary>Public Names Definition Record (32-bit)</summary>
    PUBDEF32 = 0x91,

    /// <summary>Line Numbers Record (16-bit)</summary>
    LINNUM = 0x94,

    /// <summary>Line Numbers Record (32-bit)</summary>
    LINNUM32 = 0x95,

    /// <summary>List of Names Record</summary>
    LNAMES = 0x96,

    /// <summary>Segment Definition Record (16-bit)</summary>
    SEGDEF = 0x98,

    /// <summary>Segment Definition Record (32-bit)</summary>
    SEGDEF32 = 0x99,

    /// <summary>Group Definition Record</summary>
    GRPDEF = 0x9A,

    /// <summary>Fixup Record (16-bit)</summary>
    FIXUPP = 0x9C,

    /// <summary>Fixup Record (32-bit)</summary>
    FIXUPP32 = 0x9D,

    /// <summary>Logical Enumerated Data Record (16-bit)</summary>
    LEDATA = 0xA0,

    /// <summary>Logical Enumerated Data Record (32-bit)</summary>
    LEDATA32 = 0xA1,

    /// <summary>Logical Iterated Data Record (16-bit)</summary>
    LIDATA = 0xA2,

    /// <summary>Logical Iterated Data Record (32-bit)</summary>
    LIDATA32 = 0xA3,

    /// <summary>Communal Names Definition Record</summary>
    COMDEF = 0xB0,

    /// <summary>Backpatch Record (16-bit)</summary>
    BAKPAT = 0xB2,

    /// <summary>Backpatch Record (32-bit)</summary>
    BAKPAT32 = 0xB3,

    /// <summary>Local External Names Definition Record (32-bit)</summary>
    LEXTDEF = 0xB4,

    /// <summary>Local External Names Definition Record (32-bit)</summary>
    LEXTDEF32 = 0xB5,

    /// <summary>Local Public Names Definition Record (16-bit)</summary>
    LPUBDEF = 0xB6,

    /// <summary>Local Public Names Definition Record (32-bit)</summary>
    LPUBDEF32 = 0xB7,

    /// <summary>Local Communal Names Definition Record</summary>
    LCOMDEF = 0xB8,

    /// <summary>COMDAT External Names Definition Record</summary>
    CEXTDEF = 0xBC,

    /// <summary>Initialized Communal Data Record (16-bit)</summary>
    COMDAT = 0xC2,

    /// <summary>Initialized Communal Data Record (32-bit)</summary>
    COMDAT32 = 0xC3,

    /// <summary>Symbol Line Numbers Record (16-bit)</summary>
    LINSYM = 0xC4,

    /// <summary>Symbol Line Numbers Record (32-bit)</summary>
    LINSYM32 = 0xC5,

    /// <summary>Alias Definition Record</summary>
    ALIAS = 0xC6,

    /// <summary>Named Backpatch Record (16-bit)</summary>
    NBKPAT = 0xC8,

    /// <summary>Named Backpatch Record (32-bit)</summary>
    NBKPAT32 = 0xC9,

    /// <summary>Local Logical Names Definition Record</summary>
    LLNAMES = 0xCA,

    /// <summary>OMF Version Number Record</summary>
    VERNUM = 0xCC,

    /// <summary>Vendor-specific OMF Extension Record</summary>
    VENDEXT = 0xCE,

    /// <summary>Library Header Record</summary>
    LibraryHeader = 0xF0,

    /// <summary>Library End Record</summary>
    LibraryEnd = 0xF1,
}

[TypeConverter(typeof(ExpandableObjectConverter))]
public abstract class Record
{
    [Browsable(false)]
    public RecordNumber RecordNumber { get; private set; }

    /// <summary>
    /// Gets the position of the record expressed as a byte offset to the
    /// beginning of the OBJ or LIB file.
    /// </summary>
    [Browsable(false)]
    public int Position { get; private set; }

    internal Record(RecordReader reader, RecordContext context)
    {
        this.Position = reader.Position;
        this.RecordNumber = reader.RecordNumber;
    }

    internal static Record ReadRecord(BinaryReader binaryReader, RecordContext context) => ReadRecord(binaryReader, context, RecordNumber.None);

    internal static Record ReadRecord(
        BinaryReader binaryReader, 
        RecordContext context,
        RecordNumber expectedRecord)
    {
        var reader = new RecordReader(binaryReader);
        if (expectedRecord != RecordNumber.None &&
            reader.RecordNumber != expectedRecord)
        {
            throw new InvalidDataException(string.Format(
                "Expecting record {0}, but got record {1}.",
                expectedRecord, reader.RecordNumber));
        }

        Record r = reader.RecordNumber switch
        {
            RecordNumber.LibraryHeader => new LibraryHeaderRecord(reader, context),
            RecordNumber.LibraryEnd => new LibraryEndRecord(reader, context),
            RecordNumber.ALIAS => new ALIASRecord(reader, context),
            RecordNumber.CEXTDEF => new CEXTDEFRecord(reader, context),
            RecordNumber.COMDAT or RecordNumber.COMDAT32 => new COMDATRecord(reader, context),
            RecordNumber.COMDEF => new COMDEFRecord(reader, context),
            RecordNumber.COMENT => new CommentRecord(reader, context),
            RecordNumber.EXTDEF => new EXTDEFRecord(reader, context),
            RecordNumber.FIXUPP or RecordNumber.FIXUPP32 => new FixupRecord(reader, context),
            RecordNumber.GRPDEF => new GRPDEFRecord(reader, context),
            RecordNumber.LCOMDEF => new LCOMDEFRecord(reader, context),
            RecordNumber.LEDATA or RecordNumber.LEDATA32 => new LEDATARecord(reader, context),
            RecordNumber.LEXTDEF or RecordNumber.LEXTDEF32 => new LEXTDEFRecord(reader, context),
            RecordNumber.LHEADR => new LHEADRRecord(reader, context),
            RecordNumber.LIDATA or RecordNumber.LIDATA32 => new LIDATARecord(reader, context),
            RecordNumber.LNAMES => new ListOfNamesRecord(reader, context),
            RecordNumber.LPUBDEF or RecordNumber.LPUBDEF32 => new LPUBDEFRecord(reader, context),
            RecordNumber.MODEND => new MODENDRecord(reader, context),
            RecordNumber.PUBDEF or RecordNumber.PUBDEF32 => new PUBDEFRecord(reader, context),
            RecordNumber.SEGDEF or RecordNumber.SEGDEF32 => new SEGDEFRecord(reader, context),
            RecordNumber.THEADR => new THEADRRecord(reader, context),
            _ => new UnknownRecord(reader, context),
        };

        // TODO: check all bytes are consumed.
        // ...

        // Update RecordContext.LastRecord. This is necessary so that
        // a FIXUPP record knows which record to fix up.
        if (context != null)
        {
            context.LastRecord = r;
        }

        return r;
    }

    public override string ToString() => $"{RecordNumber} @ {Position:X}";
}

/// <summary>
/// Represents a record with an unknown record number.
/// </summary>
class UnknownRecord : Record
{
    public byte[] Data { get; private set; }

    public UnknownRecord(RecordReader reader, RecordContext context)
        : base(reader, context)
    {
        this.Data = reader.Data;
    }

    public override string ToString() => $"? {RecordNumber} @ {Position:X}";
}

class LibraryHeaderRecord(RecordReader reader, RecordContext context) : Record(reader, context)
{
    /// <summary>
    /// Gets the size, in bytes, of a page in the library file. Each
    /// object module in the library file must be aligned on page
    /// boundary.
    /// </summary>
    public int PageSize { get; private set; } = reader.Data.Length + 4;
}

class LibraryEndRecord(RecordReader reader, RecordContext context) : Record(reader, context)
{
}

/// <summary>
/// Translator Header Record -- contains the name of the source file that
/// defines this object module. Mutliple THEADR records may be present in
/// an object module (e.g. if the object is compiled from multiple source
/// files).
/// </summary>
class THEADRRecord : Record
{
    public string Name { get; private set; }

    public THEADRRecord(RecordReader reader, RecordContext context)
        : base(reader, context)
    {
        this.Name = reader.ReadPrefixedString();
        context.SourceName = Name;
    }
}

/// <summary>
/// Library Module Header Record -- contains the name of a module within
/// a library file. This record is not used by MS LINK.
/// </summary>
class LHEADRRecord : Record
{
    public string Name { get; private set; }

    public LHEADRRecord(RecordReader reader, RecordContext context)
        : base(reader, context)
    {
        this.Name = reader.ReadPrefixedString();
    }
}

/// <summary>
/// Module End Record -- denotes the end of an object module.
/// </summary>
class MODENDRecord : Record
{
    public bool IsMainModule { get; private set; }
    public bool IsStartAddressPresent { get; private set; }
    public bool IsStartAddressRelocatable { get; private set; }

    public MODENDRecord(RecordReader reader, RecordContext context)
        : base(reader, context)
    {
        byte type = reader.ReadByte();
        this.IsMainModule = (type & 0x80) != 0;
        this.IsStartAddressPresent = (type & 0x40) != 0;
        this.IsStartAddressRelocatable = (type & 0x01) != 0;

        // TODO: read the start address field...
    }
}

#region External Name Related Records

/// <summary>
/// External Names Definition Record -- contains a list of symbolic
/// external references, i.e. references to symbols defined in other
/// object modules.
/// </summary>
/// <remarks>
/// EXTDEF names are ordered by occurrence jointly with the COMDEF and
/// LEXTDEF records, and referenced by an index in FIXUPP records.
/// 
/// The linker resolves external references by matching the symbols
/// declared in EXTDEF records with symbols declared in PUBDEF records.
/// </remarks>
class EXTDEFRecord : Record
{
    public ExternalNameDefinition[] Definitions { get; private set; }

    public EXTDEFRecord(RecordReader reader, RecordContext context)
        : base(reader, context)
    {
        int startIndex = context.ExternalNames.Count;
        while (!reader.IsEOF)
        {
            ExternalNameDefinition def = new ExternalNameDefinition();
            def.Name = reader.ReadPrefixedString();
            def.TypeIndex = reader.ReadIndex();
            def.DefinedBy = reader.RecordNumber;
            context.ExternalNames.Add(def);
        }
        int endIndex = context.ExternalNames.Count;
        this.Definitions = context.ExternalNames.GetRange(
            startIndex , (endIndex - startIndex)).ToArray();
    }
}

/// <summary>
/// Local External Names Definition Record -- identical to EXTDEF records
/// except that the names defined in LEXTDEF records are visible only in
/// the module where they are defined.
/// </summary>
/// <remarks>
/// LEXTDEF records are associated with LPUBDEF and LCOMDEF records and
/// ordered with EXTDEF and COMDEF records by occurrence, so that they
/// may be referenced by an external name index for fixups.
/// </remarks>
class LEXTDEFRecord : EXTDEFRecord
{
    public LEXTDEFRecord(RecordReader reader, RecordContext context)
        : base(reader, context)
    {
    }
}

/// <summary>
/// COMDAT External Names Definition Record -- serves the same purpose as
/// the EXTDEF record, except that the name referenced are defined in
/// COMDAT records.
/// </summary>
/// <remarks>
/// A CEXTDEF can precede the COMDAT to which it will be resolved. In this
/// case, the location of the COMDAT is not known at the time the CEXTDEF
/// is seen.
/// 
/// This record is produced when a FIXUPP record refers to a COMDAT
/// symbol.
/// </remarks>
class CEXTDEFRecord : Record
{
    public ExternalNameDefinition[] Definitions { get; private set; }

    public CEXTDEFRecord(RecordReader reader, RecordContext context)
        : base(reader, context)
    {
        int startIndex = context.ExternalNames.Count;
        while (!reader.IsEOF)
        {
            UInt16 nameIndex = reader.ReadIndex();
            if (nameIndex == 0 || nameIndex > context.Names.Count)
            {
                throw new InvalidDataException("LogicalNameIndex is out of range.");
            }
            UInt16 typeIndex = reader.ReadIndex();

            var def = new ExternalNameDefinition
            {
                Name = context.Names[nameIndex - 1],
                TypeIndex = typeIndex,
                DefinedBy = reader.RecordNumber
            };
            context.ExternalNames.Add(def);
        }
        int endIndex = context.ExternalNames.Count;
        this.Definitions = context.ExternalNames.GetRange(
            startIndex, endIndex - startIndex).ToArray();
    }
}

#endregion

/// <summary>
/// Public Names Definition Record -- defines public symbols in this
/// object module. The symbols are also available for export if so
/// indicated in an EXPDEF comment record.
/// </summary>
/// <remarks>
/// All defined functions and initialized global variables generate
/// PUBDEF records in most compilers.
/// </remarks>
class PUBDEFRecord : Record
{
    public SegmentDefinition BaseSegment { get; private set; }
    public GroupDefinition BaseGroup { get; private set; }
    public UInt16 BaseFrame { get; private set; }
    public PublicNameDefinition[] Definitions { get; private set; }

    public PUBDEFRecord(RecordReader reader, RecordContext context)
        : base(reader, context)
    {
        int baseGroupIndex = reader.ReadIndex();
        if (baseGroupIndex > context.Groups.Count)
            throw new InvalidDataException("GroupIndex is out of range.");
        if (baseGroupIndex > 0)
            this.BaseGroup = context.Groups[baseGroupIndex - 1];

        int baseSegmentIndex = reader.ReadIndex();
        if (baseSegmentIndex > context.Segments.Count)
            throw new InvalidDataException("SegmentIndex is out of range.");
        if (baseSegmentIndex == 0)
            this.BaseFrame = reader.ReadUInt16();
        else
            this.BaseSegment = context.Segments[baseSegmentIndex - 1];

        int startIndex = context.PublicNames.Count;
        while (!reader.IsEOF)
        {
            var def = new PublicNameDefinition
            {
                DefinedBy = reader.RecordNumber,
                BaseGroup = BaseGroup,
                BaseSegment = BaseSegment,
                BaseFrame = BaseFrame,
                Name = reader.ReadPrefixedString(),
                Offset = (int)reader.ReadUInt16Or32(),
                TypeIndex = reader.ReadIndex()
            };
            context.PublicNames.Add(def);
        }
        int endIndex = context.PublicNames.Count;
        this.Definitions = context.PublicNames.GetRange(
            startIndex, endIndex - startIndex).ToArray();
    }
}

class LPUBDEFRecord(RecordReader reader, RecordContext context) : PUBDEFRecord(reader, context)
{
}

/// <summary>
/// Communal Names Definition Record -- declares a list of communal
/// variables (uninitialized static data or data that may match
/// initialized static data in another compilation unit).
/// </summary>
class COMDEFRecord : Record
{
    public ExternalNameDefinition[] Definitions { get; private set; }

    public COMDEFRecord(RecordReader reader, RecordContext context)
        : base(reader, context)
    {
        int startIndex = context.ExternalNames.Count;
        while (!reader.IsEOF)
        {
            var def = new CommunalNameDefinition
            {
                DefinedBy = reader.RecordNumber,
                Name = reader.ReadPrefixedString(),
                TypeIndex = reader.ReadIndex(),
                DataType = reader.ReadByte(),
                ElementCount = ReadEncodedInteger(reader)
            };
            if (def.DataType == 0x61) // FAR data: count, elemsize
                def.ElementSize = ReadEncodedInteger(reader);
            else
                def.ElementSize = 1;
            context.ExternalNames.Add(def);
        }
        int endIndex = context.ExternalNames.Count;
        this.Definitions = context.ExternalNames.GetRange(
            startIndex, endIndex - startIndex).ToArray();
    }

    private static UInt32 ReadEncodedInteger(RecordReader reader)
    {
        var b = reader.ReadByte();
        return b == 0x81 ? reader.ReadUInt16() : b == 0x84 ? reader.ReadUInt24() : b == 0x88 ? reader.ReadUInt32() : b;
    }
}

class LCOMDEFRecord(RecordReader reader, RecordContext context) : COMDEFRecord(reader, context)
{
}

/// <summary>
/// Defines a list of names that can be referenced by subsequent records
/// in the object module.
/// </summary>
public class ListOfNamesRecord : Record
{
    public string[] Names { get; private set; }

    internal ListOfNamesRecord(RecordReader reader, RecordContext context)
        : base(reader, context)
    {
        List<string> names = [];
        while (!reader.IsEOF)
        {
            names.Add(reader.ReadPrefixedString());
        }
        this.Names = names.ToArray();
        context.Names.AddRange(Names);
    }
}

class GRPDEFRecord : Record
{
    public GroupDefinition Definition { get; private set; }

    public GRPDEFRecord(RecordReader reader, RecordContext context)
        : base(reader, context)
    {
        this.Definition = new GroupDefinition();

        UInt16 groupNameIndex = reader.ReadIndex();
        if (groupNameIndex == 0 || groupNameIndex > context.Names.Count)
        {
            throw new InvalidDataException("GroupNameIndex is out of range.");
        }
        this.Definition.Name = context.Names[groupNameIndex - 1];

        while (!reader.IsEOF)
        {
            reader.ReadByte(); // 'type' ignored
            UInt16 segmentIndex = reader.ReadIndex();
            if (segmentIndex == 0 || segmentIndex > context.Segments.Count)
            {
                throw new InvalidDataException("SegmentIndex is out of range.");
            }
            this.Definition.Segments.Add(context.Segments[segmentIndex - 1]);
        }

        context.Groups.Add(Definition);
    }
}

/// <summary>
/// Contains contiguous binary data to be copied into the program's
/// executable binary image.
/// </summary>
class LEDATARecord : Record
{
    public SegmentDefinition Segment { get; private set; }
    public UInt32 DataOffset { get; private set; }
    public byte[] Data { get; private set; }

    public LEDATARecord(RecordReader reader, RecordContext context)
        : base(reader, context)
    {
        UInt16 segmentIndex = reader.ReadIndex();
        if (segmentIndex == 0 || segmentIndex > context.Segments.Count)
        {
            throw new InvalidDataException("SegmentIndex is out of range.");
        }
        this.Segment = context.Segments[segmentIndex - 1];

        this.DataOffset = reader.ReadUInt16Or32();
        this.Data = reader.ReadToEnd(); // TBD: this can be optimized to
                                        // reduce extra data copy

        // Fill the segment's data.
        if (Data.Length + DataOffset > Segment.Length)
            throw new InvalidDataException("The LEDATA overflows the segment.");

        Array.Copy(Data, 0, Segment.Data, DataOffset, Data.Length);
    }
}

/// <summary>
/// Contains contiguous binary data to be copied into the program's
/// executable binary image. The data is stored as a repeating pattern.
/// </summary>
class LIDATARecord : Record
{
    public UInt16 SegmentIndex { get; private set; }
    public UInt32 DataOffset { get; private set; }
    public byte[] Data { get; private set; }

    public LIDATARecord(RecordReader reader, RecordContext context)
        : base(reader, context)
    {
        this.SegmentIndex = reader.ReadIndex();
        if (SegmentIndex == 0 || SegmentIndex > context.Segments.Count)
            throw new InvalidDataException("SegmentIndex is out of range.");

        this.DataOffset = reader.ReadUInt16Or32();
        this.Data = reader.ReadToEnd();

        // TODO: parse LIDATA (recursive; a bit messy)
    }
}

class COMDATRecord(RecordReader reader, RecordContext context) : Record(reader, context)
{
}

class ALIASRecord : Record
{
    public AliasDefinition[] Definitions { get; private set; }

    public ALIASRecord(RecordReader reader, RecordContext context)
        : base(reader, context)
    {
        int startIndex = context.Aliases.Count;
        while (!reader.IsEOF)
        {
            AliasDefinition def = new()
            {
                AliasName = reader.ReadPrefixedString(),
                SubstituteName = reader.ReadPrefixedString()
            };
            context.Aliases.Add(def);
        }
        int endIndex = context.Aliases.Count;
        this.Definitions = context.Aliases.GetRange(startIndex,( endIndex - startIndex)).ToArray();
    }
}
