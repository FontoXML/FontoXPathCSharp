using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions;

public class FilterExpression : AbstractExpression
{
    private readonly AbstractExpression _filterExpression;
    private readonly AbstractExpression _selector;

    public FilterExpression(AbstractExpression selector, AbstractExpression filterExpression) : base(new[] { selector },
        new OptimizationOptions(selector.CanBeStaticallyEvaluated && filterExpression.CanBeStaticallyEvaluated))
    {
        _selector = selector;
        _filterExpression = filterExpression;
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
    {
        var valuesToFilter = _selector.EvaluateMaybeStatically(
            dynamicContext,
            executionParameters
        );

        if (_filterExpression.CanBeStaticallyEvaluated)
        {
            var result = _filterExpression.EvaluateMaybeStatically(dynamicContext, executionParameters);
            if (result.IsEmpty()) return result;

            var resultValue = result.First();
            if (resultValue != null && resultValue.GetValueType().IsSubtypeOf(ValueType.XsInt))
            {
                var requestedIndex = resultValue.GetAs<IntValue>(ValueType.XsInt)!.Value;
                var iterator = valuesToFilter.GetValue();

                var done = false;

                return SequenceFactory.CreateFromIterator(_ =>
                {
                    if (!done)
                        for (var value = iterator(IterationHint.None);
                             !value.IsDone;
                             value = iterator(IterationHint.None))
                            if (requestedIndex-- == 1)
                            {
                                done = true;
                                return value;
                            }

                    return IteratorResult<AbstractValue>.Done();
                });
            }

            if (result.GetEffectiveBooleanValue()) return valuesToFilter;

            return SequenceFactory.CreateEmpty();
        }

        var iteratorToFilter = valuesToFilter.GetValue();
        IteratorResult<AbstractValue>? iteratorItem = null;
        var i = 0;
        ISequence? filterResultSequence = null;

        return SequenceFactory.CreateFromIterator(hint =>
        {
            var isHintApplied = false;
            while (iteratorItem == null || !iteratorItem.IsDone)
            {
                if (iteratorItem == null)
                {
                    iteratorItem = iteratorToFilter(isHintApplied ? IterationHint.None : hint);
                    isHintApplied = true;
                }

                if (iteratorItem.IsDone) return iteratorItem;

                if (filterResultSequence == null)
                    filterResultSequence = _filterExpression.EvaluateMaybeStatically(
                        dynamicContext?.ScopeWithFocus(i, iteratorItem.Value, valuesToFilter), executionParameters);

                var first = filterResultSequence.First();

                bool shouldReturnCurrentValue;
                if (first == null)
                    shouldReturnCurrentValue = false;
                else if (first.GetValueType().IsSubtypeOf(ValueType.XsInt))
                    shouldReturnCurrentValue = first.GetAs<IntValue>(ValueType.XsInt).Value == i + 1;
                else
                    shouldReturnCurrentValue = filterResultSequence.GetEffectiveBooleanValue();

                filterResultSequence = null;
                var returnableValue = iteratorItem.Value;
                iteratorItem = null;

                i++;
                if (shouldReturnCurrentValue) return IteratorResult<AbstractValue>.Ready(returnableValue!);
            }

            return iteratorItem;
        });
    }
}