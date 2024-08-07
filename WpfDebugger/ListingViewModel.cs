﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Windows.Media;
using Disassembler;
using X86Codec;

namespace WpfDebugger;

/// <summary>
/// Represents the view model of ASM listing.
/// </summary>
public class ListingViewModel
{
    readonly List<ListingRow> rows = [];
    readonly List<ProcedureItem> procItems = [];
    //readonly List<SegmentItem> segmentItems = new List<SegmentItem>();
    //private Disassembler16 dasm;
    //private BinaryImage image;
    readonly BinaryImage image;

    /// <summary>
    /// Array of the address of each row. This array is used to speed up
    /// row lookup. While this information can be obtained from the rows
    /// collection itself, using a separate array has two benefits:
    /// 1, it utilizes BinarySearch() without the need to create a dummy
    ///    ListingRow object or a custom comparer;
    /// 2, it saves extra memory indirections and is thus faster.
    /// The cost is of course a little extra memory footprint.
    /// </summary>
    private int[] rowAddresses; // rename to rowOffsets

    public ListingViewModel(Assembly assembly, int segmentId)
    {
        this.image = assembly.GetImage();
        
        // Make a list of the errors in this segment. Ideally we should
        // put this logic into ErrorCollection. But for convenience we
        // leave it here for the moment.
        List<Error> errors =
            (from error in assembly.GetImage().Errors
             where error.Location.Segment == segmentId
             orderby error.Location
             select error).ToList();
        int iError = 0;

        // Find the segment. 
        // Todo: we should provide a way to do this.
        Segment segment = null;
        foreach (Segment seg in image.Segments)
        {
            if (seg.Id == segmentId)
            {
                segment = seg;
                break;
            }
        }

        // Display analyzed code and data.
        // TODO: a segment may not start at zero.
        Address address = new Address(segmentId, segment.OffsetBounds.Begin);
        while (image.IsAddressValid(address))
        {
            ByteAttribute b = image[address];

            while (iError < errors.Count && errors[iError].Location.Offset <= address.Offset)
            {
                rows.Add(new ErrorListingRow(assembly, errors[iError++]));
            }

            if (IsLeadByteOfCode(b))
            {
#if false
                if ( b.BasicBlock != null && b.BasicBlock.Location.Offset == i)
                {
                    rows.Add(new LabelListingRow(0, b.BasicBlock));
                }
#endif

                Instruction insn = image.Instructions.Find(address);
                System.Diagnostics.Debug.Assert(insn != null);
                rows.Add(new CodeListingRow(
                    assembly, address, insn, 
                    image.GetBytes(address, insn.EncodedLength).ToArray()));

                address += insn.EncodedLength; // TODO: handle wrapping
            }
            else if (IsLeadByteOfData(b))
            {
                Address j = address + 1;
                while (image.IsAddressValid(j) &&
                       image[j].Type == ByteType.Data &&
                       !image[j].IsLeadByte)
                    j += 1;

                int count = j.Offset - address.Offset;
                rows.Add(new DataListingRow(
                    assembly, address,
                    image.GetBytes(address, count).ToArray()));
                address = j; // TODO: handle wrapping
            }
            else
            {
                //if (errorMap.ContainsKey(i))
                {
                    //    rows.Add(new ErrorListingRow(errorMap[i]));
                }
                Address j = address + 1;
                while (image.IsAddressValid(j) &&
                       !IsLeadByteOfCode(image[j]) &&
                       !IsLeadByteOfData(image[j]))
                    j += 1;

                int count = j.Offset - address.Offset;
                rows.Add(new BlankListingRow(
                    assembly, address, 
                    image.GetBytes(address, count).ToArray()));
                address = j; // TODO: handle wrapping
#if false
                try
                {
                    address = address.Increment(j - i);
                }
                catch (AddressWrappedException)
                {
                    address = Address.Invalid;
                }
#endif
            }
        }

        while (iError < errors.Count)
        {
            rows.Add(new ErrorListingRow(assembly, errors[iError++]));
        }

        // Create a sorted array containing the address of each row.
        rowAddresses = new int[rows.Count];
        for (int i = 0; i < rows.Count; i++)
        {
            rowAddresses[i] = rows[i].Location.Offset;
        }

#if false
        // Create a ProcedureItem view object for each non-empty
        // procedure.
        // TODO: display an error for empty procedures.
        foreach (Procedure proc in image.Procedures)
        {
            if (proc.IsEmpty)
                continue;

            ProcedureItem item = new ProcedureItem(proc);
            //var range = proc.Bounds;
            //item.FirstRowIndex = FindRowIndex(range.Begin);
            //item.LastRowIndex = FindRowIndex(range.End - 1);
            
            // TBD: need to check broken instruction conditions
            // as well as leading/trailing unanalyzed bytes.
            procItems.Add(item);
        }

        // Create segment items.
        foreach (Segment segment in image.Segments)
        {
            segmentItems.Add(new SegmentItem(segment));
        }
#endif
    }

    private static bool IsLeadByteOfCode(ByteAttribute b) => (b.Type == ByteType.Code && b.IsLeadByte);

    private static bool IsLeadByteOfData(ByteAttribute b) => (b.Type == ByteType.Data && b.IsLeadByte);

    public BinaryImage Image => image;

    public List<ListingRow> Rows => rows;

#if false
    /// <summary>
    /// Finds the row that covers the given address. If no row occupies
    /// that address, finds the closest row.
    /// </summary>
    /// <param name="address">The address to find.</param>
    /// <returns>ListingRow, or null if the view is empty.</returns>
    public int FindRowIndex(Pointer address)
    {
        return FindRowIndex(address.LinearAddress);
    }
#endif

    /// <summary>
    /// Finds the first row that covers the given address.
    /// </summary>
    /// <param name="address">The address to find.</param>
    /// <returns>Index of the row found, which may be rows.Count if the
    /// address is past the end of the list.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The view model is
    /// empty, or address is smaller than the address of the first row.
    /// </exception>
    /// TODO: maybe we should split this to FindRowLowerBound and FindRowUpperBound
    public int FindRowIndex(int offset)
    {
#if false
        if (rowAddresses.Length == 0 ||
            address < rowAddresses[0])
            throw new ArgumentOutOfRangeException("address");
        if (address > image.EndAddress)
            throw new ArgumentOutOfRangeException("address");
        if (address == image.EndAddress)
            return rowAddresses.Length;
#endif

        int k = Array.BinarySearch(rowAddresses, offset);
        if (k >= 0) // found; find first one
        {
            while (k > 0 && rowAddresses[k - 1] == offset)
                k--;
            return k;
        }
        else // not found, but would be inserted at ~k
        {
            k = ~k;
            return k - 1;
        }
    }

    public List<ProcedureItem> ProcedureItems
    {
        get { return procItems; }
    }

    //public List<SegmentItem> SegmentItems
    //{
    //    get { return segmentItems; }
    //}
}

/// <summary>
/// Represents a row in ASM listing.
/// </summary>
public abstract class ListingRow
{
    readonly Assembly assembly;
    readonly Address location;
    
    protected ListingRow(Assembly assembly, Address location)
    {
        this.assembly = assembly;
        this.location = location;
    }

    public Assembly Assembly => assembly;

    /// <summary>
    /// Gets the address of the listing row.
    /// </summary>
    public Address Location => location;

    public string LocationString => assembly.GetImage().FormatAddress(location);

    public virtual Color ForeColor => Colors.Black;

    /// <summary>
    /// Gets the opcode bytes of this listing row. Must not be null.
    /// </summary>
    public abstract byte[] Opcode { get; }

    public string OpcodeText
    {
        get
        {
            if (Opcode == null)
                return null;
            else if (Opcode.Length <= 6)
                return FormatBinary(Opcode, 0, Opcode.Length);
            else
                return FormatBinary(Opcode, 0, 6) + "...";
        }
    }

    /// <summary>
    /// Gets the label to display for this row, or null if there is no
    /// label to display.
    /// </summary>
    public virtual string Label
    {
        get
        {
            // Check whether we have a procedure starting at this address.
            var proc = assembly.GetImage().Procedures.Find(this.Location);
            if (proc != null)
                return proc.Name;
            else
                return null;
        }
    }

    /// <summary>
    /// Gets the main text to display for this listing row.
    /// </summary>
    public abstract string Text { get; }

    public virtual string RichText
    {
        get { return Text; }
    }

    public static string FormatBinary(byte[] data, int startIndex, int count)
    {
        var builder = new StringBuilder();
        for (int i = 0; i < count; i++)
        {
            if (i > 0)
                builder.Append(' ');
            builder.AppendFormat("{0:x2}", data[startIndex + i]);
        }
        return builder.ToString();
    }
}

#if false
class ListingRowLocationComparer : IComparer<ListingRow>
{
    public int Compare(ListingRow x, ListingRow y)
    {
        return x.Location.EffectiveAddress.CompareTo(y.Location.EffectiveAddress);
    }
}
#endif

/// <summary>
/// Represents a continuous range of unanalyzed bytes.
/// </summary>
class BlankListingRow : ListingRow
{
    private byte[] data;

    public BlankListingRow(Assembly assembly, Address location, byte[] data)
        : base(assembly, location)
    {
        this.data = data;
    }

    public override byte[] Opcode
    {
        get { return data; }
    }

    public override string Text => string.Format($"{{0}} unanalyzed bytes.", data.Length);

#if false
    public override ListViewItem CreateViewItem()
    {
        ListViewItem item = base.CreateViewItem();
        item.BackColor = Color.LightGray;
        return item;
    }
#endif
}

class CodeListingRow(Assembly assembly, Address location, Instruction instruction, byte[] code) : ListingRow(assembly, location)
{
    private Instruction instruction = instruction;
    private byte[] code = code;
    private string strInstruction =
            new SymbolicInstructionFormatter().FormatInstruction(instruction);

    public Instruction Instruction => this.instruction;

    public override byte[] Opcode => code;

    public override string Text
        //get { return instruction.ToString(); }
        => strInstruction;

    public override string RichText
    {
        get
        {
#if false
            if (instruction.Operands.Length == 1 &&
                instruction.Operands[0] is RelativeOperand)
            {
                StringBuilder sb = new StringBuilder();
                // TODO: add prefix
                sb.Append(instruction.Operation.ToString());

                sb.AppendFormat(" <a href=\"ddd://document1/#{0}\">{1}</a>",
                    "somewhere",
                    instruction.Operands[0]);
                return sb.ToString();
            }
            else
#endif
            {
                return Text;
            }
        }
    }
}

class DataListingRow : ListingRow
{
    private byte[] data;

    public DataListingRow(Assembly assembly, Address location, byte[] data)
        : base(assembly, location)
    {
        this.data = data;
    }

    public override byte[] Opcode => data;

    public override string Text => data.Length switch
    {
        1 => string.Format("db {0:x2}", data[0]),
        2 => string.Format("dw {0:x4}", BitConverter.ToUInt16(data, 0)),
        4 => string.Format("dd {0:x8}", BitConverter.ToUInt32(data, 0)),
        _ => "** data **",
    };
}

class ErrorListingRow : ListingRow
{
    private Error error;

    public ErrorListingRow(Assembly assembly, Error error)
        : base(assembly, error.Location)
    {
        this.error = error;
    }

    public override Color ForeColor => Colors.Red;

    public override byte[] Opcode => [];

    public override string Text => error.Message;

#if false
    public override ListViewItem CreateViewItem()
    {
        ListViewItem item = base.CreateViewItem();
        item.ForeColor = Color.Red;
        return item;
    }
#endif
}

class LabelListingRow : ListingRow
{
    private BasicBlock block;

    public LabelListingRow(Assembly assembly, BasicBlock block)
        : base(assembly, Address.Invalid)
    {
        this.block = block;
    }

    public override byte[] Opcode => null;

    public override string Text => $"loc_{block.Location.Offset}";

#if false
    public override ListViewItem CreateViewItem()
    {
        ListViewItem item = base.CreateViewItem();
        //item.UseItemStyleForSubItems = true;
        //item.SubItems[2].ForeColor = Color.Blue;
        item.ForeColor = Color.Blue;
        return item;
    }
#endif
}

public class ProcedureItem(Procedure procedure)
{
    public Procedure Procedure { get; private set; } = procedure;

    /// <summary>
    /// Gets or sets the index of the first row to display for this
    /// procedure. Note that this row may not belong to this procedure.
    /// </summary>
    //public int FirstRowIndex { get; set; }

    /// <summary>
    /// Gets or sets the index of the last row to display for this
    /// procedure. Note that this row may not belong to this procedure.
    /// </summary>
    //public int LastRowIndex { get; set; }

    public override string ToString() => Procedure.EntryPoint.ToString();
}

#if false
public class SegmentItem
{
    public SegmentItem(Segment segment)
    {
        this.SegmentStart = segment.OffsetBounds.ToFarPointer(segment.SegmentAddress);
    }

    public Address SegmentStart { get; private set; }

    public UInt16 SegmentAddress
    {
        get { return (UInt16)SegmentStart.Segment; }
    }

    public override string ToString()
    {
        return SegmentStart.ToString();
    }
}
#endif

public enum ListingScope : int
{
    /// <summary>
    /// Displays only the current procedure. If this procedure crosses
    /// multiple segments or is not contiguous, display a label to
    /// indicate the discontinuities.
    /// </summary>
    Procedure,

    /// <summary>
    /// Displays only the current segment. If a procedure on this segment
    /// jumps to another segment, that part is not displayed.
    /// </summary>
    Segment,

    /// <summary>
    /// Displays all segments in the current module, in the order of their
    /// segment ID.
    /// </summary>
    Module,
}
