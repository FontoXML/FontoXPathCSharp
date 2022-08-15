using FontoXPathCSharp.Sequences;

namespace FontoXPathCSharp.Expressions;

public class SequenceExpression<TNode> : PossiblyUpdatingExpression<TNode>
{
    public SequenceExpression(AbstractExpression<TNode>[] childExpressions) : base(childExpressions,
        new OptimizationOptions(childExpressions.All(e => e.CanBeStaticallyEvaluated)))
    {
    }

    public override ISequence PerformFunctionalEvaluation(
        DynamicContext? dynamicContext,
        ExecutionParameters<TNode> executionParameters,
        SequenceCallback[] sequenceCallbacks)
    {
        return sequenceCallbacks.Length == 0
            ? SequenceFactory.CreateEmpty()
            : ISequence.ConcatSequences(sequenceCallbacks.Select(cb => cb(dynamicContext!)).ToArray());
    }
}