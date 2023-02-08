using System.Globalization;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class IntegerValue : NumericValue<long>
{
    public IntegerValue(long value, ValueType type) : base(value, type)
    {
        if (!type.IsSubtypeOf(ValueType.XsInteger))
            throw new Exception("Cannot assign an integer value from a type that does not inherit xs:integer");
    }

    public static IntegerValue CreateIntegerValue(object? value, ValueType type)
    {
        var integerValue = value is string str
            ? CreateFromString(str)
            : CreateFromValue(value);

        return new IntegerValue(integerValue, type);
    }

    private static long CreateFromString(string str)
    {
        return NumericCast(str, v => long.Parse(v, NumberStyles.Integer | NumberStyles.AllowDecimalPoint));
    }

    private static long CreateFromValue(object? val)
    {
        return NumericCast(val, Convert.ToInt64);
    }
}