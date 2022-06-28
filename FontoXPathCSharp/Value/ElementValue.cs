using System.Xml;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class ElementValue : NodeValue
{
    public ElementValue(XmlNode value) : base(value, ValueType.Element)
    {
    }
}