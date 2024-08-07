using System;
using System.Collections.Generic;

namespace Disassembler;

internal class Graph<T1, T2>
{
    public Graph()
    {
    }

    public List<XRef> Edges { get; internal set; }

    internal void AddEdge(XRef xref)
    {
        throw new NotImplementedException();
    }

    internal void Clear()
    {
        throw new NotImplementedException();
    }

    internal IEnumerable<XRef> GetIncomingEdges(Address invalid)
    {
        throw new NotImplementedException();
    }

    internal IEnumerable<XRef> GetOutgoingEdges(Address source)
    {
        throw new NotImplementedException();
    }
}