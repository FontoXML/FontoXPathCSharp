namespace FontoXPathCSharp.Value;

public abstract class AbstractValue
{
    protected readonly ValueType Type;

    protected AbstractValue(ValueType type)
    {
        Type = type;
    }

    public T? GetAs<T>(ValueType type) where T : AbstractValue
    {
        if (Type == type) return (T?) this;
        return null;
    }
}