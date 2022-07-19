using FontoXPathCSharp.Value;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.DataTypes.Casting;

public delegate Result<string> IntermediateStringResultFunction(AbstractValue input);

public class CastToStringLikeType
{
    public static IntermediateStringResultFunction ToStringLikeType(InstanceOfFunction instanceOf)
    {
        if (instanceOf(ValueType.XsString, ValueType.XsUntypedAtomic))
            return value => new SuccessResult<string>(value + "");

        if (instanceOf(ValueType.XsAnyUri)) return value => new SuccessResult<string>(value.ToString() ?? string.Empty);

        if (instanceOf(ValueType.XsQName))
            return value =>
            {
                var qNameValue = value.GetAs<QNameValue>().Value;
                return new SuccessResult<string>(qNameValue.Prefix != null
                    ? $"{qNameValue.Prefix}:{qNameValue.LocalName}"
                    : qNameValue.LocalName);
            };

        if (instanceOf(ValueType.XsNotation))
            return value => new SuccessResult<string>(value.ToString() ?? string.Empty);

        if (instanceOf(ValueType.XsNumeric))
        {
            if (instanceOf(ValueType.XsInteger, ValueType.XsDecimal))
                return value =>
                    new SuccessResult<string>(value.ToString() ?? string.Empty);

            if (instanceOf(ValueType.XsFloat))
                return value =>
                {
                    var floatValue = value.GetAs<FloatValue>().Value;
                    
                    if (!float.IsFinite(floatValue))
                        return new SuccessResult<string>($"{(floatValue < 0 ? "-" : "")}INF");

                    if (float.IsNaN(floatValue)) return new SuccessResult<string>("NaN");
                    //Yes, this should be precisely equal to -0.0, it's a special case.
                    if (floatValue == -0.0f) return new SuccessResult<string>("-0");
                    // C#'s notation for large numbers uses E+, XPath's uses E
                    return new SuccessResult<string>((floatValue + "").Replace("E+", "E"));
                };

            if (instanceOf(ValueType.XsDouble))
                return value =>
                {
                    double doubleValue;
                    try
                    {
                        doubleValue = value.GetAs<DoubleValue>().Value;
                    }
                    catch(Exception ex)
                    {
                        throw new InvalidCastException($"Type: {value.GetType().Name}, Value: {value}", ex);
                    }

                    if (!double.IsFinite(doubleValue))
                        return new SuccessResult<string>($"{(doubleValue < 0 ? "-" : "")}INF");

                    if (double.IsNaN(doubleValue)) return new SuccessResult<string>("NaN");
                    if (doubleValue == -0.0) return new SuccessResult<string>("-0");
                    // C#'s notation for large numbers uses E+, XPath's uses E
                    return new SuccessResult<string>((doubleValue + "").Replace("E+", "E"));
                };
        }

        if (
            instanceOf(
                ValueType.XsDateTime,
                ValueType.XsDate,
                ValueType.XsTime,
                ValueType.XsGDay,
                ValueType.XsGMonth,
                ValueType.XsGMonthDay,
                ValueType.XsGYear,
                ValueType.XsGYearMonth,
                ValueType.XsYearMonthDuration,
                ValueType.XsDayTimeDuration,
                ValueType.XsDuration)
        ) return value => new SuccessResult<string>(value.ToString() ?? string.Empty);

        if (instanceOf(ValueType.XsHexBinary))
            return value => new SuccessResult<string>(value.ToString()?.ToUpper() ?? string.Empty);

        return value => new SuccessResult<string>(value.ToString() ?? string.Empty);
    }
}