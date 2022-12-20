using System.Text.RegularExpressions;
using System.Xml;
using DateTime = FontoXPathCSharp.Value.InternalValues.DateTime;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class DateTimeValue : AtomicValue
{
    private readonly DateTime _dateTime;

    public DateTimeValue(DateTime dateTime) : base(dateTime.GetValueType)
    {
        _dateTime = dateTime;
    }

    public DateTimeValue(DateTimeValue dateTimeValue) : base(dateTimeValue.GetValueType())
    {
        _dateTime = dateTimeValue._dateTime;
    }

    public DateTimeValue(
        int years,
        int months,
        int days,
        int hours,
        int minutes,
        int seconds,
        float secondFraction,
        TimeSpan? timezone,
        bool hasTimezone,
        ValueType type) : base(type)
    {
        _dateTime = new DateTime(years, months, days, hours, minutes, seconds, secondFraction, timezone, hasTimezone,
            type);
    }


    public int GetDay => _dateTime.GetDay;
    public int GetSeconds => _dateTime.GetSeconds;
    public int GetHours => _dateTime.GetHours;
    public int GetMinutes => _dateTime.GetMinutes;
    public int GetMonth => _dateTime.GetMonth;
    public int GetYear => _dateTime.GetYear;

    public float GetSecondFraction => _dateTime.GetSecondFraction;
    public bool IsPositive => _dateTime.IsPositive;
    public TimeSpan GetTimezone => _dateTime.GetTimezone;
    public bool HasTimezone => _dateTime.HasTimezone;

    public override object GetValue()
    {
        return _dateTime;
    }

    public override string ToString()
    {
        return _dateTime.ToString();
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
                    HasTimezone,
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
                    HasTimezone,
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
                    HasTimezone,
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
                    HasTimezone,
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
                    HasTimezone,
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
                    HasTimezone,
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
                    HasTimezone,
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
                    HasTimezone,
                    ValueType.XsDateTime
                );
        }
    }


    public static DateTimeValue FromString(string dateTimeString, ValueType type)
    {
        //TODO: Figure out why this insists on adding your own timezone.
        var dateTime = XmlConvert.ToDateTimeOffset(dateTimeString);

        var matches = Regex.Match(
                dateTimeString,
                @"^(?:(-?\d{4,}))?(?:--?(\d\d))?(?:-{1,3}(\d\d))?(T)?(?:(\d\d):(\d\d):(\d\d))?(\.\d+)?(Z|(?:[+-]\d\d:\d\d))?$")
            .Groups;
        var hasTimezone = matches[9].Success;

        return new DateTimeValue(new DateTime(dateTime, hasTimezone, type));
    }

    public static DateTimeValue? CreateDateTime(object value, ValueType type)
    {
        return value switch
        {
            DateTime dateTime => new DateTimeValue(dateTime),
            DateTimeValue dateTimeValue => new DateTimeValue(dateTimeValue),
            string s => FromString(s, type),
            _ => null
        };
    }
}