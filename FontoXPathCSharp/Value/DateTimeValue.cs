using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class DateTimeValue : AbstractValue
{
    private readonly DateTime _dateTime;
    private readonly TimeZoneInfo _timeZone;

    public DateTimeValue(DateTime dateTime, TimeZoneInfo timeZone, ValueType type) : base(type)
    {
        _dateTime = dateTime;
        _timeZone = timeZone;
    }

    public int GetDay => _dateTime.Day;
    public int GetSeconds => _dateTime.Second;
    public int GetHours => _dateTime.Hour;
    public int GetMinutes => _dateTime.Minute;
    public int GetMonth => _dateTime.Month;
    public int GetYear => _dateTime.Year;
    public bool IsPositive => _dateTime.Year >= 0;
    public string GetTimezone => _timeZone.Id;

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
        if (isNegative) {
            yearString = yearString[1..];
        }
        return (isNegative ? "-" : "") + yearString.PadLeft(4, '0');
    }

    private static string ConvertToTwoCharString(int value)
    {
        var valueString = value + "";
        return valueString.PadLeft(2, '0');
    }
    
    private static string ConvertSecondsToString(int seconds) {
        var secondsString = seconds + "";
        if (secondsString.Split('.')[0].Length == 1) {
            secondsString = secondsString.PadLeft(secondsString.Length + 1, '0');
        }
        return secondsString;
    }

    private static bool IsUTC(TimeZoneInfo timeZone)
    {
        var utcAliases = new[]
        {
            "Etc/UTC",
            "Etc/UCT",
            "Etc/Universal",
            "Etc/Zulu",
            "UCT",
            "UTC",
            "Universal",
            "Zulu"
        };
        
        return utcAliases.Contains(timeZone.Id);
    }
    
    // private static string TimezoneToString(TimeZoneInfo timezone) {
    //     if (IsUTC(timezone)) {
    //         return "Z";
    //     }
    //
    //     return (timezone.IsPositive() ? '+' : '-') +
    //            ConvertToTwoCharString(Math.Abs(timezone.Hours)) +
    //            ':' +
    //            ConvertToTwoCharString(Math.Abs(timezone.GetMinutes));
    // }

}