using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.Axes;

internal static class ContextNodeUtils
{
    public static NodeValue ValidateContextNode(AbstractValue? value)
    {
        if (value == null)
            throw new Exception("XPDY0002: context is absent, it needs to be present to use axes.");
        if (!value.GetValueType().IsSubtypeOf(ValueType.Node))
            throw new Exception("XPTY0020: Axes can only be applied to nodes.");
        return (NodeValue)value;
    }
}