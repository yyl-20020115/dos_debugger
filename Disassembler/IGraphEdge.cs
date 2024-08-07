namespace Disassembler;

public interface IGraphEdge<TNode>
{
    TNode Source { get; }
    TNode Target { get; }
}