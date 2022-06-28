using System.Xml;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class AttributeValue : NodeValue
{
    public AttributeValue(XmlNode value) : base(value, ValueType.Attribute)
    {
    }
}