using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.DataTypes.Casting;

public delegate Result<double> IntermediateFloatResultFunction(AbstractValue input);

public class CastToFloatLikeType
{
    public static IntermediateFloatResultFunction ToFloatLikeType(InstanceOfFunction instanceOf, ValueType to)
    {
        if (instanceOf(ValueType.XsNumeric))
            return value => new SuccessResult<double>((double)value.GetAs<DoubleValue>().Value);

        if (instanceOf(ValueType.XsBoolean))
            return value => new SuccessResult<double>((double)value.GetAs<DoubleValue>().Value != 0.0 ? 1 : 0);

        if (instanceOf(ValueType.XsString) || instanceOf(ValueType.XsUntypedAtomic))
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

                var floatValue = double.Parse(stringValue ?? string.Empty);

                if (!double.IsNaN(floatValue)) return new SuccessResult<double>(floatValue);

                return new ErrorResult<double>($"FORG0001: Cannot cast {value} to {to.ToString()}");
            };

        return _ => new ErrorResult<double>(
            $"XPTY0004: Casting not supported from given type to {to} or any of its derived types.");
    }
}