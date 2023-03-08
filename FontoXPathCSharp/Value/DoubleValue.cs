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
            ? CreateFromString(str)
            : CreateFromValue(value);

        return new DoubleValue(doubleValue);
    }

    private static double CreateFromString(string str)
    {
        return NumericCast(str, _ =>
        {
            return str switch
            {
                "INF" => double.PositiveInfinity,
                "-INF" => double.NegativeInfinity,
                _ => double.Parse(str)
            };
        });
    }

    private static double CreateFromValue(object? val)
    {
        return NumericCast(val, Convert.ToDouble);
    }
}