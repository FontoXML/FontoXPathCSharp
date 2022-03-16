using System.Xml;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions;

public class PathExpression : AbstractExpression
{
    private readonly AbstractExpression[] _stepExpressions;

    public PathExpression(AbstractExpression[] stepExpressions)
    {
        _stepExpressions = stepExpressions;
    }

    public override ISequence Evaluate(DynamicContext dynamicContext, ExecutionParameters executionParameters)
    {
        return _stepExpressions.Aggregate(new ArrayBackedSequence(new[] {dynamicContext.ContextItem}),
            (contextItems, step) =>
            {
                return new ArrayBackedSequence(contextItems
                    .SelectMany(c =>
                    {
                        // NOTE: if dynamicContext is passed as a reference, this will overwrite ut
                        dynamicContext.ContextItem = c;
                        return (IEnumerable<AbstractValue>) step.Evaluate(dynamicContext, executionParameters);
                    }).ToArray());
            });
    }
}