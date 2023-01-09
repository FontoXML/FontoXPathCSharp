using System.Diagnostics;
using FontoXPathCSharp.Expressions.DataTypes.Builtins;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public abstract class AtomicValue : AbstractValue
{
    public static readonly AtomicValue TrueBoolean = Create(true, ValueType.XsBoolean);
    public static readonly AtomicValue FalseBoolean = Create(false, ValueType.XsBoolean);

    protected AtomicValue(ValueType type) : base(type)
    {
    }

    public override T GetAs<T>()
    {
        return typeof(T) == typeof(UntypedAtomicValue)
            ? (T)(object)new UntypedAtomicValue(GetValue())
            : base.GetAs<T>();
    }

    public abstract object GetValue();

    private bool Equals(AtomicValue other)
    {
        return GetValue().Equals(other.GetValue());
    }

    public override int GetHashCode()
    {
        return GetValue().GetHashCode();
    }

    public override string ToString()
    {
        return $"<Value>[type: {Type}, value: {GetValue()}]";
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((AtomicValue)obj);
    }

    // public static AtomicValue Create(bool value, ValueType type)
    // {
    //     
    // }
    //
    // public static AtomicValue Create(decimal value, ValueType type)
    // {
    //     
    // }
    //
    // public static AtomicValue Create(string value, ValueType type)
    // {
    //     
    // }
    //
    // public static AtomicValue Create(long value, ValueType type)
    // {
    //     
    // }
    //
    // public static AtomicValue Create(QName value, ValueType type)
    // {
    //     
    // }
    //
    // private static string PrintStackTrace(StackTrace st)
    // {
    //     return string.Concat("<-", st.GetFrames().Select(f => f.GetMethod()!.Name));
    // }

    public static AtomicValue Create<T>(T value, ValueType type)
    {
        if (!BuiltinDataTypes.Instance.BuiltinDataTypesByType.ContainsKey(type))
            throw new Exception($"Cannot create atomic value from type: {type}");

        return type switch
        {
            ValueType.XsBoolean => BooleanValue.CreateBooleanValue(value),
            ValueType.XsInteger
                or ValueType.XsPositiveInteger
                or ValueType.XsNegativeInteger
                or ValueType.XsNonPositiveInteger
                or ValueType.XsNonNegativeInteger
                or ValueType.XsByte
                or ValueType.XsUnsignedByte
                or ValueType.XsShort
                or ValueType.XsUnsignedShort
                or ValueType.XsInt
                or ValueType.XsUnsignedInt
                or ValueType.XsLong
                or ValueType.XsUnsignedLong => IntegerValue.CreateIntegerValue(value, type),
            ValueType.XsFloat => FloatValue.CreateFloatValue(value),
            ValueType.XsDouble => DoubleValue.CreateDoubleValue(value),
            ValueType.XsDecimal => DecimalValue.CreateDecimalValue(value),
            ValueType.XsQName => new QNameValue(value),
            ValueType.XsUntypedAtomic => new UntypedAtomicValue(value!),
            ValueType.XsString => StringValue.CreateStringValue(value),
            ValueType.XsDate
                or ValueType.XsDateTime
                or ValueType.XsGDay
                or ValueType.XsGMonth
                or ValueType.XsGMonthDay
                or ValueType.XsGYear
                or ValueType.XsGYearMonth
                or ValueType.XsTime => DateTimeValue.CreateDateTime(value, type),
            ValueType.XsDuration or
                ValueType.XsDayTimeDuration or
                ValueType.XsYearMonthDuration => DurationValue.CreateDuration(value, type),
            _ => throw new NotImplementedException($"Atomic Value for {type} is not implemented yet.")
        };
    }
}