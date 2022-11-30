using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.DataTypes.Casting;

public static class CastToTime
{
    private static AtomicValue CreateTimeValue(DateTimeValue value)
    {
        return AtomicValue.Create(value, ValueType.XsTime);
    }

    public static CastingFunction ToTime(ValueType type)
    {
        if (type.IsSubtypeOf(ValueType.XsTime))
            return value =>
                new SuccessResult<AtomicValue>(
                    CreateTimeValue(((DateTimeValue)value).ConvertToType(ValueType.XsTime)));

        if (type.IsSubtypeOfAny(ValueType.XsUntypedAtomic, ValueType.XsString))
            return value =>
                new SuccessResult<AtomicValue>(
                    CreateTimeValue(DateTimeValue.FromString(Convert.ToString(value.GetValue())!, ValueType.XsTime)));

        return _ => new ErrorResult<AtomicValue>(
            "Casting not supported from given type to xs:time or any of its derived types.",
            "XPTY0004"
        );
    }
}