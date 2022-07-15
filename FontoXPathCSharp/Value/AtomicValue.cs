using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public abstract class AtomicValue : AbstractValue
{
    public AtomicValue(ValueType type) : base(type)
    {
    }

    public abstract object GetValue();

    protected bool Equals(AtomicValue other)
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

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }
}