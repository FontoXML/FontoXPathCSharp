using FontoXPathCSharp.Value;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions;

public abstract class AtomicValue : AbstractValue
{
    public AtomicValue(ValueType type) : base(type)
    {
    }

    public abstract object GetValue();
}