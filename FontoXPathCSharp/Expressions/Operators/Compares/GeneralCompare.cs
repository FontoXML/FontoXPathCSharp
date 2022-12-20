using FontoXPathCSharp.EvaluationUtils;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.Operators.Compares;

public class GeneralCompare<TNode> : AbstractExpression<TNode> where TNode : notnull
{
    private readonly AbstractExpression<TNode> _firstExpression;
    private readonly AbstractExpression<TNode> _secondExpression;
    private readonly CompareType _type;

    public GeneralCompare(CompareType type, AbstractExpression<TNode> firstExpression,
        AbstractExpression<TNode> secondExpression) : base(
        firstExpression.Specificity,
        new[] { firstExpression, secondExpression },
        new OptimizationOptions(false))
    {
        _type = type;
        _firstExpression = firstExpression;
        _secondExpression = secondExpression;
    }

    private static (ValueType?, ValueType?) DetermineTargetType(ValueType firstType, ValueType secondType)
    {
        ValueType? firstTargetType = null;
        ValueType? secondTargetType = null;

        if (!firstType.IsSubtypeOf(ValueType.XsUntypedAtomic) && !secondType.IsSubtypeOf(ValueType.XsUntypedAtomic))
            return (firstTargetType, secondTargetType);

        if (firstType.IsSubtypeOf(ValueType.XsNumeric))
            secondTargetType = ValueType.XsDouble;
        else if (secondType.IsSubtypeOf(ValueType.XsNumeric))
            firstTargetType = ValueType.XsDouble;
        else if (firstType.IsSubtypeOf(ValueType.XsDayTimeDuration))
            secondTargetType = ValueType.XsDayTimeDuration;
        else if (secondType.IsSubtypeOf(ValueType.XsDayTimeDuration))
            firstTargetType = ValueType.XsDayTimeDuration;
        else if (firstType.IsSubtypeOf(ValueType.XsYearMonthDuration))
            secondTargetType = ValueType.XsYearMonthDuration;
        else if (secondType.IsSubtypeOf(ValueType.XsYearMonthDuration))
            firstTargetType = ValueType.XsYearMonthDuration;
        else if (firstType.IsSubtypeOf(ValueType.XsUntypedAtomic))
            firstTargetType = secondType;
        else if (secondType.IsSubtypeOf(ValueType.XsUntypedAtomic))
            secondTargetType = firstType;

        return (firstTargetType, secondTargetType);
    }

    private static ISequence PerformGeneralCompare(
        CompareType type, 
        ISequence firstSequence,
        ISequence secondSequence,
        DynamicContext dynamicContext)
    {
        return secondSequence.MapAll(allSecondValues =>
        {
            var result = firstSequence.Filter((firstValue, _, _) =>
            {
                foreach (var secondVar in allSecondValues)
                {
                    var secondValue = secondVar;
                    var (firstTargetType, secondTargetType) =
                        DetermineTargetType(firstValue.GetValueType(), secondValue.GetValueType());

                    if (firstTargetType.HasValue)
                        firstValue = firstValue.CastToType(firstTargetType.Value);
                    else if (secondTargetType.HasValue)
                        secondValue = secondValue.CastToType(secondTargetType.Value);

                    if (ValueCompare<TNode>.PerformValueCompare(type, firstValue, secondValue, dynamicContext))
                        return true;
                }

                return false;
            });

            return SequenceFactory.CreateFromValue(new BooleanValue(!result.IsEmpty()));
        });
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode>? executionParameters)
    {
        var firstSequence = _firstExpression.EvaluateMaybeStatically(dynamicContext, executionParameters);
        var secondSequence = _secondExpression.EvaluateMaybeStatically(dynamicContext, executionParameters);

        if (firstSequence.IsEmpty() || secondSequence.IsEmpty())
            return SequenceFactory.CreateFromValue(new BooleanValue(false));

        var firstAtomizedSequence = Atomize.AtomizeSequence(firstSequence, executionParameters!);
        var secondAtomizedSequence = Atomize.AtomizeSequence(secondSequence, executionParameters!);

        return PerformGeneralCompare(_type, firstAtomizedSequence, secondAtomizedSequence, dynamicContext!);
    }
}