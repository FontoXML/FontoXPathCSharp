using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class IntValue : AtomicValue
{
    public readonly long Value;

    public IntValue(long value) : base(ValueType.XsInt)
    {
        Value = value;
    }

    public IntValue(object? value) : base(ValueType.XsInt)
    {
        Value = value is string s
            ? long.TryParse(s, out var val) ? val : throw new Exception($"Can't parse {s} into an integer.")
            : ConvertToInt(value);
    }

    private long ConvertToInt(object? value)
    {
        return value != null
            ? Convert.ToInt64(value)
            : throw new Exception("Tried to initialize an IntValue with null.");
    }

    public override object GetValue()
    {
        return Value;
    }
}