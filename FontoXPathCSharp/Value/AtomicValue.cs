using FontoXPathCSharp.Expressions.DataTypes.Builtins;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public abstract class AtomicValue : AbstractValue
{
    public AtomicValue(ValueType type) : base(type)
    {
    }

    public abstract object GetValue();

    private bool Equals(AtomicValue other)
    {
        return GetValue().Equals(other.GetValue());
    }
    
    public override string ToString()
    {
        return "<Value>[type: " + Type + ", value: " + GetValue() + "]";
    }
    
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((AtomicValue)obj);
    }

    public static AtomicValue Create<T>(T value, ValueType type)
    {
        if (!BuiltinDataTypes.Instance.BuiltinDataTypesByType.ContainsKey(type))
            throw new Exception($"Cannot create atomic value from type: {type}");

        return type switch
        {
            ValueType.XsBoolean => new BooleanValue(value),
            ValueType.XsInt or ValueType.XsInteger => new IntValue(value),
            ValueType.XsFloat => new FloatValue(value),
            ValueType.XsDouble => new DoubleValue(value),
            ValueType.XsQName => new QNameValue((QName)(object)value!),
            ValueType.XsUntypedAtomic => new UntypedAtomicValue(value!),
            ValueType.XsString => new StringValue(value),
            _ => throw new ArgumentOutOfRangeException($"Atomic Value for {type} is not implemented yet.")
        };
    }

    public static readonly AtomicValue TrueBoolean = Create(true, ValueType.XsBoolean);
    public static readonly AtomicValue FalseBoolean = Create(false, ValueType.XsBoolean);
}