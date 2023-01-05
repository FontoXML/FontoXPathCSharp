using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.DataTypes.Casting;

public static class CastToDateTime
{
    private static AtomicValue CreateDateTimeValue(DateTimeValue value)
    {
        return AtomicValue.Create(value, ValueType.XsDateTime);
    }

    public static CastingFunction ToDateTime(ValueType type)
    {
        if (type.IsSubtypeOf(ValueType.XsDateTime))
            return value =>
                new SuccessResult<AtomicValue>(
                    CreateDateTimeValue((value.GetAs<DateTimeValue>()).ConvertToType(ValueType.XsDateTime)));

        if (type.IsSubtypeOfAny(ValueType.XsUntypedAtomic, ValueType.XsString))
            return value =>
                new SuccessResult<AtomicValue>(
                    CreateDateTimeValue(DateTimeValue.FromString(value.GetValue().ToString()!,
                        ValueType.XsDateTime)));

        return _ => new ErrorResult<AtomicValue>(
            "Casting not supported from given type to xs:dateTime or any of its derived types.",
            "XPTY0004"
        );
    }
}