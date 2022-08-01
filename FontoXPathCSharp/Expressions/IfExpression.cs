using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions;

public class IfExpression<TNode> : PossiblyUpdatingExpression<TNode>
{
    private AbstractExpression<TNode> _ifExpr;

    public IfExpression(AbstractExpression<TNode> ifExpr, AbstractExpression<TNode> thenExpr,
        AbstractExpression<TNode> elseExpr) : base(
        new[] { ifExpr, thenExpr, elseExpr },
        new OptimizationOptions(
            ifExpr.CanBeStaticallyEvaluated && thenExpr.CanBeStaticallyEvaluated && elseExpr.CanBeStaticallyEvaluated,
            thenExpr.ResultOrder == elseExpr.ResultOrder ? thenExpr.ResultOrder : ResultOrdering.Unsorted)
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