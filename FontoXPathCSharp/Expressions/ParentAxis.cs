using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions;

public class ParentAxis : AbstractExpression
{
    private readonly AbstractTestExpression _parentExpression;

    public ParentAxis(AbstractTestExpression parentExpression) : base(new AbstractExpression[] {parentExpression},
        new OptimizationOptions(false))
    {
        _parentExpression = parentExpression;
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
    {
        var parentNode = executionParameters?.DomFacade.ParentNode;
        if (parentNode == null) return new EmptySequence();

        // TODO: we technically need a pointer to parentNode here
        var isMatch =
            _parentExpression.EvaluateToBoolean(dynamicContext, new NodeValue(parentNode), executionParameters);
        return isMatch ? new SingletonSequence(new NodeValue(parentNode)) : new EmptySequence();
    }
}