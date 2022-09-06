using FontoXPathCSharp.Sequences;

namespace FontoXPathCSharp.Expressions;

public class SequenceOperator<TNode> : PossiblyUpdatingExpression<TNode>
{
    public SequenceOperator(AbstractExpression<TNode>[] expressions) : base(
        expressions.Aggregate(new Specificity(), (specificity, selector) => specificity.Add(selector.Specificity)),
        expressions,
        new OptimizationOptions(expressions.All(e => e.CanBeStaticallyEvaluated)))
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