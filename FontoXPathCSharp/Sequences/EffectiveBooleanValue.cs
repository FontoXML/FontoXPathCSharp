
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

static class EffectiveBooleanValue
{
    public static bool Compute(AbstractValue value)
    {

        if (SubtypeUtils.IsSubtypeOf(value.GetValueType(), ValueType.Node))
        {
            return true;
        }

        if (SubtypeUtils.IsSubtypeOf(value.GetValueType(), ValueType.XsString))
        {
            return value.GetAs<StringValue>(ValueType.XsString).Value.Length > 0;
        }
        if (SubtypeUtils.IsSubtypeOf(value.GetValueType(), ValueType.XsAnyUri))
        {
            return value.GetAs<StringValue>(ValueType.XsAnyUri).Value.Length > 0;
        }
        if (SubtypeUtils.IsSubtypeOf(value.GetValueType(), ValueType.XsUntypedAtomic))
        {
            return value.GetAs<StringValue>(ValueType.XsUntypedAtomic).Value.Length > 0;
        }

        if (SubtypeUtils.IsSubtypeOf(value.GetValueType(), ValueType.XsNumeric))
        {
            return value.GetAs<IntValue>(ValueType.XsNumeric).Value > 0;
        }

        throw new Exception("FORG0006");
    }
}