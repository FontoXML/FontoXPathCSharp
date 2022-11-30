using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.DataTypes.Casting;

public static class CastToGMonthDay
{
    private static AtomicValue CreateGMonthDayValue(DateTimeValue value)
    {
        return AtomicValue.Create(value, ValueType.XsGMonthDay);
    }

    public static CastingFunction ToGMonthDay(ValueType type)
    {
        if (type.IsSubtypeOf(ValueType.XsDateTime))
            return value =>
                new SuccessResult<AtomicValue>(
                    CreateGMonthDayValue(((DateTimeValue)value).ConvertToType(ValueType.XsGMonthDay)));

        if (type.IsSubtypeOfAny(ValueType.XsUntypedAtomic, ValueType.XsString))
            return value =>
                new SuccessResult<AtomicValue>(
                    CreateGMonthDayValue(DateTimeValue.FromString(Convert.ToString(value.GetValue())!,
                        ValueType.XsGMonthDay)));

        return _ => new ErrorResult<AtomicValue>(
            "Casting not supported from given type to xs:gMonthDay or any of its derived types.",
            "XPTY0004"
        );
    }
}