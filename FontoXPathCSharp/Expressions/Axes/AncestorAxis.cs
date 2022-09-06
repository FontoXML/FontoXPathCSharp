using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions.Axes;

public class AncestorAxis<TNode> : AbstractExpression<TNode>
{
    private readonly AbstractTestExpression<TNode> _ancestorExpression;
    private readonly bool _inclusive;


    public AncestorAxis(AbstractTestExpression<TNode> ancestorExpression, bool inclusive) : base(
        ancestorExpression.Specificity,
        new AbstractExpression<TNode>[] { ancestorExpression },
        new OptimizationOptions(false)
    )
    {
        _ancestorExpression = ancestorExpression;
        _inclusive = inclusive;
    }

    private static Iterator<AbstractValue> GenerateAncestors(IDomFacade<TNode>? domFacade, TNode? contextPointer)
    {
        var ancestor = contextPointer;
        return _ =>
        {
            if (ancestor == null) return IteratorResult<AbstractValue>.Done();

            var previousAncestor = ancestor;
            ancestor = domFacade.GetParentNode(previousAncestor);

            return IteratorResult<AbstractValue>.Ready(new NodeValue<TNode>(previousAncestor, domFacade));
        };
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode> executionParameters)
    {
        var domFacade = executionParameters.DomFacade;
        var contextItem = ContextNodeUtils<TNode>.ValidateContextNode(dynamicContext!.ContextItem!);

        var ancestorExpressionBucket = _ancestorExpression.GetBucket();
        var onlyElementAncestors = ancestorExpressionBucket != null &&
                                   (ancestorExpressionBucket.StartsWith(BucketConstants.NamePrefix) ||
                                    ancestorExpressionBucket == BucketConstants.Type1);
        var ancestorAxisBucket = onlyElementAncestors ? BucketConstants.Type1 : null;

        return SequenceFactory
            .CreateFromIterator(
                GenerateAncestors(
                    domFacade,
                    _inclusive
                        ? contextItem.Value
                        : domFacade.GetParentNode(contextItem.Value, ancestorAxisBucket)
                )
            )
            .Filter(
                (item, _, _) =>
                    _ancestorExpression.EvaluateToBoolean(
                        dynamicContext,
                        item,
                        executionParameters
                    )
            );
    }
}