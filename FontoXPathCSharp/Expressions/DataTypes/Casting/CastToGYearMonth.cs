using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.DataTypes.Casting;

public static class CastToGYearMonth
{
    private static AtomicValue CreateGYearMonthValue(DateTimeValue value)
    {
        return AtomicValue.Create(value, ValueType.XsGYearMonth);
    }

    public static CastingFunction ToGYearMonth(ValueType type)
    {
        if (type.IsSubtypeOf(ValueType.XsDateTime))
            return value =>
                new SuccessResult<AtomicValue>(
                    CreateGYearMonthValue(((DateTimeValue)value).ConvertToType(ValueType.XsGYearMonth)));

        if (type.IsSubtypeOfAny(ValueType.XsUntypedAtomic, ValueType.XsString))
            return value =>
                new SuccessResult<AtomicValue>(
                    CreateGYearMonthValue(DateTimeValue.FromString(Convert.ToString(value.GetValue())!,
                        ValueType.XsGYearMonth)));

        return _ => new ErrorResult<AtomicValue>(
            "Casting not supported from given type to xs:gYearMonth or any of its derived types.",
            "XPTY0004"
        );
    }
}