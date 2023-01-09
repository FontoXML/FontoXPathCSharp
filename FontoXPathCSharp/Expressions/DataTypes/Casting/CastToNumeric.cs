using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Value;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.DataTypes.Casting;

public class CastToNumeric
{
    private static readonly ValueType[] NumericTypes =
    {
        ValueType.XsDouble,
        ValueType.XsFloat,
        ValueType.XsDecimal,
        ValueType.XsInteger
    };

    public static CastingFunction ToNumeric(ValueType inputType,
        Func<ValueType, ValueType, CastingFunction> CastToPrimitiveType)
    {
        return value =>
        {
            foreach (var outputType in NumericTypes)
            {
                var result = CastToPrimitiveType(inputType, outputType)(value);
                if (result.Success) return result;
            }

            return new ErrorResult<AtomicValue>(
                $"Casting not supported from '{value}' given type to xs:numeric or any of its derived types.",
                "XPTY0004");
        };
    }
}