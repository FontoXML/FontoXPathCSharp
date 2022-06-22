using FontoXPathCSharp.Expressions;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class DoubleValue : AtomicValue
{
    public readonly double Value;

    public DoubleValue(double value) : base(ValueType.XsDouble)
    {
        Value = value;
    }

    public override string ToString()
    {
        return "<Value>[type: " + Type + ", value: " + Value + "]";
    }
}