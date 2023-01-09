using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value.InternalValues;

public class DateTime : IComparable<DateTime>, IComparable
{
    private readonly DateTimeOffset _dateTime;
    private readonly bool _hasTimezone;

    public DateTime(
        int years,
        int months,
        int days,
        int hours,
        int minutes,
        int seconds,
        float secondFraction,
        TimeSpan? timezone,
        bool hasTimezone,
        ValueType type)
    {
        _hasTimezone = hasTimezone && timezone != null;
        _dateTime = new DateTimeOffset(
            years,
            months,
            days,
            hours,
            minutes,
            seconds,
            (int)(secondFraction * 1000),
            timezone ?? TimeSpan.Zero
        );
        GetValueType = type;
    }

    public DateTime(DateTimeOffset dateTime, bool hasTimezone, ValueType type)
    {
        _hasTimezone = hasTimezone;
        _dateTime = dateTime;
        GetValueType = type;
    }

    public int GetDay => _dateTime.Day;
    public int GetSeconds => _dateTime.Second;
    public int GetHours => _dateTime.Hour;
    public int GetMinutes => _dateTime.Minute;
    public int GetMonth => _dateTime.Month;
    public int GetYear => _dateTime.Year;
    public float GetSecondFraction => _dateTime.Millisecond / 1000f;
    public bool IsPositive => GetYear >= 0;
    public TimeSpan GetTimezone => _dateTime.Offset;

    public bool HasTimezone => _hasTimezone;

    public ValueType GetValueType { get; }

    public int CompareTo(object? obj)
    {
        return obj is DateTime dateTime ? CompareTo(dateTime) : 1;
    }
    
    public int CompareTo(DateTime? other)
    {
        return other != null ? _dateTime.CompareTo(other._dateTime) : 1;
    }

    public override string ToString()
    {
        return GetValueType switch
        {
            ValueType.XsDateTime =>
                $"{ConvertYearToString(GetYear)}-{ConvertToTwoCharString(GetMonth)}-{ConvertToTwoCharString(GetDay)}T{ConvertToTwoCharString(GetHours)}:{ConvertToTwoCharString(GetMinutes)}:{ConvertSecondsToString(GetSeconds + GetSecondFraction)}{TimezoneToString(GetTimezone)}",
            ValueType.XsDate =>
                $"{ConvertYearToString(GetYear)}-{ConvertToTwoCharString(GetMonth)}-{ConvertToTwoCharString(GetDay)}{TimezoneToString(GetTimezone)}",
            ValueType.XsTime =>
                $"{ConvertToTwoCharString(GetHours)}:{ConvertToTwoCharString(GetMinutes)}:{ConvertSecondsToString(GetSeconds + GetSecondFraction)}{TimezoneToString(GetTimezone)}",
            ValueType.XsGDay => $"---{ConvertToTwoCharString(GetDay)}{TimezoneToString(GetTimezone)}",
            ValueType.XsGMonth => $"--{ConvertToTwoCharString(GetMonth)}{TimezoneToString(GetTimezone)}",
            ValueType.XsGMonthDay =>
                $"--{ConvertToTwoCharString(GetMonth)}-{ConvertToTwoCharString(GetDay)}{TimezoneToString(GetTimezone)}",
            ValueType.XsGYear => $"{ConvertYearToString(GetYear)}{TimezoneToString(GetTimezone)}",
            ValueType.XsGYearMonth =>
                $"{ConvertYearToString(GetYear)}-{ConvertToTwoCharString(GetMonth)}{TimezoneToString(GetTimezone)}",
            _ => ""
        };
    }

    private static string ConvertToTwoCharString(int value)
    {
        var valueString = value + "";
        return valueString.PadLeft(2, '0');
    }

    private static string ConvertYearToString(int year)
    {
        var yearString = year + "";
        var isNegative = year < 0;
        if (isNegative) yearString = yearString[1..];
        return (isNegative ? "-" : "") + yearString.PadLeft(4, '0');
    }

    private static string ConvertSecondsToString(float seconds)
    {
        var secondsString = seconds + "";
        if (secondsString.Split('.')[0].Length == 1)
            secondsString = secondsString.PadLeft(secondsString.Length + 1, '0');
        return secondsString;
    }

    private string TimezoneToString(TimeSpan timezone)
    {
        if (!_hasTimezone) return "";

        if (IsUtc(timezone)) return "Z";
        return $"{(timezone.TotalSeconds >= 0 ? '+' : '-')}{ConvertToTwoCharString(Math.Abs(timezone.Hours))}" +
               $":{ConvertToTwoCharString(Math.Abs(timezone.Minutes))}";
    }

    private static bool IsUtc(TimeSpan timezone)
    {
        return timezone.Hours == 0 && timezone.Minutes == 0;
    }

    public static Duration operator -(DateTime a, DateTime b)
    {
        var result = a._dateTime - b._dateTime;
        return new Duration(result);
    }

    public static DateTime operator +(DateTime dateTime, Duration duration)
    {
        var result = dateTime._dateTime + new TimeSpan(duration.RawSeconds * TimeSpan.TicksPerSecond);
        return new DateTime(result, dateTime.HasTimezone, dateTime.GetValueType);
    }

    public static DateTime operator -(DateTime dateTime, Duration duration)
    {
        var result = dateTime._dateTime - new TimeSpan(duration.RawSeconds * TimeSpan.TicksPerSecond);
        return new DateTime(result, dateTime.HasTimezone, dateTime.GetValueType);
    }
}