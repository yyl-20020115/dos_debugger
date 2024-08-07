﻿using System;
using System.Collections.Generic;

namespace Disassembler;

/// <summary>
/// Represents a cross-reference between code and code or code and data.
/// A xref between code and code is analog to an edge in a Control Flow 
/// Graph.
/// </summary>
public class XRef : IGraphEdge<Address>
{
    /// <summary>
    /// Gets the target address being referenced. This may be set to
    /// <code>ResolvedAddress.Invalid</code> if the target address cannot
    /// be determined, such as in a dynamic jump or call.
    /// </summary>
    public Address Target { get; private set; }

    /// <summary>
    /// Gets the source address that refers to target. This may be set to
    /// <code>ResolvedAddress.Invalid</code> if the source address cannot
    /// be determined, such as in the entry routine of a program.
    /// </summary>
    public Address Source { get; private set; }

    /// <summary>
    /// Gets the type of this cross-reference.
    /// </summary>
    public XRefType Type { get; private set; }

    /// <summary>
    /// Gets the address of the associated data item. This is relevant
    /// if Type is NearIndexedJump or FarIndexedJump, where DataLocation
    /// contains the address of the jump table entry.
    /// </summary>
    public Address DataLocation { get; private set; }

    //public XRefContext Context { get; set; }

#if false
    /// <summary>
    /// Returns true if this xref is dynamic, i.e. its Target address
    /// contains <code>LogicalAddress.Invalid</code>.
    /// </summary>
    public bool IsDynamic
    {
        get { return Target == Address.Invalid; }
    }
#endif

    public XRef()
    {
        this.Source = Address.Invalid;
        this.Target = Address.Invalid;
        this.DataLocation = Address.Invalid;
    }

    public XRef(XRefType type, Address source, Address target, Address dataLocation)
    {
        this.Source = source;
        this.Target = target;
        this.Type = type;
        this.DataLocation = dataLocation;
    }

    public XRef(XRefType type, Address source, Address target)
        : this(type, source, target, Address.Invalid)
    {
    }

    public override string ToString() => $"{Source} -> {Target} ({Type})";

    /// <summary>
    /// Compares two XRef objects by source, target, and data location,
    /// in descending priority.
    /// </summary>
    public static int CompareByLocation(XRef x, XRef y)
    {
        int cmp = x.Source.CompareTo(y.Source);
        if (cmp == 0)
            cmp = x.Target.CompareTo(y.Target);
        if (cmp == 0)
            cmp = x.DataLocation.CompareTo(y.DataLocation);
        return cmp;
    }

    /// <summary>
    /// Compares two XRef objects by priority (precedence). An XRef object
    /// with a smaller numeric Type value has higher precedence, and 
    /// compare smaller (as in a min-priority queue).
    /// </summary>
    public static int CompareByPriority(XRef x, XRef y)
    {
        return (int)x.Type - (int)y.Type;
    }
}

/// <summary>
/// Defines types of cross-references. The numeric values of the enum
/// members are in decreasing order of their priority in analysis.
/// </summary>
public enum XRefType : int
{
    /// <summary>
    /// Indicates that the XRef object is invalid.
    /// </summary>
    None = 0,

    /// <summary>
    /// User specified entry point (such as program start or symbol).
    /// </summary>
    UserSpecified,

    /// <summary>
    /// A JMPN instruction refers to this location.
    /// </summary>
    NearJump,

    /// <summary>
    /// A JMPF instruction refers to this location.
    /// </summary>
    FarJump,

    /// <summary>
    /// A CALLN instruction refers to this location.
    /// </summary>
    NearCall,

    /// <summary>
    /// A CALLF instruction refers to this location.
    /// </summary>
    FarCall,

    /// <summary>
    /// A Jcc instruction refers to this location. In the x86 instruction
    /// set, a conditional jump is always near and always relative (i.e.
    /// its target address can always be determined).
    /// </summary>
    ConditionalJump,

    /// <summary>
    /// Indicates a branch-not-taken or a return-from-call condition.
    /// </summary>
    FallThrough,

    /// <summary>
    /// A JUMP instruction where the jump target address is given by
    /// a memory location (such as jump table). ??????
    /// </summary>
    //IndirectJump,

#if false
XREF_RETURN_FROM_CALL      = 5,    
XREF_RETURN_FROM_INTERRUPT = 6,    
#endif

    /// <summary>
    /// A JMPN instruction refers to this location indirectly through
    /// a word-sized jump table entry. The address of the jump table
    /// entry is stored in the DataLocation field of the XRef object.
    /// </summary>
    NearIndexedJump,

    /// <summary>
    /// A JMPF instruction refers to this location indirectly through
    /// a dword-sized jump table entry. The address of the jump table
    /// entry is stored in the DataLocation field of the XRef object.
    /// </summary>
    /* FarIndexedJump, */

    /// <summary>
    /// An INT xx or INTO instruction. The Source is the location of the
    /// instruction, and Target.Offset is the interrupt number.
    /// </summary>
    Interrupt,

    /// <summary>
    /// An IRET instruction.
    /// </summary>
    InterruptReturn,

    /// <summary>
    /// A RET instruction.
    /// </summary>
    NearReturn,

    /// <summary>
    /// A RETF instruction.
    /// </summary>
    FarReturn,

    /// <summary>
    /// A HLT instruction.
    /// </summary>
    Halt,
}

/// <summary>
/// Provides additional information related to a cross reference.
/// </summary>
//public class XRefContext
//{
//    public LogicalAddress LogicalTarget;
//}

/// <summary>
/// Maintains a collection of cross references and provides methods to
/// find xrefs by source or target efficiently.
/// </summary>
// TODO: maybe we should make an interface IGraph<TNode,TEdge>
// and make XRefCollection implement that interface. That's better
// because we are much more than a collection!
public class XRefCollection : ICollection<XRef>
{
    readonly Graph<Address, XRef> graph = new();

    /// <summary>
    /// Creates a cross reference collection.
    /// </summary>
    public XRefCollection()
    {
    }

    /// <summary>
    /// Clears all the cross references stored in this collection.
    /// </summary>
    public void Clear() => graph.Clear();

    /// <summary>
    /// Gets the number of cross references in this collection.
    /// </summary>
    public int Count => graph.Edges.Count;

    public void Add(XRef xref)
    {
        if (xref == null)
        {
            throw new ArgumentNullException(nameof(xref));
        }
        if (xref.Source == Address.Invalid &&
            xref.Target == Address.Invalid)
        {
            throw new ArgumentException("Source and Target cannot be both Invalid.");
        }

        graph.AddEdge(xref);

        // Raise the XRefAdded event.
        //if (XRefAdded != null)
        //{
        //    LogicalXRefAddedEventArgs e = new LogicalXRefAddedEventArgs(xref);
        //    XRefAdded(this, e);
        //}
    }

    /// <summary>
    /// Gets a list of xrefs with dynamic target (i.e. target == Invalid).
    /// </summary>
    /// <returns></returns>
    public IEnumerable<XRef> GetDynamicReferences() => graph.GetIncomingEdges(Address.Invalid);

    /// <summary>
    /// Gets all cross references that points to 'target', in the order
    /// that they were added.
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public IEnumerable<XRef> GetReferencesTo(Address target) => graph.GetIncomingEdges(target);

    /// <summary>
    /// Gets all cross references that points from 'source'.
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public IEnumerable<XRef> GetReferencesFrom(Address source) => graph.GetOutgoingEdges(source);

    //public event EventHandler<LogicalXRefAddedEventArgs> XRefAdded;

    #region ICollection Interface Implementation

    public bool Contains(XRef item) => throw new NotSupportedException();

    public void CopyTo(XRef[] array, int arrayIndex) => graph.Edges.CopyTo(array, arrayIndex);

    public bool IsReadOnly => false;

    public bool Remove(XRef item) => throw new NotSupportedException();

    public IEnumerator<XRef> GetEnumerator() => graph.Edges.GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
}

#if false
public class LogicalXRefAddedEventArgs : EventArgs
{
    public XRef XRef { get; private set; }

    public LogicalXRefAddedEventArgs(XRef xref)
    {
        this.XRef = xref;
    }
}
#endif
