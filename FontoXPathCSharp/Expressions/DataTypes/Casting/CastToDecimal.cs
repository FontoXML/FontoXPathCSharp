using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.DataTypes.Casting;

public static class CastToDecimal
{
    public static CastingFunction ToDecimal(ValueType from)
    {
        if (from.IsSubtypeOf(ValueType.XsInteger))
            return value =>
                new SuccessResult<AtomicValue>(AtomicValue.Create(value.GetAs<IntegerValue>().Value,
                    ValueType.XsDecimal));

        if (from.IsSubtypeOf(ValueType.XsFloat))
            return value =>
            {
                var floatValue = value.GetAs<FloatValue>().Value;
                if (float.IsNaN(floatValue) || !float.IsFinite(floatValue))
                    return new ErrorResult<AtomicValue>($"Can not cast '{value}' to xs:decimal", "FOCA0002");

                if (Math.Abs(floatValue) > float.MaxValue)
                    return new ErrorResult<AtomicValue>(
                        $"Can not cast '{value}'to xs:decimal, it is out of bounds for C# numbers", "FOCA0002");

                return new SuccessResult<AtomicValue>(AtomicValue.Create(floatValue, ValueType.XsDecimal));
            };

        if (from.IsSubtypeOf(ValueType.XsDouble))
            return value =>
            {
                var doubleValue = value.GetAs<DoubleValue>().Value;
                if (double.IsNaN(doubleValue) || !double.IsFinite(doubleValue))
                    return new ErrorResult<AtomicValue>($"Can not cast '{value}' to xs:decimal", "FOCA0002");

                if (Math.Abs(doubleValue) > double.MaxValue)
                    return new ErrorResult<AtomicValue>(
                        $"Can not cast '{value}' to xs:decimal, it is out of bounds for C# numbers", "FOCA0002");

                return new SuccessResult<AtomicValue>(AtomicValue.Create(doubleValue, ValueType.XsDecimal));
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


                var decimalValue = Convert.ToDecimal(stringValue);
                return new SuccessResult<AtomicValue>(AtomicValue.Create(decimalValue, ValueType.XsDecimal));
                // return new ErrorResult<AtomicValue>($"Can not cast {stringValue} to xs:decimal", "FORG0001");
            };


        return _ => new ErrorResult<AtomicValue>(
            $"Casting not supported from {from} to xs:decimal or any of its derived types.", "XPTY0004");
    }
}