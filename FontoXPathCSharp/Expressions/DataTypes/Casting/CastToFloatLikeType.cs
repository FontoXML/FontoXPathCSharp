using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.DataTypes.Casting;

public delegate Result<double> IntermediateFloatResultFunction(AbstractValue input);

public static class CastToFloatLikeType
{
    public static IntermediateFloatResultFunction ToFloatLikeType(ValueType from, ValueType to)
    {
        if (from.IsSubtypeOf(ValueType.XsNumeric))
            return value => new SuccessResult<double>(Convert.ToDouble(value.GetAs<AtomicValue>().GetValue()));

        if (from.IsSubtypeOf(ValueType.XsBoolean))
            return value => new SuccessResult<double>(Convert.ToBoolean(value.GetAs<AtomicValue>().GetValue()) ? 1 : 0);

        if (from.IsSubtypeOfAny(ValueType.XsString, ValueType.XsUntypedAtomic))
            return value =>
            {
                var stringValue = value.GetValueType().IsSubtypeOf(ValueType.XsString)
                    ? value.GetAs<StringValue>().Value
                    : value.GetAs<UntypedAtomicValue>().Value.ToString();

                switch (stringValue)
                {
                    case "NaN": return new SuccessResult<double>(double.NaN);
                    case "INF" or "+INF": return new SuccessResult<double>(double.PositiveInfinity);
                    case "-INF": return new SuccessResult<double>(double.NegativeInfinity);
                    case "0" or "+0": return new SuccessResult<double>(0.0);
                    case "-0": return new SuccessResult<double>(-0.0);
                }

                var floatValue = Convert.ToDouble(stringValue);

                if (!double.IsNaN(floatValue)) return new SuccessResult<double>(floatValue);

                return new ErrorResult<double>($"Cannot cast {value} to {to.ToString()}",
                    new Error[] { new("FORG0001", $"Cannot cast {value} to {to.ToString()}") });
            };

        return _ => new ErrorResult<double>(
            "",
            new Error[]
            {
                new("XPTY0004", $"Casting not supported from given type to {to} or any of its derived types.")
            });
    }
}