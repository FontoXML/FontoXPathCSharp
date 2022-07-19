using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class FloatValue : AtomicValue
{
    public readonly float Value;

    public FloatValue(float value) : base(ValueType.XsFloat)
    {
        Value = value;
    }

    public FloatValue(object? value) : base(ValueType.XsFloat)
    {
        Value = value is string s
            ? float.TryParse(s, out var val) ? val : StringEdgeCasesOrException(s) 
            : (float)(value ?? throw new Exception("Tried to initialize an FloatValue with null."));
    }

    private float StringEdgeCasesOrException(string s)
    {
        return s switch
        {
            "INF" => float.PositiveInfinity,
            "-INF" => float.NegativeInfinity,
            _ => throw new Exception($"Can't parse {s} into an float for a FloatValue.")
        };
    }

    public override object GetValue()
    {
        return Value;
    }
}