using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.DataTypes.Casting;

public class CastToDecimal
{
    public static CastingFunction ToDecimal(ValueType from)
    {
        if (from.IsSubtypeOf(ValueType.XsInteger))
            return value => new SuccessResult<AtomicValue>(AtomicValue.Create(value, ValueType.XsDecimal));
        if (from.IsSubtypeOf(ValueType.XsFloat))
            return value =>
            {
                var floatValue = value.GetAs<FloatValue>().Value;
                if (float.IsNaN(floatValue) || !float.IsFinite(floatValue))
                    return new ErrorResult<AtomicValue>($"FOCA0002: Can not cast {value} to xs:decimal");

                if (Math.Abs(floatValue) > float.MaxValue)
                    return new ErrorResult<AtomicValue>(
                        $"FOAR0002: Can not cast {value} to xs:decimal, it is out of bounds for JavaScript numbers");

                return new SuccessResult<AtomicValue>(AtomicValue.Create(floatValue, ValueType.XsDecimal));
            };
        if (from.IsSubtypeOf(ValueType.XsBoolean))
            return value =>
                new SuccessResult<AtomicValue>(
                    AtomicValue.Create(value.GetAs<BooleanValue>().Value ? 1 : 0, ValueType.XsDecimal));

        if (from.IsSubtypeOfAny(ValueType.XsString, ValueType.XsUntypedAtomic))
            return value =>
            {
                var stringValue = value.GetValueType().IsSubtypeOf(ValueType.XsString)
                    ? value.GetAs<StringValue>().Value
                    : value.GetAs<UntypedAtomicValue>().Value.ToString();

                var decimalValue = double.Parse(stringValue ?? string.Empty);
                if (!double.IsNaN(decimalValue) || double.IsFinite(decimalValue))
                    return new SuccessResult<AtomicValue>(AtomicValue.Create(decimalValue, ValueType.XsDecimal));

                return new ErrorResult<AtomicValue>($"FORG0001: Can not cast {stringValue} to xs:decimal");
            };

        return _ => new ErrorResult<AtomicValue>(
            "XPTY0004: Casting not supported from given type to xs:decimal or any of its derived types.");
    }
}