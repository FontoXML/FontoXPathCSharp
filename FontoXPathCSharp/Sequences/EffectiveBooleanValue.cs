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

        if (value.GetValueType().IsSubtypeOfAny(ValueType.XsString, ValueType.XsAnyUri))
            return value.GetAs<StringValue>().Value.Length > 0;

        if (value.GetValueType().IsSubtypeOf(ValueType.XsUntypedAtomic))
            return (Convert.ToString(value.GetAs<UntypedAtomicValue>().Value) ?? string.Empty).Length > 0;

        if (value.GetValueType().IsSubtypeOf(ValueType.XsNumeric))
            return value.GetValueType() switch
            {
                ValueType.XsFloat => value.GetAs<FloatValue>().Value > 0,
                ValueType.XsDouble => value.GetAs<DoubleValue>().Value > 0,
                ValueType.XsDecimal => value.GetAs<DecimalValue>().Value > 0,
                _ => value.GetAs<IntegerValue>().Value > 0
            };

        throw new XPathException("FORG0006", 
            $"Cannot determine the effective boolean value of a value with the type {value.GetValueType().Name()}"
            );
    }
}