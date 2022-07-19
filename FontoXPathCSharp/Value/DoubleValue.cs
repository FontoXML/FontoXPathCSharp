using FontoXPathCSharp.Expressions;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class DoubleValue : AtomicValue
{
    public readonly double Value;

    public DoubleValue(double value) : base(ValueType.XsDouble)
    {
        Value = value;
    }
    
    public DoubleValue(object? value) : base(ValueType.XsDouble)
    {
        Value = value is string s
            ? double.TryParse(s, out var val) ? val : StringEdgeCasesOrException(s)
            : (double)(value ?? throw new Exception("Tried to initialize an DoubleValue with null."));
    }

    private double StringEdgeCasesOrException(string s)
    {
        return s switch
        {
            "INF" => double.PositiveInfinity,
            "-INF" => double.NegativeInfinity,
            _ => throw new Exception($"Can't parse {s} into an double for a DoubleValue.")
        };
    }

    public override object GetValue()
    {
        return Value;
    }
}