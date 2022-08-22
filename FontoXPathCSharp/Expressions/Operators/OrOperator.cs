using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.Operators;

public class OrOperator<TNode> : AbstractExpression<TNode> where TNode : notnull
{
    private readonly string? _bucket;
    private readonly AbstractExpression<TNode>[] _subExpressions;

    public OrOperator(AbstractExpression<TNode>[] expressions) : base(expressions,
        new OptimizationOptions(expressions.All(e => e.CanBeStaticallyEvaluated)))
    {
        // TODO: Adding specificity to expressions
        // var maxSpecificity = expressions.Aggregate(new Specificity(),
        //     (currentMaxSpecificity, selector) => {
        //     if (currentMaxSpecificity.CompareTo(selector.specificity) > 0) {
        //         return currentMaxSpecificity;
        //     }
        //     return selector;
        // });
        _subExpressions = expressions;
        _bucket = expressions.First().GetBucket() != null
                  && expressions.All(e => e.GetBucket() == expressions.First().GetBucket())
            ? expressions.First().GetBucket()
            : null;
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
            if (!done)
            {
                while (i < _subExpressions.Length)
                {
                    if (resultSequence == null)
                    {
                        var subExpression = _subExpressions[i];
                        if (contextItemBuckets != null && subExpression.GetBucket() != null)
                            if (!contextItemBuckets.Contains(subExpression.GetBucket()))
                            {
                                // This subExpression may NEVER match the given node
                                // We do not even have to evaluate the expression
                                i++;
                                continue;
                            }

                        resultSequence = subExpression.EvaluateMaybeStatically(
                            dynamicContext,
                            executionParameters
                        );
                    }

                    var ebv = resultSequence.GetEffectiveBooleanValue();
                    if (ebv)
                    {
                        done = true;
                        return IteratorResult<AbstractValue>.Ready(AtomicValue.TrueBoolean);
                    }

                    resultSequence = null;
                    i++;
                }

                done = true;
                return IteratorResult<AbstractValue>.Ready(AtomicValue.FalseBoolean);
            }

            return IteratorResult<AbstractValue>.Done();
        });
    }

    public override string? GetBucket()
    {
        return _bucket;
    }
}