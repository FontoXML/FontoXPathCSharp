using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class StringValue : AtomicValue
{
    public readonly string Value;

    public StringValue(string value) : base(ValueType.XsString)
    {
        Value = value;
    }

    public StringValue(object? value) : base(ValueType.XsString)
    {
        Value = (value is string s
            ? s
            : value != null
                ? value.ToString()
                : throw new Exception("Tried to initialize an StringValue with null.")) ?? string.Empty;
    }

    public override object GetValue()
    {
        return Value;
    }
}