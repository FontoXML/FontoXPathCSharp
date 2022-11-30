using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.DataTypes.Casting;

public class CastToGDay
{
    private static AtomicValue CreateGDayValue(DateTimeValue value)
    {
        return AtomicValue.Create(value, ValueType.XsGDay);
    }

    public static CastingFunction ToGDay(ValueType type)
    {
        if (type.IsSubtypeOf(ValueType.XsDateTime))
            return value =>
                new SuccessResult<AtomicValue>(
                    CreateGDayValue(((DateTimeValue)value).ConvertToType(ValueType.XsGDay)));

        if (type.IsSubtypeOfAny(ValueType.XsUntypedAtomic, ValueType.XsString))
            return value =>
                new SuccessResult<AtomicValue>(
                    CreateGDayValue(DateTimeValue.FromString(Convert.ToString(value.GetValue())!, ValueType.XsGDay)));

        return _ => new ErrorResult<AtomicValue>(
            "Casting not supported from given type to xs:gDay or any of its derived types.",
            "XPTY0004"
        );
    }
}