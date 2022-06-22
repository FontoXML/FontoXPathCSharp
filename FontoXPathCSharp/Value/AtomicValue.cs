using FontoXPathCSharp.Value;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions;

public class AtomicValue : AbstractValue
{
    public AtomicValue(ValueType type) : base(type)
    {
    }
}