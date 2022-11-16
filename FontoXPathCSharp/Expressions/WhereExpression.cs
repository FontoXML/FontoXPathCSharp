using FontoXPathCSharp.Sequences;

namespace FontoXPathCSharp.Expressions;

public class WhereExpression<TNode> : FlworExpression<TNode> where TNode : notnull
{
    private readonly AbstractExpression<TNode> _testExpression;

    public WhereExpression(AbstractExpression<TNode> testExpression, AbstractExpression<TNode> returnExpression) : base(
        new Specificity(),
        new[] { testExpression, returnExpression },
        new OptimizationOptions(false),
        returnExpression)
    {
        _testExpression = testExpression;
    }

    public override ISequence DoFlworExpression(
        DynamicContext outerDynamicContext,
        Iterator<DynamicContext> dynamicContextIterator,
        ExecutionParameters<TNode> executionParameters,
        Func<Iterator<DynamicContext>, ISequence> createReturnSequence)
    {
        DynamicContext? currentDynamicContext = null;
        ISequence? testExpressionResult = null;
        return createReturnSequence(_ =>
            {
                while (true)
                {
                    if (testExpressionResult == null)
                    {
                        var currentDynamicContextValue = dynamicContextIterator(IterationHint.None);

                        if (currentDynamicContextValue.IsDone) return IteratorResult<DynamicContext>.Done();

                        currentDynamicContext = currentDynamicContextValue.Value;
                        testExpressionResult = _testExpression.EvaluateMaybeStatically(
                            currentDynamicContext,
                            executionParameters
                        );
                    }

                    var effectiveBooleanValue = testExpressionResult.GetEffectiveBooleanValue();

                    // Prepare for next iteration
                    var dynamicContextToReturn = currentDynamicContext;
                    currentDynamicContext = null;
                    testExpressionResult = null;

                    if (!effectiveBooleanValue) continue;
                    return IteratorResult<DynamicContext>.Ready(dynamicContextToReturn!);
                }
            }
        );
    }
}