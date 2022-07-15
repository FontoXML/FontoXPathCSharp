using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class DoubleValue : AtomicValue
{
    public readonly decimal Value;

    public DoubleValue(decimal value) : base(ValueType.XsDouble)
    {
        Value = value;
    }

    public override string ToString()
    {
        return "<Value>[type: " + Type + ", value: " + Value + "]";
    }

    public override object GetValue()
    {
        return Value;
    }
}