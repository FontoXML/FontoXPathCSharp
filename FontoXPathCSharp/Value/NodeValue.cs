using System.Xml;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class NodeValue : AbstractValue
{
    public NodeValue(XmlNode value) : base(GetNodeType(value))
    {
        Value = value;
    }

    private static ValueType GetNodeType(XmlNode node)
    {
        return node.NodeType switch
        {
            XmlNodeType.Element => ValueType.Element,
            XmlNodeType.Attribute => ValueType.Attribute,
            XmlNodeType.Text => ValueType.Text,
            XmlNodeType.CDATA => ValueType.Text,
            XmlNodeType.ProcessingInstruction => ValueType.ProcessingInstruction,
            XmlNodeType.Comment => ValueType.Comment,
            XmlNodeType.Document => ValueType.DocumentNode,
            _ => ValueType.Node
        };
    }

    public XmlNode Value { get; }

    public override string ToString()
    {
        return $"<Value>[type: {Type}, value: {Value}]";
    }
}