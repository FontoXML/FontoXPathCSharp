using FontoXPathCSharp.Expressions;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class FloatValue : AtomicValue
{
    public readonly decimal Value;

    public FloatValue(decimal value) : base(ValueType.XsFloat)
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