using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.DataTypes.Casting;

public static class CastToDate
{
    private static AtomicValue CreateDateValue(DateTimeValue value)
    {
        return AtomicValue.Create(value, ValueType.XsDate);
    }

    public static CastingFunction ToDate(ValueType type)
    {
        if (type.IsSubtypeOf(ValueType.XsDateTime))
            return value =>
                new SuccessResult<AtomicValue>(CreateDateValue(((DateTimeValue)value).ConvertToType(ValueType.XsDate)));

        if (type.IsSubtypeOfAny(ValueType.XsUntypedAtomic, ValueType.XsString))
            return value =>
                new SuccessResult<AtomicValue>(
                    CreateDateValue(DateTimeValue.FromString(Convert.ToString(value.GetValue())!)));

        return _ => new ErrorResult<AtomicValue>(
            "Casting not supported from given type to xs:date or any of its derived types.",
            "XPTY0004"
        );
    }
}