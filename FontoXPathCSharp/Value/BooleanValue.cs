using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class BooleanValue : AbstractValue
{
    private readonly bool _value;

    public BooleanValue(bool value) : base(ValueType.XsBoolean)
    {
        _value = value;
    }

    public override string ToString()
    {
        return "<Value>[type: " + Type + ", value: " + _value + "]";
    }
}