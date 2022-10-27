using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions;

public class IfExpression<TNode> : PossiblyUpdatingExpression<TNode> where TNode : notnull
{
    private AbstractExpression<TNode> _ifExpr;

    public IfExpression(AbstractExpression<TNode> ifExpr, AbstractExpression<TNode> thenExpr,
        AbstractExpression<TNode> elseExpr) : base(
        ifExpr.Specificity.Add(thenExpr.Specificity).Add(elseExpr.Specificity),
        new[] { ifExpr, thenExpr, elseExpr },
        new OptimizationOptions(
            ifExpr.CanBeStaticallyEvaluated && thenExpr.CanBeStaticallyEvaluated && elseExpr.CanBeStaticallyEvaluated,
            thenExpr.Peer == elseExpr.Peer && thenExpr.Peer,
            thenExpr.ExpectedResultOrder == elseExpr.ExpectedResultOrder ? thenExpr.ExpectedResultOrder : ResultOrdering.Unsorted,
            thenExpr.Subtree == elseExpr.Subtree && thenExpr.Subtree)
    )
    {
        _ifExpr = ifExpr;
    }

    public override ISequence PerformFunctionalEvaluation(
        DynamicContext? dynamicContext,
        ExecutionParameters<TNode> executionParameters,
        SequenceCallback[] sequenceCallbacks)
    {
        Iterator<AbstractValue>? resultIterator = null;

        var ifExpressionResultSequence = sequenceCallbacks[0](dynamicContext!);

        return SequenceFactory.CreateFromIterator(
            hint =>
            {
                if (resultIterator == null)
                {
                    var resultSequence = ifExpressionResultSequence.GetEffectiveBooleanValue()
                        ? sequenceCallbacks[1](dynamicContext!)
                        : sequenceCallbacks[2](dynamicContext!);
                    resultIterator = resultSequence.GetValue();
                }

                return resultIterator(hint);
            }
        );
    }
}