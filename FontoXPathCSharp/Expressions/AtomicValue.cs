using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions;

public class AtomicValue<T>
{
    public AtomicValue(T value, ValueType type)
    {
        this.Type = type;
        this.Value = value;
    }

    public ValueType Type { get; }

    public T Value { get; }
}