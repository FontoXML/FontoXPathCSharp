using FontoXPathCSharp.Expressions;

namespace FontoXPathCSharp.EvaluationUtils;

public class EvaluationContext
{
    public DynamicContext Context { get; }
    public ExecutionParameters Parameters { get; }
    public AbstractExpression Expression { get; }
    
    public EvaluationContext(DynamicContext dynamicContext, ExecutionParameters executionParameters, AbstractExpression expression)
    {
        Context = dynamicContext;
        Parameters = executionParameters;
        Expression = expression;
    }
}
