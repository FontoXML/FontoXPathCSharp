using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class BooleanValue : AtomicValue
{
    public readonly bool Value;

    public BooleanValue(bool value) : base(ValueType.XsBoolean)
    {
        Value = value;
    }

    public BooleanValue(object? value) : base(ValueType.XsBoolean)
    {
        Value = value is string s
            ? bool.TryParse(s, out var val) ? val : throw new Exception($"Can't parse {s} into an bool.")
            : ConvertToBool(value);
    }

    private bool ConvertToBool(object? value)
    {
        return value != null
            ? Convert.ToBoolean(value)
            : throw new Exception("Tried to initialize an BoolValue with null.");
    }


    public override object GetValue()
    {
        return Value;
    }
}