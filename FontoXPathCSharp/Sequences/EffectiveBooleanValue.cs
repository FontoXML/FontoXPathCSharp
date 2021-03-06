using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Sequences;

internal static class EffectiveBooleanValue
{
    public static bool GetEffectiveBooleanValue(this AbstractValue value)
    {
        if (value.GetValueType().IsSubtypeOf(ValueType.Node)) return true;

        if (value.GetValueType().IsSubtypeOf(ValueType.XsBoolean))
            return value.GetAs<BooleanValue>().Value;

        if (value.GetValueType().IsSubtypeOf(ValueType.XsString))
            return value.GetAs<StringValue>().Value.Length > 0;

        if (value.GetValueType().IsSubtypeOf(ValueType.XsAnyUri))
            return value.GetAs<StringValue>().Value.Length > 0;

        if (value.GetValueType().IsSubtypeOf(ValueType.XsUntypedAtomic))
            return value.GetAs<StringValue>().Value.Length > 0;

        if (value.GetValueType().IsSubtypeOf(ValueType.XsNumeric))
            return value.GetAs<IntValue>().Value > 0;

        throw new Exception("FORG0006");
    }
}