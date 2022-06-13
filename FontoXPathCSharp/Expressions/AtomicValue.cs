using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions;

public class AtomicValue<T>
{
    public ValueType type;
    public T value;
}