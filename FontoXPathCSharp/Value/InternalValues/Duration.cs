using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value.InternalValues;

public class Duration : IComparable<Duration>, IComparable
{
    private static readonly int[,] MonthsToMinMaxValues =
    {
        { 28, 31 },
        { 59, 62 },
        { 89, 91 },
        { 120, 123 },
        { 150, 153 },
        { 181, 184 },
        { 212, 215 },
        { 242, 245 },
        { 273, 276 },
        { 303, 306 },
        { 334, 337 },
        { 365, 366 }
    };

    public Duration(long milliseconds, int months, ValueType type)
    {
        RawMilliSeconds = milliseconds;
        RawMonths = months;
        DurationType = type;
    }

    public Duration(TimeSpan timeSpan)
    {
        DurationType = ValueType.XsDayTimeDuration;
        RawMilliSeconds = (long)timeSpan.TotalMilliseconds;
    }

    public ValueType DurationType { get; }

    public int Days => RawSeconds / 86400;

    public int Hours => RawSeconds % 86400 / 3600;

    public int Minutes => RawSeconds % 3600 / 60;

    public int Months => RawMonths % 12;

    public int RawMonths { get; }

    public int RawSeconds => (int)(RawMilliSeconds / 1000);

    public int Seconds => RawSeconds % 60;

    public long RawMilliSeconds { get; }

    public int Years => RawMonths / 12;

    public bool IsPositive => RawMilliSeconds >= 0 && RawMonths >= 0;


    public int CompareTo(object? obj)
    {
        return obj is Duration duration ? CompareTo(duration) : 1;
    }

    public int CompareTo(Duration? other)
    {
        if (other == null) return 1;

        if (IsPositive && !other.IsPositive) return 1;

        if (!IsPositive && other.IsPositive) return -1;

        if (Equals(other)) return 0;

        var thisMinDays = ComputeMinDays(this);
        var thisMaxDays = ComputeMaxDays(this);
        var otherMinDays = ComputeMinDays(other);
        var otherMaxDays = ComputeMaxDays(other);

        if (thisMinDays == otherMinDays && thisMaxDays == otherMaxDays)
        {
            var thisSecondsWithoutDays = Hours * 3600 + Minutes * 60 + Seconds;
            var otherSecondsWithoutDays = other.Hours * 3600 + other.Minutes * 60 + other.Seconds;
            return thisSecondsWithoutDays > otherSecondsWithoutDays ? 1 :
                thisSecondsWithoutDays < otherSecondsWithoutDays ? -1 : 0;
        }

        var bothPositive = IsPositive && other.IsPositive;
        if (thisMinDays > otherMaxDays) return bothPositive ? 1 : -1;

        if (thisMaxDays < otherMinDays) return bothPositive ? -1 : 1;

        return 0;
    }

    private static int ComputeMaxDays(Duration duration)
    {
        var years = Math.Abs(duration.Years);
        var months = Math.Abs(duration.Months);
        var maxNumberOfLeapYears = (int)Math.Ceiling(years / 4f);

        return Math.Abs(duration.Days) +
               (months == 0 ? 0 : MonthsToMinMaxValues[months - 1, 1]) +
               maxNumberOfLeapYears * 366 +
               (years - maxNumberOfLeapYears) * 365;
    }

    private static int ComputeMinDays(Duration duration)
    {
        var years = Math.Abs(duration.Years);
        var months = Math.Abs(duration.Months);
        var minNumberOfLeapYears = (int)Math.Floor(years / 4f);

        return Math.Abs(duration.Days) +
               (months == 0 ? 0 : MonthsToMinMaxValues[months - 1, 0]) +
               minNumberOfLeapYears * 366 +
               (years - minNumberOfLeapYears) * 365;
    }

    public override string ToString()
    {
        return $"{(IsPositive ? "P" : "-P")}{ToStringWithoutP()}";
    }

    public string ToStringWithoutP()
    {
        return DurationType switch
        {
            ValueType.XsDuration => DurationToStringWithoutP(),
            ValueType.XsDayTimeDuration => DayTimeToStringWithoutP(),
            ValueType.XsYearMonthDuration => YearMonthToStringWithoutP(),
            _ => throw new Exception($"Cannot create a duration string out of {DurationType}")
        };
    }

    private string DurationToStringWithoutP()
    {
        var tym = YearMonthToStringWithoutP();
        var tdt = DayTimeToStringWithoutP();
        return tym == "0M" ? tdt : tdt == "T0S" ? tym : $"{tym}{tdt}";
    }

    private string YearMonthToStringWithoutP()
    {
        var years = Math.Abs(Years);
        var months = Math.Abs(Months);
        var stringValue = $"{(years != 0 ? $"{years.ToString()}Y" : "")}" +
                          $"{(months != 0 ? $"{months.ToString()}M" : "")}";

        return stringValue != "" ? "0M" : stringValue;
    }

    private string DayTimeToStringWithoutP()
    {
        var days = Math.Abs(Days);
        var hours = Math.Abs(Hours);
        var minutes = Math.Abs(Minutes);
        var seconds = Math.Abs(Seconds);
        var dayPart = days != 0 ? $"{days.ToString()}D" : "";
        var timePart = (hours != 0 ? $"{hours}H" : "") +
                       (minutes != 0 ? $"{minutes}M" : "") +
                       (seconds != 0 ? $"{seconds}S" : "");

        return dayPart == "" && timePart == "" ? $"{dayPart}T{timePart}" :
            dayPart == "" ? dayPart :
            timePart == "" ? $"T{timePart}" : "T0S";
    }
}