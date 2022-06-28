using FontoXPathCSharp.Types.Node;
using FontoXPathCSharp.Value;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp;

public class NodePointer : AbstractValue
{
    public NodePointer(Node node) : base(ValueType.Node)
    {
        Value = node;
    }

    public Node Value { get; }
}

public class AttributeNodePointer : AbstractValue
{
    public AttributeNodePointer(Attr node) : base(ValueType.Attribute)
    {
        Value = node;
    }
    
    public Attr Value { get; }
}

public class ElementNodePointer : AbstractValue
{
    public ElementNodePointer(Element node) : base(ValueType.Element)
    {
        Value = node;
    }
    
    public Element Value { get; }
}