using FontoXPathCSharp.Sequences;

namespace FontoXPathCSharp.Expressions;

public class PathExpression : AbstractExpression
{
    private readonly AbstractExpression[] _stepExpressions;

    public PathExpression(AbstractExpression[] stepExpressions) : base(stepExpressions, new OptimizationOptions(false))
    {
        _stepExpressions = stepExpressions;
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
    {
        return _stepExpressions.Aggregate(SequenceFactory.CreateFromArray(new[] { dynamicContext!.ContextItem! }),
            (contextItems, step) =>
            {
                return SequenceFactory.CreateFromArray(contextItems
                    .SelectMany(c =>
                    {
                        // NOTE: if dynamicContext is passed as a reference, this will overwrite ut
                        dynamicContext.ContextItem = c;
                        return step.Evaluate(dynamicContext, executionParameters);
                    }).ToArray());
            });
    }
}