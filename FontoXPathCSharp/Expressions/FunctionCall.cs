using FontoXPathCSharp.Sequences;

namespace FontoXPathCSharp.Expressions;

public class FunctionCall : AbstractExpression
{
    private readonly AbstractExpression _functionReferenceExpression;

    public FunctionCall(AbstractExpression functionReferenceExpression)
    {
        _functionReferenceExpression = functionReferenceExpression;
    }

    public override ISequence Evaluate(DynamicContext dynamicContext, ExecutionParameters executionParameters)
    {
        _functionReferenceExpression.Evaluate(dynamicContext, executionParameters);
        throw new NotImplementedException();
    }
}