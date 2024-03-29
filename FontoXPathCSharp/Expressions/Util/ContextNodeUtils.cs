using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.Util;

internal static class ContextNodeUtils<TNode> where TNode : notnull
{
    public static TNode ValidateContextNode(AbstractValue? value)
    {
        if (value == null)
            throw new XPathException("XPDY0002", "Context is absent, it needs to be present to use axes.");
        if (!value.GetValueType().IsSubtypeOf(ValueType.Node))
            throw new XPathException("XPTY0020", "Axes can only be applied to nodes.");
        return value.GetAs<NodeValue<TNode>>().Value;
    }
}