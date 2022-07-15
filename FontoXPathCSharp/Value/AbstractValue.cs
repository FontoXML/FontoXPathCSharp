using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public abstract class AbstractValue
{
    protected readonly ValueType Type;

    protected AbstractValue(ValueType type)
    {
        Type = type;
    }

    public T GetAs<T>() where T : AbstractValue
    {
        return this as T ?? throw new InvalidCastException($"Casting AbstractValue({Type}) to {typeof(T).Name}");
    }

    public ValueType GetValueType()
    {
        return Type;
    }

    public AtomicValue CastToType(ValueType type)
    {
        return GetValueType().IsSubtypeOf(ValueType.XsAnyAtomicType)
            ? TypeCasting.CastToType(GetAs<AtomicValue>(), type)
            : throw new XPathException("Can't cast a non-atomic value.");
    }
}