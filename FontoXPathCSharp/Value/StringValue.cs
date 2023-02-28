using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class StringValue : AtomicValue
{
    public readonly string Value;

    public StringValue(string value) : base(ValueType.XsString)
    {
        Value = value;
    }

    public static StringValue CreateStringValue(object? value)
    {
        var stringValue = value switch
        {
            null => throw new Exception("Tried to initialize a xs:string with null."),
            string s => s,
            AtomicValue a => a.GetValue().ToString() ?? "",
            _ => value.ToString() ?? ""
        };

        return new StringValue(stringValue);
    }

    public override object GetValue()
    {
        return Value;
    }
}