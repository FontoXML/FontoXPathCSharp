namespace FontoXPathCSharp.Value;

public abstract class AbstractValue
{
    protected readonly ValueType Type;

    protected AbstractValue(ValueType type)
    {
        Type = type;
    }
}