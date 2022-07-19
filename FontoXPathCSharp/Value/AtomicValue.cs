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
            ValueType.XsBoolean => new BooleanValue((bool)(object)value!),
            ValueType.XsInt or ValueType.XsInteger => new IntValue((int)(object)value!),
            ValueType.XsFloat => new FloatValue((decimal)(object)value!),
            ValueType.XsDouble => new DoubleValue((decimal)(object)value!),
            ValueType.XsString => new StringValue((string)(object)value!),
            ValueType.XsQName => new QNameValue((QName)(object)value!),
            ValueType.XsUntypedAtomic => new UntypedAtomicValue(value!),
            _ => throw new ArgumentOutOfRangeException($"Atomic Value for {type} is not implemented yet.")
        };
    }

    public static readonly AtomicValue TrueBoolean = Create(true, ValueType.XsBoolean);
    public static readonly AtomicValue FalseBoolean = Create(false, ValueType.XsBoolean);
}