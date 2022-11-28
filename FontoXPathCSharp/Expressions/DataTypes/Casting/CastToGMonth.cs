using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.DataTypes.Casting;

public static class CastToGMonth
{
    private static AtomicValue CreateGMonthValue(DateTimeValue value)
    {
        return AtomicValue.Create(value, ValueType.XsGMonth);
    }

    public static CastingFunction ToGMonth(ValueType type)
    {
        if (type.IsSubtypeOf(ValueType.XsDateTime))
            return value =>
                new SuccessResult<AtomicValue>(
                    CreateGMonthValue(((DateTimeValue)value).ConvertToType(ValueType.XsGMonth)));

        if (type.IsSubtypeOfAny(ValueType.XsUntypedAtomic, ValueType.XsString))
            return value =>
                new SuccessResult<AtomicValue>(
                    CreateGMonthValue(DateTimeValue.FromString(Convert.ToString(value.GetValue())!)));

        return _ => new ErrorResult<AtomicValue>(
            "Casting not supported from given type to xs:gMonth or any of its derived types.",
            "XPTY0004"
        );
    }
}