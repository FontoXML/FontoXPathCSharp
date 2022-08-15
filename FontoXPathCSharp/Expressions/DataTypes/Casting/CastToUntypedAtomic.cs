using FontoXPathCSharp.Value;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.DataTypes.Casting;

public class CastToUntypedAtomic
{
    public static CastingFunction ToUntypedAtomic(ValueType from)
    {
        var caster = CastToStringLikeType.ToStringLikeType(from);
        return value =>
        {
            var castResult = caster(value);
            return castResult switch
            {
                ErrorResult<string> e => new ErrorResult<AtomicValue>(e.Message),
                SuccessResult<string> r => new SuccessResult<AtomicValue>(
                    AtomicValue.Create(r.Data, ValueType.XsUntypedAtomic))
            };
        };
    }
}