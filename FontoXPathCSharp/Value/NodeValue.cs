using System.Xml;

namespace FontoXPathCSharp.Value;

public class NodeValue : AbstractValue
{
    private readonly XmlNode Value;

    public NodeValue(XmlNode value) : base(ValueType.Node)
    {
        Value = value;
    }
    
    public override string ToString()
    {
        return "<Value>[type: " + Type + ", value: " + Value + "]";
    }
}