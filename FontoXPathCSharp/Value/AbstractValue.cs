namespace FontoXPathCSharp.Value;

public abstract class AbstractValue
{
    public ValueType Type;

    public AbstractValue(ValueType type)
    {
        Type = type;
    }
}