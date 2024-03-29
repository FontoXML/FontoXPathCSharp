using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.DataTypes.Casting;

public static class CastToBoolean
{
    public static CastingFunction ToBoolean(ValueType from)
    {
        if (from.IsSubtypeOf(ValueType.XsNumeric))
            return value =>
                new SuccessResult<AtomicValue>(
                    Convert.ToBoolean(value.GetValue())
                        ? AtomicValue.TrueBoolean
                        : AtomicValue.FalseBoolean);

        if (from.IsSubtypeOfAny(ValueType.XsString, ValueType.XsUntypedAtomic))
            return value =>
            {
                return value.GetValue().ToString() switch
                {
                    "true" or "1" => new SuccessResult<AtomicValue>(AtomicValue.TrueBoolean),
                    "false" or "0" => new SuccessResult<AtomicValue>(AtomicValue.FalseBoolean),
                    _ => new ErrorResult<AtomicValue>(
                        "Casting not supported from given type to xs:boolean or any of its derived types.", "XPTY0004")
                };
            };

        return _ => new ErrorResult<AtomicValue>(
            "Casting not supported from given type to xs:boolean or any of its derived types.", "XPTY0004");
    }
}