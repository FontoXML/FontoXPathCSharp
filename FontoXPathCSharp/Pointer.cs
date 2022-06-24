using FontoXPathCSharp.Types.Node;
using FontoXPathCSharp.Value;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp;

public abstract class Pointer : AbstractValue
{
    protected Pointer(Node node, ValueType type) : base(type)
    {
        Value = node;
    }

    public Node Value { get; }
}

public class NodePointer : Pointer
{
    public NodePointer(Node node) : base(node, ValueType.Node)
    {
    }

    public Node Value { get; }
}

public class AttributeNodePointer : Pointer
{
    public AttributeNodePointer(Attr node) : base(node, ValueType.Attribute)
    {
    }
    
    public Attr Value { get; }
}

public class ElementNodePointer : Pointer
{
    public ElementNodePointer(Element node) : base(node, ValueType.Element)
    {
    }
    
    public Element Value { get; }
}