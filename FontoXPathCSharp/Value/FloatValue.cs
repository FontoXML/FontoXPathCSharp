using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class FloatValue : NumericValue<float>
{
    public FloatValue(float value) : base(value, ValueType.XsFloat)
    {
    }

    public static FloatValue CreateFloatValue(object? value)
    {
        var floatValue = value is string str
            ? HandleStringParse(str)
            : ConvertToFloat(value);

        return new FloatValue(floatValue);
    }

    private static float ConvertToFloat(object? value)
    {
        return value != null
            ? Convert.ToSingle(value)
            : throw new XPathException("FORG0001","Tried to initialize a FloatValue with null.");
    }

    private static float HandleStringParse(string str)
    {
        try
        {
            return str switch
            {
                "INF" => float.PositiveInfinity,
                "-INF" => float.NegativeInfinity,
                _ => float.Parse(str)
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