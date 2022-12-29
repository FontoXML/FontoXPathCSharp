using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class IntegerValue : NumericValue
{
    public readonly long Value;

    public IntegerValue(long value, ValueType type) : base(type)
    {
        if (!type.IsSubtypeOf(ValueType.XsInteger))
            throw new XPathException("", "Cannot assign an integer value from a type that does not inherit xs:integer");
        Value = value;
    }

    public static IntegerValue CreateIntegerValue(object? value, ValueType type)
    {
        var integerValue = value is string s
            ? long.TryParse(s, out var val) ? val : throw new Exception($"Can't parse {s} into an integer.")
            : ConvertToInt(value);

        return new IntegerValue(integerValue, type);
    }

    private static long ConvertToInt(object? value)
    {
        return value != null
            ? Convert.ToInt64(value)
            : throw new Exception("Tried to initialize an IntValue with null.");
    }

    public override object GetValue()
    {
        return Value;
    }
}