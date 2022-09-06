using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Sequences;

namespace FontoXPathCSharp.Expressions.Axes;

public class SelfAxis<TNode> : AbstractExpression<TNode>
{
    private readonly string? _filterBucket;
    private readonly AbstractTestExpression<TNode> _selector;

    public SelfAxis(AbstractTestExpression<TNode> selector, string? filterBucket) : base(
        selector.Specificity,
        new AbstractExpression<TNode>[] { selector },
        new OptimizationOptions(false))
    {
        _selector = selector;
        _filterBucket = BucketUtils.IntersectBuckets(selector.GetBucket(), filterBucket);
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode> executionParameters)
    {
        var isMatch = _selector.EvaluateToBoolean(dynamicContext, dynamicContext?.ContextItem!, executionParameters);
        return isMatch ? SequenceFactory.CreateFromValue(dynamicContext?.ContextItem!) : SequenceFactory.CreateEmpty();
    }

    public override string? GetBucket()
    {
        return _filterBucket;
    }
}