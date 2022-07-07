using FontoXPathCSharp.Sequences;

namespace FontoXPathCSharp.Expressions;

public class FilterExpression : AbstractExpression
{
    private readonly AbstractExpression _filterExpression;
    private readonly AbstractExpression _selector;

    public FilterExpression(AbstractExpression selector, AbstractExpression filterExpression) : base(new[] { selector },
        new OptimizationOptions(selector.CanBeStaticallyEvaluated && filterExpression.CanBeStaticallyEvaluated))
    {
        _selector = selector;
        _filterExpression = filterExpression;
        throw new NotImplementedException();
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
    {
        throw new NotImplementedException();
    }
}