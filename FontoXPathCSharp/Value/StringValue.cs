using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class StringValue : AbstractValue
{
    public readonly string Value;

    public StringValue(string value) : base(ValueType.XsString)
    {
        Value = value;
    }

    public override string ToString()
    {
        return "<Value>[type: " + Type + ", value: " + Value + "]";
    }
}