using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.DataTypes.Casting;

public class CastToYearMonthDuration
{
    private static AtomicValue CreateYearMonthDurationValue(DurationValue value)
    {
        return AtomicValue.Create(value, ValueType.XsYearMonthDuration);
    }

    public static CastingFunction ToYearMonthDuration(ValueType from)
    {
        if (from.IsSubtypeOf(ValueType.XsDuration) && !from.IsSubtypeOf(ValueType.XsDayTimeDuration))
            return value => new SuccessResult<AtomicValue>(CreateYearMonthDurationValue(value.GetAs<DurationValue>()));

        if (from.IsSubtypeOf(ValueType.XsDayTimeDuration))
            return _ =>
                new SuccessResult<AtomicValue>(
                    CreateYearMonthDurationValue(DurationValue.YearMonthDurationFromString("P0M")!));

        if (from.IsSubtypeOfAny(ValueType.XsUntypedAtomic, ValueType.XsString))
            return value =>
            {
                var parsedDuration = DurationValue.YearMonthDurationFromString(value.GetValue().ToString());
                return parsedDuration != null
                    ? new SuccessResult<AtomicValue>(CreateYearMonthDurationValue(parsedDuration))
                    : new ErrorResult<AtomicValue>(
                        $"Cannot cast {value} to xs:yearMonthDuration",
                        "FORG0001"
                    );
            };

        return _ => new ErrorResult<AtomicValue>(
            "Casting not supported from given type to xs:yearMonthDuration or any of its derived types.",
            "XPTY0004"
        );
    }
}