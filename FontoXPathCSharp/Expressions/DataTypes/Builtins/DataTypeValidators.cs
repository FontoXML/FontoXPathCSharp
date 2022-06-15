using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.DataTypes.Builtins;

public class DataTypeValidators
{
    public static Func<string, bool>? GetValidatorForType(ValueType valueType)
    {
        // TODO: Implement the actual valdator functions
        // they are only a tiny part of the spec and not fundamental to things working, so not relevant for now.
        return _ => true;
    }
}