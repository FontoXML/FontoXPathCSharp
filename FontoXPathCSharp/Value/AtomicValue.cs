using FontoXPathCSharp.Value;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions;

public abstract class AtomicValue : AbstractValue
{
    public abstract object GetValue();
    
    public AtomicValue(ValueType type) : base(type)
    {
    }
}