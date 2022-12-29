using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public abstract class NumericValue<T> : AtomicValue where T : notnull
{
    public readonly T Value;

    protected NumericValue(T value, ValueType type) : base(type)
    {
        if (!type.IsSubtypeOf(ValueType.XsNumeric))
            throw new Exception("Cannot create a NumericValue with a type that does not inherit xs:numeric");
        Value = value;
    }

    public override object GetValue()
    {
        return Value;
    }
}