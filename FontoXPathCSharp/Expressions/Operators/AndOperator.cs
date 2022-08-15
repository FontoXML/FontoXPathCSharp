using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions.Operators;

public class AndOperator<TNode> : AbstractExpression<TNode>
{
    private readonly AbstractExpression<TNode>[] _subExpressions;


    public AndOperator(AbstractExpression<TNode>[] childExpressions) : base(
        childExpressions,
        new OptimizationOptions(childExpressions.All(e => e.CanBeStaticallyEvaluated)))
    {
        _subExpressions = childExpressions;
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode> executionParameters)
    {
        var i = 0;
        ISequence? resultSequence = null;
        var done = false;

        return SequenceFactory.CreateFromIterator(_ =>
        {
            if (done) return IteratorResult<AbstractValue>.Done();
            while (i < _subExpressions.Length)
            {
                if (resultSequence == null)
                {
                    var subExpression = _subExpressions[i];

                    // TODO: Context Item Buckets

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
}