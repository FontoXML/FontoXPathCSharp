using FontoXPathCSharp.Types.Node;
using FontoXPathCSharp.Value;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp;

public abstract class Pointer<TNode> : AbstractValue where TNode : Node
{
    protected Pointer(TNode node, ValueType type) : base(ValueType.Node)
    {
        Value = node;
    }

    public TNode Value { get; }
}

public class NodePointer : Pointer<Node>
{
    public NodePointer(Node node) : base(node, ValueType.Node)
    {
    }
}

public class AttributeNodePointer : Pointer<Attr>
{
    public AttributeNodePointer(Attr node) : base(node, ValueType.Attribute)
    {
    }
}