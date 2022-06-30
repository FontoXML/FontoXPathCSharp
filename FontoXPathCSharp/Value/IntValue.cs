using FontoXPathCSharp.Expressions;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class IntValue : AtomicValue
{
    public readonly int Value;

    public IntValue(int value) : base(ValueType.XsInteger)
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