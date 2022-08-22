using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.Operators;

public class AndOperator<TNode> : AbstractExpression<TNode> where TNode : notnull
{
    private readonly string? _bucket;
    private readonly AbstractExpression<TNode>[] _subExpressions;

    public AndOperator(AbstractExpression<TNode>[] childExpressions) : base(
        childExpressions,
        new OptimizationOptions(childExpressions.All(e => e.CanBeStaticallyEvaluated)))
    {
        _subExpressions = childExpressions;
        _bucket = childExpressions.Aggregate(
            (string?)null,
            (bucket, expression) => BucketUtils.IntersectBuckets(bucket, expression.GetBucket())
        );
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode> executionParameters)
    {
        var i = 0;
        ISequence? resultSequence = null;
        var done = false;
        string[]? contextItemBuckets = null;

        if (dynamicContext != null)
        {
            var contextItem = dynamicContext.ContextItem;
            if (contextItem != null && contextItem.GetValueType().IsSubtypeOf(ValueType.Node))
                contextItemBuckets = BucketUtils.GetBucketsForNode(
                    contextItem.GetAs<NodeValue<TNode>>().Value,
                    executionParameters.DomFacade
                );
        }

        return SequenceFactory.CreateFromIterator(_ =>
        {
            if (done) return IteratorResult<AbstractValue>.Done();
            while (i < _subExpressions.Length)
            {
                if (resultSequence == null)
                {
                    var subExpression = _subExpressions[i];

                    if (contextItemBuckets != null && subExpression.GetBucket() != null)
                        if (!contextItemBuckets.Contains(subExpression.GetBucket()))
                        {
                            i++;
                            done = true;
                            return IteratorResult<AbstractValue>.Ready(AtomicValue.FalseBoolean);
                        }

                    resultSequence = subExpression.EvaluateMaybeStatically(dynamicContext, executionParameters);
                }

                var ebv = resultSequence.GetEffectiveBooleanValue();
                if (ebv == false)
                {
                    done = true;
                    return IteratorResult<AbstractValue>.Ready(AtomicValue.FalseBoolean);
                }

                resultSequence = null;
                i++;
            }

            done = true;
            return IteratorResult<AbstractValue>.Ready(AtomicValue.TrueBoolean);
        });
    }

    public override string? GetBucket()
    {
        return _bucket;
    }
}