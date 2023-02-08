using System.Text.RegularExpressions;
using FontoXPathCSharp.Value.InternalValues;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class DurationValue : AtomicValue, IComparable<DurationValue>
{
    public DurationValue(Duration duration) : base(duration.DurationType)
    {
        Value = duration;
    }

    public DurationValue(Duration duration, ValueType type) : base(type)
    {
        Value = duration;
    }

    public Duration Value { get; }

    public int CompareTo(DurationValue? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        return Value.CompareTo(other.Value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public override object GetValue()
    {
        return Value;
    }

    private bool Equals(DurationValue other)
    {
        return base.Equals(other) && Value.Equals(other.Value);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((DurationValue)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Value);
    }

    public static DurationValue? FromString(string? durationString, ValueType type)
    {
        if (durationString == null) return null;

        switch (type)
        {
            case ValueType.XsDuration:
                var dtd = DayTimeDurationFromString(durationString);
                var ymd = YearMonthDurationFromString(durationString);
                return new DurationValue(new Duration(
                    dtd != null ? dtd.Value.RawMilliSeconds : 0,
                    ymd != null ? ymd.Value.RawMonths : 0,
                    type
                ));
            case ValueType.XsYearMonthDuration:
                return YearMonthDurationFromString(durationString);
            case ValueType.XsDayTimeDuration:
                return DayTimeDurationFromString(durationString);
            default:
                return null;
        }
    }

    public static DurationValue YearMonthDurationFromParts(
        int years,
        int months,
        bool isPositive)
    {
        var totalMonths = years * 12 + months;
        return new DurationValue(new Duration(
            0,
            isPositive ? totalMonths : -totalMonths,
            ValueType.XsYearMonthDuration));
    }

    public static DurationValue? YearMonthDurationFromString(string? yearMonthDurationString)
    {
        if (yearMonthDurationString == null) return null;

        var regex = @"^(-)?P(\d+Y)?(\d+M)?(\d+D)?(?:T(\d+H)?(\d+M)?(\d+(\.\d*)?S)?)?$";
        var match = Regex.Match(yearMonthDurationString, regex);

        if (!match.Success) return null;

        var matches = match.Groups;

        var isPositive = matches[1].Value != "-";
        var years = matches[2].Success ? int.Parse(matches[2].Value[..^1]) : 0;
        var months = matches[3].Success ? int.Parse(matches[3].Value[..^1]) : 0;

        return YearMonthDurationFromParts(years, months, isPositive);
    }

    public static DurationValue DayTimeDurationFromParts(
        int days,
        int hours,
        int minutes,
        int seconds,
        int milliseconds,
        bool isPositive)
    {
        var totalMilliseconds = (long)(days * 86400 + hours * 3600 + minutes * 60 + seconds) * 1000 + milliseconds;
        return new DurationValue(new Duration(
            isPositive ? totalMilliseconds : -totalMilliseconds,
            0,
            ValueType.XsDayTimeDuration
        ));
    }

    public static DurationValue? DayTimeDurationFromString(string? dayTimeDurationString)
    {
        if (dayTimeDurationString == null) return null;

        var regex = @"^(-)?P(\d+Y)?(\d+M)?(\d+D)?(?:T(\d+H)?(\d+M)?(\d+(\.\d*)?S)?)?$";
        var match = Regex.Match(dayTimeDurationString, regex);

        if (!match.Success) return null;

        var matches = match.Groups;

        var isPositive = matches[1].Value != "-";
        var days = matches[4].Success ? int.Parse(matches[4].Value[..^1]) : 0;
        var hours = matches[5].Success ? int.Parse(matches[5].Value[..^1]) : 0;
        var minutes = matches[6].Success ? int.Parse(matches[6].Value[..^1]) : 0;
        var seconds = matches[7].Success ? int.Parse(matches[7].Value[..^1]) : 0;
        var secondFraction = matches[8].Success ? (int)(float.Parse(matches[8].Value) * 1000) : 0;

        return DayTimeDurationFromParts(days, hours, minutes, seconds, secondFraction, isPositive);
    }


    public static DurationValue FromDuration(DurationValue duration)
    {
        return new DurationValue((Duration)duration.GetValue(), ValueType.XsDuration);
    }

    public static DurationValue CreateDuration(object? value, ValueType type)
    {
        return (value switch
        {
            Duration duration => new DurationValue(duration, type),
            DurationValue durationValue => new DurationValue(durationValue.Value, type),
            string durationString => FromString(durationString, type),
            _ => throw new Exception($"Tried to create Duration value from invalid type: {(value != null ? value.GetType().Name : "null")}")
        })!;
    }
}