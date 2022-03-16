using FontoXPathCSharp.Sequences;

namespace FontoXPathCSharp.Expressions;

public abstract class AbstractExpression
{
    public abstract ISequence Evaluate(DynamicContext dynamicContext, ExecutionParameters executionParameters);
}