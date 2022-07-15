using FontoXPathCSharp.EvaluationUtils;
using FontoXPathCSharp.Value;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.DataTypes.Casting;

public class CastToString
{
    public static CastingFunction ToString(InstanceOfFunction instanceOf)
    {
        var caster = CastToStringLikeType.ToStringLikeType(instanceOf);
        return value =>
        {
            var castResult = caster(value);
            return castResult switch
            {
                ErrorResult<string> e => new ErrorResult<AtomicValue>(e.Message),
                SuccessResult<string> r => new SuccessResult<AtomicValue>(
                    Atomize.CreateAtomicValue(r.Data, ValueType.XsString))
            };
        };
    }
}