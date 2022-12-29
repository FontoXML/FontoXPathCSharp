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
            ? CreateFromString(str) 
            : CreateFromValue(value);
        
        return new FloatValue(floatValue);
    }

    private static float CreateFromString(string str)
    {
        return NumericCast(str, v =>
        {
            return str switch
            {
                "INF" => float.PositiveInfinity,
                "-INF" => float.NegativeInfinity,
                _ => float.Parse(str)
            };
        });
    }

    private static float CreateFromValue(object? val)
    {
        return NumericCast(val, Convert.ToSingle);
    }
}