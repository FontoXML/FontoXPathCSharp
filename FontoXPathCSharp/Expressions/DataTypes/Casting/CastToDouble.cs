using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Value;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.DataTypes.Casting;

public static class CastToDouble
{
    public static CastingFunction ToDouble(ValueType from)
    {
        var caster = CastToFloatLikeType.ToFloatLikeType(from, ValueType.XsDouble);
        return value =>
        {
            var castResult = caster(value);
            return castResult switch
            {
                ErrorResult<double> e => new ErrorResult<AtomicValue>(e.Message, e.ErrorCode),
                SuccessResult<double> r => new SuccessResult<AtomicValue>(
                    AtomicValue.Create(r.Data, ValueType.XsDouble)),
                _ => throw new ArgumentOutOfRangeException()
            };
        };
    }
}