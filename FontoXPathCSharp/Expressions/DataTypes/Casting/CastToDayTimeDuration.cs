using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.DataTypes.Casting;

public class CastToDayTimeDuration
{
    private static AtomicValue CreateDayTimeDurationValue(DurationValue value)
    {
        return AtomicValue.Create(value, ValueType.XsDayTimeDuration);
    }

    public static CastingFunction ToDayTimeDuration(ValueType from)
    {
        if (from.IsSubtypeOf(ValueType.XsDuration) && !from.IsSubtypeOf(ValueType.XsYearMonthDuration))
            return value => new SuccessResult<AtomicValue>(CreateDayTimeDurationValue(value.GetAs<DurationValue>()));

        if (from.IsSubtypeOf(ValueType.XsYearMonthDuration))
            return _ =>
                new SuccessResult<AtomicValue>(
                    CreateDayTimeDurationValue(DurationValue.DayTimeDurationFromString("PT0.0S")!));

        if (from.IsSubtypeOfAny(ValueType.XsUntypedAtomic, ValueType.XsString))
            return value =>
            {
                var parsedDuration = DurationValue.DayTimeDurationFromString(value.GetValue().ToString());
                return parsedDuration != null
                    ? new SuccessResult<AtomicValue>(CreateDayTimeDurationValue(parsedDuration))
                    : new ErrorResult<AtomicValue>(
                        $"Cannot cast {value} to xs:dayTimeDuration",
                        "FORG0001"
                    );
            };

        return _ => new ErrorResult<AtomicValue>(
            "Casting not supported from given type to xs:yearMonthDuration or any of its derived types.",
            "XPTY0004"
        );
    }
}