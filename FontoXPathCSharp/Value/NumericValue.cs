using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public abstract class NumericValue<TS> : AtomicValue where TS : notnull
{
    public readonly TS Value;

    protected NumericValue(TS value, ValueType type) : base(type)
    {
        if (!type.IsSubtypeOf(ValueType.XsNumeric))
            throw new Exception("Cannot create a NumericValue with a type that does not inherit xs:numeric");
        Value = value;
    }

    // public override T GetAs<T>()
    // {
    //     if (Type.IsSubtypeOf(ValueType.XsInteger) && typeof(T) == DecimalValue) return (T)(object)new DecimalValue(Value);
    //     return base.GetAs<T>();
    // }

    public override object GetValue()
    {
        return Value;
    }
}