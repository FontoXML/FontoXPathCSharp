using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class DecimalValue : NumericValue<decimal>
{
    public DecimalValue(decimal value) : base(value, ValueType.XsDecimal)
    {
    }

    public static DecimalValue CreateDecimalValue(object? value)
    {
        var decimalValue = value is string s
            ? HandleStringParse(s)
            : ConvertToDecimal(value);

        return new DecimalValue(decimalValue);
    }

    private static decimal HandleStringParse(string str)
    {
        try
        {
            return decimal.Parse(str);
        }
        catch (FormatException formatEx)
        {
            throw new XPathException("FORG0001", formatEx.Message);
        }
        catch (OverflowException overflowEx)
        {
            throw new XPathException("FOCA0001", overflowEx.Message);
        }
    }

    private static decimal ConvertToDecimal(object? value)
    {
        return value != null
            ? Convert.ToDecimal(value)
            : throw new XPathException("FORG0001", "Tried to initialize a DecimalValue with null.");
    }
}