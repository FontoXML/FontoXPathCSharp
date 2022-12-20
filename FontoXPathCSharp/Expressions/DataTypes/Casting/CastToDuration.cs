using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.DataTypes.Casting;

public class CastToDuration
{
    private static AtomicValue CreateDurationValue(DurationValue value)
    {
        return AtomicValue.Create(value, ValueType.XsDuration);
    }

    public static CastingFunction ToDuration(ValueType from)
    {
        if (from.IsSubtypeOfAny(ValueType.XsYearMonthDuration, ValueType.XsDayTimeDuration))
            return value =>
                new SuccessResult<AtomicValue>(
                    CreateDurationValue(DurationValue.FromDuration(value.GetAs<DurationValue>())));

        if (from.IsSubtypeOf(ValueType.XsDuration))
            return value => new SuccessResult<AtomicValue>(CreateDurationValue(value.GetAs<DurationValue>()));

        if (from.IsSubtypeOfAny(ValueType.XsUntypedAtomic, ValueType.XsString))
            return value =>
            {
                var parsedDuration = DurationValue.FromString(value.GetValue().ToString());
                return parsedDuration != null
                    ? new SuccessResult<AtomicValue>(CreateDurationValue(parsedDuration))
                    : new ErrorResult<AtomicValue>(
                        $"Can not cast {value} to xs:duration",
                        "FORG0001"
                    );
            };

        return _ => new ErrorResult<AtomicValue>(
            "Casting not supported from given type to xs:duration or any of its derived types.",
            "XPTY0004"
        );
    }
}