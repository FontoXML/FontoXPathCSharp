using System.Globalization;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class IntegerValue : NumericValue<long>
{
    public IntegerValue(long value, ValueType type) : base(value, type)
    {
        if (!type.IsSubtypeOf(ValueType.XsInteger))
            throw new Exception("Cannot assign an integer value from a type that does not inherit xs:integer");
    }

    public static IntegerValue CreateIntegerValue(object? value, ValueType type)
    {
        var integerValue = value is string str
            ? HandleStringParse(str)
            : ConvertToInt(value);

        return new IntegerValue(integerValue, type);
    }

    private static long HandleStringParse(string str)
    {
        try
        {
            var style = NumberStyles.Integer | NumberStyles.AllowDecimalPoint;
            return long.Parse(str, style);
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

    private static long ConvertToInt(object? value)
    {
        return value != null
            ? Convert.ToInt64(value)
            : throw new XPathException("FORG0001", "Tried to initialize an IntValue with null.");
    }
}