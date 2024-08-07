using System.Collections.Generic;
using System.Linq;

namespace Disassembler;

public abstract class Graph<TNode, TEdge> where TEdge : IGraphEdge<TNode>
{
    public Graph() { }
    public List<TEdge> Edges { get; internal set; } = [];

    public void AddEdge(TEdge xref) => this.Edges.Add(xref);

    public void Clear() => this.Edges.Clear();

    public abstract IEnumerable<TEdge> GetIncomingEdges(TNode source);

    public abstract IEnumerable<TEdge> GetOutgoingEdges(TNode target);
}
public class AddressXRefGraph : Graph<Address, XRef>
{
    public override IEnumerable<XRef> GetIncomingEdges(Address source) => this.Edges.Where(e => e.Source == source);
    public override IEnumerable<XRef> GetOutgoingEdges(Address target) => this.Edges.Where(e => e.Target == target);
}