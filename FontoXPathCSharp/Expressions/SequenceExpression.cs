using FontoXPathCSharp.Sequences;

namespace FontoXPathCSharp.Expressions;

public class SequenceExpression : AbstractExpression
{
    public SequenceExpression(AbstractExpression[] childExpressions) : base(childExpressions,
        new OptimizationOptions(childExpressions.All(e => e.CanBeStaticallyEvaluated)))
    {
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
    {
        throw new NotImplementedException("SequenceExpression.Evaluate not implemented");
    }
}