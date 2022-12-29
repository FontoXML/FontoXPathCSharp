using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class DecimalValue : NumericValue
{
    public readonly decimal Value;

    public DecimalValue(decimal value) : base(ValueType.XsDecimal)
    {
        Value = value;
    }

    public static DecimalValue CreateDecimalValue(object? value)
    {
        var decimalValue = value is string s
            ? decimal.TryParse(s, out var val) ? val : throw new Exception($"Can't parse '{s}' into a DecimalValue.")
            : ConvertToDecimal(value);
        return new DecimalValue(decimalValue);
    }

    private static decimal ConvertToDecimal(object? value)
    {
        return value != null
            ? Convert.ToDecimal(value)
            : throw new Exception("Tried to initialize a DecimalValue with null.");
    }

    public override object GetValue()
    {
        return Value;
    }
}