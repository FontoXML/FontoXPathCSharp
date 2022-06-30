using System.Xml;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class PointerValue : AbstractValue
{
    public PointerValue(NodePointer pointer, XmlNode domFacade) : base(GetNodeSubtype(pointer, domFacade))
    {
        Value = pointer;
    }

    public NodePointer Value { get; }

    private static ValueType GetNodeSubtype(NodePointer pointer, XmlNode domFacade)
    {
        return domFacade.NodeType switch
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
}