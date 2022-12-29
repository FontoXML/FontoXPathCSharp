using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public abstract class NumericValue : AtomicValue
{
    protected NumericValue(ValueType type) : base(type)
    {
        if (!type.IsSubtypeOf(ValueType.XsNumeric))
            throw new Exception("Cannot create a NumericValue with a type that does not inherit xs:numeric");
    }
}