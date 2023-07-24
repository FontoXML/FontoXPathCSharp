using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class StringValue : AtomicValue
{
    public readonly string Value;

    private StringValue(string value, ValueType type) : base(type)
    {
        Value = value;
    }

    public static StringValue CreateStringValue(object? value, ValueType type)
    {
        if (!type.IsSubtypeOfAny(ValueType.XsString, ValueType.XsAnyUri)) 
            throw new Exception("Can only create a StringValue for xs:string or xs:anyURI");
        
        var stringValue = value switch
        {
            null => throw new Exception($"Tried to initialize a StringValue (for {type.Name()}) with null."),
            string s => s,
            AtomicValue a => a.GetValue().ToString() ?? "",
            _ => value.ToString() ?? ""
        };

        return new StringValue(stringValue, type);
    }

    public override object GetValue()
    {
        return Value;
    }
}