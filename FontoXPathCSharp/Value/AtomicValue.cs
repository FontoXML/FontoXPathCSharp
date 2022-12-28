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

    public static AtomicValue Create<T>(T value, ValueType type)
    {
        if (!BuiltinDataTypes.Instance.BuiltinDataTypesByType.ContainsKey(type))
            throw new Exception($"Cannot create atomic value from type: {type}");

        return type switch
        {
            ValueType.XsBoolean => new BooleanValue(value),
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
                or ValueType.XsUnsignedLong => new IntValue(value),
            ValueType.XsFloat => new FloatValue(value),
            ValueType.XsDouble or ValueType.XsDecimal => new DoubleValue(value),
            ValueType.XsQName => new QNameValue(value),
            ValueType.XsUntypedAtomic => new UntypedAtomicValue(value!),
            ValueType.XsString => new StringValue(value),
            ValueType.XsDate
                or ValueType.XsDateTime
                or ValueType.XsGDay
                or ValueType.XsGMonth
                or ValueType.XsGMonthDay
                or ValueType.XsGYear
                or ValueType.XsGYearMonth
                or ValueType.XsTime => DateTimeValue.CreateDateTime(value, type),
            _ => throw new NotImplementedException($"Atomic Value for {type} is not implemented yet.")
        };
    }
}