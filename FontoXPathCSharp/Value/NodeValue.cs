using System.Xml;

namespace FontoXPathCSharp.Value;

public class NodeValue : AbstractValue
{
    public XmlNode Value;

    public NodeValue(XmlNode value) : base(ValueType.NODE)
    {
        Value = value;
    }
    
    public override string ToString()
    {
        return "<Value>[type: " + Type + ", value: " + Value + "]";
    }
}