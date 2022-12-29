using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class DecimalValue : NumericValue<decimal>
{
    public DecimalValue(decimal value) : base(value, ValueType.XsDecimal)
    {
    }

    public static DecimalValue CreateDecimalValue(object? value)
    {
        var decimalValue = value is string str
            ? CreateFromString(str)
            : CreateFromValue(value);

        return new DecimalValue(decimalValue);
    }

    private static decimal CreateFromString(string str)
    {
        return NumericCast(str, decimal.Parse);
    }

    private static decimal CreateFromValue(object? val)
    {
        return NumericCast(val, Convert.ToDecimal);
    }
}