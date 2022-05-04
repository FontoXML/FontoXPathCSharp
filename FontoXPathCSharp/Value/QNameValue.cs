using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class QNameValue : AbstractValue
{
    private readonly QName _value;
    
    public QNameValue(QName value) : base(ValueType.XsQName)
    {
        _value = value;
    }
    
    public override string ToString()
    {
        return "<Value>[type: " + Type + ", value: " + _value + "]";
    }
}