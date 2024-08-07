﻿using System;
using System.ComponentModel;

namespace Disassembler;

[TypeConverter(typeof(ExpandableObjectConverter))]
public class SegmentGroup : IAddressReferent
{
    /// <summary>
    /// Gets the name of the group. Groups from different object modules
    /// are combined if their names are identical.
    /// </summary>
    [Browsable(true)]
    public string Name { get; internal set; }

    /// <summary>
    /// Gets the logical segments contained in this group.
    /// </summary>
    [Browsable(true)]
    public LogicalSegment[] Segments { get; internal set; }

    public override string ToString() => Name;

    string IAddressReferent.Label => Name;

    public Address Resolve() => throw new NotSupportedException();
}
