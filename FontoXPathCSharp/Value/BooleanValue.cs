using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class BooleanValue : AtomicValue
{
    public readonly bool Value;

    public BooleanValue(bool value) : base(ValueType.XsBoolean)
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