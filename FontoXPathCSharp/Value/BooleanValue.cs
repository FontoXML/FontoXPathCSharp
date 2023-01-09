using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class BooleanValue : AtomicValue
{
    public readonly bool Value;

    public BooleanValue(bool value) : base(ValueType.XsBoolean)
    {
        Value = value;
    }

    public static BooleanValue CreateBooleanValue(object? value)
    {
        var booleanValue = value is string str 
            ? CreateFromString(str) 
            : CreateFromValue(value);

        return new BooleanValue(booleanValue);
    }

    private static bool CreateFromString(string str)
    {
        try
        {
            return bool.Parse(str);
        }
        catch (FormatException formatEx)
        {
            throw new XPathException("FORG0001", formatEx.Message);
        }
        catch (OverflowException overflowEx)
        {
            throw new XPathException("FOCA0001", overflowEx.Message);
        }
    }

    private static bool CreateFromValue(object? value)
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