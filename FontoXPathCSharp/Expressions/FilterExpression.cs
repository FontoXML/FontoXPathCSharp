using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions;

public class FilterExpression<TNode> : AbstractExpression<TNode> where TNode : notnull
{
    private readonly AbstractExpression<TNode> _filterExpression;
    private readonly AbstractExpression<TNode> _selector;

    public FilterExpression(AbstractExpression<TNode> selector, AbstractExpression<TNode> filterExpression) : base(
        selector.Specificity.Add(filterExpression.Specificity),
        new[] { selector, filterExpression },
        new OptimizationOptions(
            selector.CanBeStaticallyEvaluated && filterExpression.CanBeStaticallyEvaluated,
            selector.Peer,
            selector.ExpectedResultOrder,
            selector.Subtree)
    )
    {
        _selector = selector;
        _filterExpression = filterExpression;
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode>? executionParameters)
    {
        var valuesToFilter = _selector.EvaluateMaybeStatically(dynamicContext, executionParameters);

        if (_filterExpression.CanBeStaticallyEvaluated)
        {
            var result = _filterExpression.EvaluateMaybeStatically(dynamicContext, executionParameters);

            if (result.IsEmpty())
                return result;

            var resultValue = result.First()!;
            if (!resultValue.GetValueType().IsSubtypeOf(ValueType.XsNumeric))
                return result.GetEffectiveBooleanValue() ? valuesToFilter : SequenceFactory.CreateEmpty();

            if (resultValue.GetValueType() != ValueType.XsInteger)
                return SequenceFactory.CreateEmpty();

            var requestedIndex = resultValue.GetAs<IntegerValue>().Value;

            var iterator = valuesToFilter.GetValue();
            var done = false;

            return SequenceFactory.CreateFromIterator(_ =>
            {
                if (done)
                    return IteratorResult<AbstractValue>.Done();

                for (var value = iterator(IterationHint.None);
                     !value.IsDone;
                     value = iterator(IterationHint.None))
                    if (requestedIndex-- == 1)
                    {
                        done = true;
                        return value;
                    }

                done = true;

                return IteratorResult<AbstractValue>.Done();
            });
        }

        var iteratorToFilter = valuesToFilter.GetValue();
        IteratorResult<AbstractValue>? iteratorItem = null;
        var i = 0;
        ISequence? filterResultSequence = null;

        return SequenceFactory.CreateFromIterator(hint =>
        {
            var isHintApplied = false;

            while (iteratorItem is not { IsDone: true })
            {
                if (iteratorItem == null)
                {
                    iteratorItem = iteratorToFilter(isHintApplied ? IterationHint.None : hint);
                    isHintApplied = true;
                }

                if (iteratorItem.IsDone) return iteratorItem;

                var newContext = dynamicContext?.ScopeWithFocus(i, iteratorItem.Value, new EmptySequence());
                filterResultSequence ??= _filterExpression.EvaluateMaybeStatically(
                    newContext, executionParameters);

                var first = filterResultSequence.First();
                bool shouldReturnCurrentValue;
                if (first == null)
                {
                    shouldReturnCurrentValue = false;
                }
                else if (first.GetValueType().IsSubtypeOf(ValueType.XsNumeric))
                {
                    shouldReturnCurrentValue = first.GetAs<IntegerValue>().Value == i + 1;
                }
                else
                {
                    var ebv = filterResultSequence.GetEffectiveBooleanValue();
                    shouldReturnCurrentValue = ebv;
                }

                filterResultSequence = null;
                var returnableValue = iteratorItem.Value;
                iteratorItem = null;

                i++;
                if (shouldReturnCurrentValue)
                    return IteratorResult<AbstractValue>.Ready(returnableValue!);
            }

            return iteratorItem;
        });
    }

    public override string? GetBucket()
    {
        return _selector.GetBucket();
    }
}