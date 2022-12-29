using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class FloatValue : NumericValue<float>
{
    public FloatValue(float value) : base(value, ValueType.XsFloat)
    {
    }

    public static FloatValue CreateFloatValue(object? value)
    {
        var floatValue = value is string s
            ? float.TryParse(s, out var val) ? val : StringEdgeCasesOrException(s)
            : ConvertToFloat(value);
        
        return new FloatValue(floatValue);
    }

    private static float ConvertToFloat(object? value)
    {
        return value != null
            ? Convert.ToSingle(value)
            : throw new Exception("Tried to initialize an FloatValue with null.");
    }


    private static float StringEdgeCasesOrException(string s)
    {
        return s switch
        {
            "INF" => float.PositiveInfinity,
            "-INF" => float.NegativeInfinity,
            _ => throw new Exception($"Can't parse {s} into an float for a FloatValue.")
        };
    }
}