using FontoXPathCSharp.EvaluationUtils;
using FontoXPathCSharp.Sequences;

namespace FontoXPathCSharp.Expressions;

public class SequenceExpression : PossiblyUpdatingExpression
{
    public SequenceExpression(AbstractExpression[] childExpressions) : base(childExpressions,
        new OptimizationOptions(childExpressions.All(e => e.CanBeStaticallyEvaluated)))
    {
    }

    public override ISequence PerformFunctionalEvaluation(
        DynamicContext? dynamicContext,
        ExecutionParameters? executionParameters,
        SequenceCallback[] sequenceCallbacks)
    {
        return sequenceCallbacks.Length == 0
            ? SequenceFactory.CreateEmpty()
            : SequenceUtils.ConcatSequences(sequenceCallbacks.Select((cb) => cb(dynamicContext!)).ToArray());
    }
}