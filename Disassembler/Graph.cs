using System;
using System.Collections.Generic;

namespace Disassembler;

internal class Graph<T1, T2>
{
    public Graph()
    {
    }

    public List<XRef> Edges { get; internal set; } = [];

    public void AddEdge(XRef xref)
    {
        this.Edges.Add(xref);
    }

    public void Clear()
    {
        this.Edges.Clear();
    }

    public IEnumerable<XRef> GetIncomingEdges(Address invalid)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<XRef> GetOutgoingEdges(Address source)
    {
        throw new NotImplementedException();
    }
}