using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class DoubleValue : NumericValue<double>
{
    public DoubleValue(double value) : base(value, ValueType.XsDouble)
    {
    }

    public static DoubleValue CreateDoubleValue(object? value)
    {
        var doubleValue = value is string str
            ? HandleStringParse(str)
            : ConvertToFloat(value);

        return new DoubleValue(doubleValue);
    }

    private static double ConvertToFloat(object? value)
    {
        return value != null
            ? Convert.ToDouble(value)
            : throw new XPathException("FORG0001","Tried to initialize a DoubleValue with null.");
    }
    
    private static double HandleStringParse(string str)
    {
        try
        {
            return str switch
            {
                "INF" => double.PositiveInfinity,
                "-INF" => double.NegativeInfinity,
                _ => double.Parse(str)
            };
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
}