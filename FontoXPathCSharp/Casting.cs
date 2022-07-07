using FontoXPathCSharp.Value;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp;

public static class Casting
{
    public static AbstractValue CastToType(this AbstractValue value, ValueType type)
    {
        if (value.GetValueType() == type)
            return value;
        
        throw new NotImplementedException("CastToType");
    }
}