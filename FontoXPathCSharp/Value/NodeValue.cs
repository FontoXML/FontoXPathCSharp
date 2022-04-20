using System.Xml;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class NodeValue : AbstractValue
{
    private readonly XmlNode _value;

    public NodeValue(XmlNode value) : base(ValueType.Node)
    {
        _value = value;
    }

    public XmlNode Value()
    {
        return _value;
    }

    public override string ToString()
    {
        return "<Value>[type: " + Type + ", value: " + _value + "]";
    }
}