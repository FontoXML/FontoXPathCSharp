using System.Xml;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class NodeValue : AbstractValue
{
    protected NodeValue(XmlNode value, ValueType valueType) : base(valueType)
    {
        Value = value;
    }
    public NodeValue(XmlNode value) : this(value, ValueType.Node) {
    }

    public XmlNode Value { get; }

    public override string ToString()
    {
        return $"<Value>[type: {Type}, value: {Value}]";
    }
}