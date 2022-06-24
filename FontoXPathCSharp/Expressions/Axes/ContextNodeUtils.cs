using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.Axes;

internal static class ContextNodeUtils
{
    public static NodePointer? ValidateContextNode(AbstractValue value)
    {
        if (value == null) 
            throw new Exception("XPDY0002: context is absent, it needs to be present to use axes.") ;
        if (!SubtypeUtils.IsSubtypeOf(value.GetValueType(), ValueType.Node))
            throw new Exception("XPTY0020: Axes can only be applied to nodes.");
        return value as NodePointer;
    }
}