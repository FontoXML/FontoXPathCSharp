using FontoXPathCSharp.EvaluationUtils;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.DataTypes.Casting;

public class CastToUntypedAtomic
{
    public static CastingFunction ToUntypedAtomic(InstanceOfFunction instanceOf)
    {
        var caster = CastToStringLikeType.ToStringLikeType(instanceOf);
        return value =>
        {
            var castResult = caster(value);
            return castResult switch
            {
                ErrorResult<string> e => new ErrorResult<AtomicValue>(e.Message),
                SuccessResult<string> r => new SuccessResult<AtomicValue>(
                    Atomize.CreateAtomicValue(r.Data, ValueType.XsUntypedAtomic))
            };
        };
    }
}