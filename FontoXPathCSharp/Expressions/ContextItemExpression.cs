using FontoXPathCSharp.Sequences;

namespace FontoXPathCSharp.Expressions;

public class ContextItemExpression : AbstractExpression
{
    public ContextItemExpression() : base(
        Array.Empty<AbstractExpression>(), new OptimizationOptions(false))
    {
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
    {
        if (dynamicContext?.ContextItem == null)
            throw new XPathException("XPDY0002: context is absent, it needs to be present to use the \".\" operator");

        return new SingletonSequence(dynamicContext.ContextItem);
    }
}