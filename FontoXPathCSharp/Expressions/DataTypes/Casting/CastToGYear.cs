using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.DataTypes.Casting;

public static class CastToGYear
{
    private static AtomicValue CreateGYearValue(DateTimeValue value)
    {
        return AtomicValue.Create(value, ValueType.XsGYear);
    }

    public static CastingFunction ToGYear(ValueType type)
    {
        if (type.IsSubtypeOf(ValueType.XsDateTime))
            return value =>
                new SuccessResult<AtomicValue>(
                    CreateGYearValue(((DateTimeValue)value).ConvertToType(ValueType.XsGYear)));

        if (type.IsSubtypeOfAny(ValueType.XsUntypedAtomic, ValueType.XsString))
            return value =>
                new SuccessResult<AtomicValue>(
                    CreateGYearValue(DateTimeValue.FromString(Convert.ToString(value.GetValue())!)));

        return _ => new ErrorResult<AtomicValue>(
            "Casting not supported from given type to xs:gYear or any of its derived types.",
            "XPTY0004"
        );
    }
}