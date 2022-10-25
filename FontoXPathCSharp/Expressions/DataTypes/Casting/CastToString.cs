using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Value;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.DataTypes.Casting;

public static class CastToString
{
    public static CastingFunction ToString(ValueType from)
    {
        var caster = CastToStringLikeType.ToStringLikeType(from);
        return value =>
        {
            var castResult = caster(value);
            return castResult switch
            {
                ErrorResult<string> e => new ErrorResult<AtomicValue>(e.Message, e.Errors),
                SuccessResult<string> r => new SuccessResult<AtomicValue>(
                    AtomicValue.Create(r.Data, ValueType.XsString)),
                _ => throw new ArgumentOutOfRangeException()
            };
        };
    }
}