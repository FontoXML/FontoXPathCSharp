using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.Axes;

internal static class ContextNodeUtils
{
    public static void ValidateContextNode(AbstractValue value)
    {
        if (SubtypeUtils.IsSubtypeOf(value.GetValueType(), ValueType.Node)) return;
        throw new Exception("!!!");
    }
}