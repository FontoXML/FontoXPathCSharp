using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.DataTypes.Casting;

public static class CastToInteger
{
    public static CastingFunction ToInteger(ValueType from)
    {
        if (from.IsSubtypeOf(ValueType.XsBoolean))
            return value =>
                new SuccessResult<AtomicValue>(AtomicValue.Create(value.GetAs<BooleanValue>().Value ? 1 : 0,
                    ValueType.XsInteger));

        if (from.IsSubtypeOf(ValueType.XsNumeric))
            return value =>
            {
                if (value.GetValueType().IsSubtypeOf(ValueType.XsFloat))
                {
                    var floatValue = value.GetAs<FloatValue>().Value;

                    return !float.IsFinite(floatValue) || float.IsNaN(floatValue)
                        ? new ErrorResult<AtomicValue>($"can not cast '{value}' to xs:integer", "FOCA0002")
                        : new SuccessResult<AtomicValue>(AtomicValue.Create(Math.Truncate(floatValue),
                            ValueType.XsInteger));
                }

                if (value.GetValueType().IsSubtypeOf(ValueType.XsDouble))
                {
                    var doubleValue = value.GetAs<DoubleValue>().Value;

                    return !double.IsFinite(doubleValue) || double.IsNaN(doubleValue)
                        ? new ErrorResult<AtomicValue>($"Can not cast '{value}' to xs:integer", "FOCA0002")
                        : new SuccessResult<AtomicValue>(AtomicValue.Create(Math.Truncate(doubleValue),
                            ValueType.XsInteger));
                }

                if (value.GetValueType() == ValueType.XsDecimal)
                {
                    var decimalValue = value.GetAs<DecimalValue>().Value;
                    return new SuccessResult<AtomicValue>(AtomicValue.Create(Math.Truncate(decimalValue),
                        ValueType.XsInteger));
                }

                // Should be an integer type in this case.
                return new SuccessResult<AtomicValue>(AtomicValue.Create(value.GetValue(), ValueType.XsInteger));
            };

        return from.IsSubtypeOfAny(ValueType.XsString, ValueType.XsUntypedAtomic)
            ? value => new SuccessResult<AtomicValue>(AtomicValue.Create(value.GetValue(), ValueType.XsInteger))
            : _ => new ErrorResult<AtomicValue>(
                $"Casting not supported from {from} to xs:integer or any of its derived types.", "XPTY0004");
    }
}