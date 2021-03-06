using FontoXPathCSharp.EvaluationUtils;
using FontoXPathCSharp.Value;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.DataTypes.Casting;

public class CastToDouble
{
    public static CastingFunction ToDouble(InstanceOfFunction instanceOf)
    {
        var caster = CastToFloatLikeType.ToFloatLikeType(instanceOf, ValueType.XsDouble);
        return value =>
        {
            var castResult = caster(value);
            return castResult switch
            {
                ErrorResult<double> e => new ErrorResult<AtomicValue>(e.Message),
                SuccessResult<double> r => new SuccessResult<AtomicValue>(
                    Atomize.CreateAtomicValue(r.Data, ValueType.XsDouble))
            };
        };
    }
}