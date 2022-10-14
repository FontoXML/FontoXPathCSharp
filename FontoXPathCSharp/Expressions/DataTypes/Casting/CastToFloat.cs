using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Value;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.DataTypes.Casting;

public class CastToFloat
{
    public static CastingFunction ToFloat(ValueType from)
    {
        var caster = CastToFloatLikeType.ToFloatLikeType(from, ValueType.XsFloat);
        return value =>
        {
            var castResult = caster(value);
            return castResult switch
            {
                ErrorResult<double> e => new ErrorResult<AtomicValue>(e.Message),
                SuccessResult<double> r => new SuccessResult<AtomicValue>(
                    AtomicValue.Create(r.Data, ValueType.XsFloat)),
                _ => throw new ArgumentOutOfRangeException()
            };
        };
    }
}