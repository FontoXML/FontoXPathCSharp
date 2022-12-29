using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class DoubleValue : NumericValue<double>
{
    public DoubleValue(double value) : base(value, ValueType.XsDouble)
    {
    }

    public static DoubleValue CreateDoubleValue(object? value)
    {
        var doubleValue = value is string s
            ? double.TryParse(s, out var val) ? val : StringEdgeCasesOrException(s)
            : ConvertToFloat(value);
        
        return new DoubleValue(doubleValue);
    }

    private static double ConvertToFloat(object? value)
    {
        return value != null
            ? Convert.ToDouble(value)
            : throw new Exception("Tried to initialize an DoubleValue with null.");
    }

    private static double StringEdgeCasesOrException(string s)
    {
        return s switch
        {
            "INF" => double.PositiveInfinity,
            "-INF" => double.NegativeInfinity,
            _ => throw new Exception($"Can't parse {s} into an double for a DoubleValue.")
        };
    }
}