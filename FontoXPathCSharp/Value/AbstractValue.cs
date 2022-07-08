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
        if (this is not T result)
            throw new InvalidCastException("Casting AbstractValue(" + Type + ") to " + typeof(T).Name);
        return result;
    }

    public ValueType GetValueType()
    {
        return Type;
    }
}