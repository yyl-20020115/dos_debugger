using System;
using System.IO;
using System.ComponentModel;

namespace Disassembler;

/// <summary>
/// Contains information of a DOS MZ executable file (.EXE).
/// </summary>
public class MZFile
{
    public readonly string FileName;

    /// <summary>
    /// Loads a DOS MZ executable file from disk.
    /// </summary>
    /// <param name="FileName">Name of the file to open.</param>
    public MZFile(string FileName)
    {
        this.FileName = FileName ?? throw new ArgumentNullException(nameof(FileName));

        using var stream = new FileStream(this.FileName,
            FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new BinaryReader(stream);
        // Read file header.
        this.Header = new MZHeader
        {
            Signature = reader.ReadUInt16(),
            LastPageSize = reader.ReadUInt16(),
            PageCount = reader.ReadUInt16(),
            RelocCount = reader.ReadUInt16(),
            HeaderSize = reader.ReadUInt16(),
            MinAlloc = reader.ReadUInt16(),
            MaxAlloc = reader.ReadUInt16(),
            InitialSS = reader.ReadUInt16(),
            InitialSP = reader.ReadUInt16(),
            Checksum = reader.ReadUInt16(),
            InitialIP = reader.ReadUInt16(),
            InitialCS = reader.ReadUInt16(),
            RelocOff = reader.ReadUInt16(),
            Overlay = reader.ReadUInt16()
        };

        // Verify signature. Both 'MZ' and 'ZM' are allowed.
        if (!(Header.Signature == 0x5A4D || Header.Signature == 0x4D5A))
            throw new InvalidDataException("Signature mismatch.");

        // Calculate the stated size of the executable.
        if (Header.PageCount <= 0)
            throw new InvalidDataException("The PageCount field must be positive.");
        int fileSize = Header.PageCount * 512 -
            (Header.LastPageSize > 0 ? 512 - Header.LastPageSize : 0);

        // Make sure the stated file size is within the actual file size.
        if (fileSize > stream.Length)
            throw new InvalidDataException("The stated file size is larger than the actual file size.");

        // Validate the header size.
        int headerSize = Header.HeaderSize * 16;
        if (headerSize < 28 || headerSize > fileSize)
            throw new InvalidDataException("The stated header size is invalid.");

        // Make sure the relocation table is within the header.
        if (Header.RelocOff < 28 ||
            Header.RelocOff + Header.RelocCount * 4 > headerSize)
        {
            throw new InvalidDataException("The relocation table location is invalid.");
        }

        // Load relocation table.
        RelocatableLocations = new FarPointer[Header.RelocCount];
        stream.Seek(Header.RelocOff, SeekOrigin.Begin);
        for (int i = 0; i < Header.RelocCount; i++)
        {
            UInt16 off = reader.ReadUInt16();
            UInt16 seg = reader.ReadUInt16();
            RelocatableLocations[i] = new FarPointer(seg, off);
        }

        // Load the whole image into memory.
        int imageSize = fileSize - headerSize;
        stream.Seek(headerSize, SeekOrigin.Begin);
        Image = new byte[imageSize];
        stream.Read(Image, 0, Image.Length);
    }

    /// <summary>
    /// Relocates the image to start from the given segment.
    /// </summary>
    /// <param name="segment">The segment to relocate to.</param>
    public void Relocate(UInt16 segment)
    {
        Header.InitialCS += segment;
        Header.InitialSS += segment;
        for (int i = 0; i < RelocatableLocations.Length; i++)
        {
            int address = RelocatableLocations[i].Segment * 16 + RelocatableLocations[i].Offset;
            if (!(address >= 0 && address + 2 <= Image.Length))
                throw new InvalidDataException("The relocation entry is out-of-range.");

            var current = BitConverter.ToUInt16(Image, address);
            current += segment;
            Image[address] = (byte)(current & 0xff);
            Image[address + 1] = (byte)(current >> 8);
        }
        baseAddress.Segment = segment;
    }

    FarPointer baseAddress;

    public FarPointer BaseAddress
    {
        get => baseAddress;
        set
        {
            if (value.Offset != 0)
                throw new ArgumentException("value must have zero offset.");
            Relocate(baseAddress.Segment);
        }
    }

    /// <summary>
    /// Gets the executable image.
    /// </summary>
    [Browsable(false)]
    public byte[] Image { get; }

    /// <summary>
    /// Gets the number of bytes in the executable image.
    /// </summary>
    public int ImageSize => Image.Length;

    /// <summary>
    /// Gets a collection of relocation entries. Each relocation entry is
    /// a far pointer relative to the beginning of the executable image,
    /// which points to a 16-bit word that contains a segment address.
    /// The module loader should add the actual segment to the word at
    /// these locations.
    /// </summary>
    public FarPointer[] RelocatableLocations { get; }

    /// <summary>
    /// Gets the address of the first instruction to execute. This address
    /// is relative to the beginning of the executable image.
    /// </summary>
    public FarPointer EntryPoint => new(Header.InitialCS, Header.InitialIP);

    /// <summary>
    /// Gets the address of the top of the stack. This address is relative
    /// to the beginning of the executable image.
    /// </summary>
    public FarPointer StackTop => new(Header.InitialSS, Header.InitialSP);

    /// <summary>
    /// Gets a copy of the file header.
    /// </summary>
    public MZHeader Header { get; }
}

/// <summary>
/// Represents the file header in a DOS MZ executable.
/// </summary>
[TypeConverter(typeof(ExpandableObjectConverter))]
public class MZHeader
{
    [Description("File format signature; should be MZ or ZM.")]
    public UInt16 Signature { get; set; }

    [Description("Number of bytes in the last page; 0 if the last page is full.")]
    public UInt16 LastPageSize { get; set; }

    [Description("Number of 512-byte pages in the file, including the last page.")]
    public UInt16 PageCount { get; set; }

    [Description("Number of relocation entries; may be 0.")]
    public UInt16 RelocCount { get; set; }

    [Description("Number of 16-byte paragraphs in the header. The executable image starts right after this.")]
    public UInt16 HeaderSize { get; set; }

    [Description("Minimum memory required, in 16-byte paragraphs.")]
    public UInt16 MinAlloc { get; set; }

    [Description("Maximum memory required, in 16-byte paragraphs; usually 0xFFFF.")]
    public UInt16 MaxAlloc { get; set; }

    [Description("Initial initial value of SS; this value must be relocated.")]
    public UInt16 InitialSS { get; set; }

    [Description("Initial value of SP.")]
    public UInt16 InitialSP { get; set; }

    [Description("Checksum of the executable file; usually not used.")]
    public UInt16 Checksum { get; set; }

    [Description("Initial value of IP.")]
    public UInt16 InitialIP { get; set; }

    [Description("Initial value of CS; this value must be relocated.")]
    public UInt16 InitialCS { get; set; }

    [Description("Offset (in bytes) of the relocation table relative to the beginning of the file.")]
    public UInt16 RelocOff { get; set; }

    [Description("Overlay number; usually 0.")]
    public UInt16 Overlay;
}

public struct FarPointer
{
    public UInt16 Offset { get; set; }
    public UInt16 Segment { get; set; }

    public FarPointer(UInt16 segment, UInt16 offset)
        : this()
    {
        this.Segment = segment;
        this.Offset = offset;
    }

    public readonly int LinearAddress => Segment * 16 + Offset;
}
