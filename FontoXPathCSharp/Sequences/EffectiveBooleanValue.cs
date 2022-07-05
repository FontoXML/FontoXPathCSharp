using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Sequences;

internal static class EffectiveBooleanValue
{
    public static bool Compute(AbstractValue value)
    {
        if (value.GetValueType().IsSubtypeOf(ValueType.Node)) return true;

        if (value.GetValueType().IsSubtypeOf(ValueType.XsBoolean))
            return value.GetAs<BooleanValue>(ValueType.XsBoolean).Value;

        if (value.GetValueType().IsSubtypeOf(ValueType.XsString))
            return value.GetAs<StringValue>(ValueType.XsString).Value.Length > 0;

        if (value.GetValueType().IsSubtypeOf(ValueType.XsAnyUri))
            return value.GetAs<StringValue>(ValueType.XsAnyUri).Value.Length > 0;

        if (value.GetValueType().IsSubtypeOf(ValueType.XsUntypedAtomic))
            return value.GetAs<StringValue>(ValueType.XsUntypedAtomic).Value.Length > 0;

        if (value.GetValueType().IsSubtypeOf(ValueType.XsNumeric))
            return value.GetAs<IntValue>(ValueType.XsNumeric).Value > 0;

        throw new Exception("FORG0006");
    }
}