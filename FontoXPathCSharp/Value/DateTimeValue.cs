using System.Text.RegularExpressions;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class DateTimeValue : AtomicValue
{
    private readonly DateTimeOffset _dateTime;

    // public DateTimeValue(DateTime dateTime, TimeZoneInfo timeZone, ValueType type) : base(type)
    // {
    //     _dateTime = dateTime;
    //     _timeZone = timeZone;
    // }

    public DateTimeValue(
        int years,
        int months,
        int days,
        int hours,
        int minutes,
        int seconds,
        float secondFraction,
        TimeSpan? timezone,
        ValueType type) : base(type)
    {
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
    }

    public int GetDay => _dateTime.Day;
    public int GetSeconds => _dateTime.Second;
    public int GetHours => _dateTime.Hour;
    public int GetMinutes => _dateTime.Minute;
    public int GetMonth => _dateTime.Month;
    public int GetYear => _dateTime.Year;

    public float GetSecondFraction => _dateTime.Millisecond / 1000f;
    public bool IsPositive => _dateTime.Year >= 0;
    public TimeSpan GetTimezone => _dateTime.Offset;

    public override object GetValue()
    {
        return _dateTime;
    }

    public override string ToString()
    {
        return Type switch
        {
            ValueType.XsDateTime => ConvertYearToString(GetYear) +
                                    '-' +
                                    ConvertToTwoCharString(GetMonth) +
                                    '-' +
                                    ConvertToTwoCharString(GetDay) +
                                    'T' +
                                    ConvertToTwoCharString(GetHours) +
                                    ':' +
                                    ConvertToTwoCharString(GetMinutes) +
                                    ':' +
                                    ConvertSecondsToString(GetSeconds),
            // + TimezoneToString(_timeZone),
            ValueType.XsDate => throw new NotImplementedException(),
            ValueType.XsTime => throw new NotImplementedException(),
            ValueType.XsGDay => throw new NotImplementedException(),
            ValueType.XsGMonth => throw new NotImplementedException(),
            ValueType.XsGMonthDay => throw new NotImplementedException(),
            ValueType.XsGYear => throw new NotImplementedException(),
            ValueType.XsGYearMonth => throw new NotImplementedException(),
            _ => throw new Exception("Unexpected subType")
        };
    }

    private static string ConvertYearToString(int year)
    {
        var yearString = year + "";
        var isNegative = yearString.StartsWith('-');
        if (isNegative) yearString = yearString[1..];
        return (isNegative ? "-" : "") + yearString.PadLeft(4, '0');
    }

    private static string ConvertToTwoCharString(int value)
    {
        var valueString = value + "";
        return valueString.PadLeft(2, '0');
    }

    private static string ConvertSecondsToString(int seconds)
    {
        var secondsString = seconds + "";
        if (secondsString.Split('.')[0].Length == 1)
            secondsString = secondsString.PadLeft(secondsString.Length + 1, '0');
        return secondsString;
    }


    public DateTimeValue ConvertToType(ValueType type)
    {
        // xs:date       xxxx-xx-xxT00:00:00
        // xs:time       1972-12-31Txx:xx:xx
        // xs:gYearMonth xxxx-xx-01T00:00:00
        // xs:gYear      xxxx-01-01T00:00:00
        // xs:gMonthDay  1972-xx-xxT00:00:00
        // xs:gMonth     1972-xx-01T00:00:00
        // xs:gDay       1972-12-xxT00:00:00

        switch (type)
        {
            case ValueType.XsGDay:
                return new DateTimeValue(
                    1972,
                    12,
                    GetDay,
                    0,
                    0,
                    0,
                    0,
                    GetTimezone,
                    ValueType.XsGDay
                );
            case ValueType.XsGMonth:
                return new DateTimeValue(
                    1972,
                    GetMonth,
                    1,
                    0,
                    0,
                    0,
                    0,
                    GetTimezone,
                    ValueType.XsGMonth
                );
            case ValueType.XsGYear:
                return new DateTimeValue(
                    GetYear,
                    1,
                    1,
                    0,
                    0,
                    0,
                    0,
                    GetTimezone,
                    ValueType.XsGYear
                );
            case ValueType.XsGMonthDay:
                return new DateTimeValue(
                    1972,
                    GetMonth,
                    GetDay,
                    0,
                    0,
                    0,
                    0,
                    GetTimezone,
                    ValueType.XsGMonthDay
                );
            case ValueType.XsGYearMonth:
                return new DateTimeValue(
                    GetYear,
                    GetMonth,
                    1,
                    0,
                    0,
                    0,
                    0,
                    GetTimezone,
                    ValueType.XsGYearMonth
                );
            case ValueType.XsTime:
                return new DateTimeValue(
                    1972,
                    12,
                    31,
                    GetHours,
                    GetMinutes,
                    GetSeconds,
                    GetSecondFraction,
                    GetTimezone,
                    ValueType.XsTime
                );
            case ValueType.XsDate:
                return new DateTimeValue(
                    GetYear,
                    GetMonth,
                    GetDay,
                    0,
                    0,
                    0,
                    0,
                    GetTimezone,
                    ValueType.XsDate
                );
            case ValueType.XsDateTime:
            default:
                return new DateTimeValue(
                    GetYear,
                    GetMonth,
                    GetDay,
                    GetHours,
                    GetMinutes,
                    GetSeconds,
                    GetSecondFraction,
                    GetTimezone,
                    ValueType.XsDateTime
                );
        }
    }


    public static DateTimeValue FromString(string dayTimeDurationString)
    {
        var match = Regex.Matches(
            dayTimeDurationString,
            @"^(?:(-?\d{4,}))?(?:--?(\d\d))?(?:-{1,3}(\d\d))?(T)?(?:(\d\d):(\d\d):(\d\d))?(\.\d+)?(Z|(?:[+-]\d\d:\d\d))?$"
        );


        var years = ParseMatch(match[1]);
        var months = ParseMatch(match[2]);
        var days = ParseMatch(match[3]);
        var t = match[4];
        var hours = ParseMatch(match[5]);
        var minutes = ParseMatch(match[6]);
        var seconds = ParseMatch(match[7]);
        var secondFraction = match[8] != Match.Empty ? Convert.ToSingle(match[8]) : 0;
        var timezone = match[9] != Match.Empty ? DurationValue.FromTimezoneString(match[9].Value) : null;
        if (years != null && years is < -271821 or > 273860)
            // These are the JavaScript bounds for date (https://tc39.github.io/ecma262/#sec-time-values-and-time-range)
            throw new XPathException(
                "FODT0001",
                "Datetime year is out of bounds"
            );

        if (t != Match.Empty)
            // There is a T separating the date and time components -> dateTime
            return new DateTimeValue(
                years ?? 0,
                months ?? 0,
                days ?? 0,
                hours ?? 0,
                minutes ?? 0,
                seconds ?? 0,
                secondFraction,
                timezone,
                ValueType.XsDateTime
            );

        if (hours != null && minutes != null && seconds != null)
            return new DateTimeValue(
                1972,
                12,
                31,
                (int)hours,
                (int)minutes,
                (int)seconds,
                secondFraction,
                timezone,
                ValueType.XsTime
            );

        if (years != null && months != null && days != null)
            // There is no T separator, but there is a complete date component -> date
            return new DateTimeValue((int)years, (int)months, (int)days, 0, 0, 0, 0, timezone, ValueType.XsDate);

        if (years != null && months != null)
            // There is no complete date component, but there is a year and a month -> gYearMonth
            return new DateTimeValue((int)years, (int)months, 1, 0, 0, 0, 0, timezone, ValueType.XsGYearMonth);

        if (months != null && days != null)
            // There is no complete date component, but there is a month and a day -> gMonthDay
            return new DateTimeValue(1972, (int)months, (int)days, 0, 0, 0, 0, timezone, ValueType.XsGMonthDay);

        if (years != null)
            // There is only a year -> gYear
            return new DateTimeValue((int)years, 1, 1, 0, 0, 0, 0, timezone, ValueType.XsGYear);

        if (months != null)
            // There is only a month -> gMonth
            return new DateTimeValue(1972, (int)months, 1, 0, 0, 0, 0, timezone, ValueType.XsGMonth);

        // There is only one option left -> gDay
        return new DateTimeValue(1972, 12, days ?? 0, 0, 0, 0, 0, timezone, ValueType.XsGDay);
    }

    private static int? ParseMatch(Match match)
    {
        return match != Match.Empty ? Convert.ToInt32(match.Value, 10) : null;
    }
}