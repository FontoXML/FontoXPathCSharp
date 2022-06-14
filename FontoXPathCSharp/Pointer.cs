using FontoXPathCSharp.Types.Node;
using FontoXPathCSharp.Value;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp;

public abstract class Pointer<TNode> : AbstractValue
{
    protected Pointer(TNode node) : base(ValueType.Node)
    {
        Node = node;
    }

    public TNode Node { get; set; }
}

public class NodePointer : Pointer<Node>
{
    public NodePointer(Node node) : base(node)
    {
    }
}