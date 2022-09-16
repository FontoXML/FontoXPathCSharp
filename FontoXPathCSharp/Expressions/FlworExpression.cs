using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions;

public abstract class FlworExpression<TNode> : AbstractExpression<TNode>
{
    protected readonly AbstractExpression<TNode> ReturnExpression;

    protected FlworExpression(
        Specificity specificity,
        AbstractExpression<TNode>[] childExpressions,
        OptimizationOptions optimizationOptions,
        AbstractExpression<TNode> returnExpression) : base(specificity, childExpressions, optimizationOptions)
    {
        ReturnExpression = returnExpression;
        IsUpdating = returnExpression.IsUpdating;
    }

    public abstract ISequence DoFlworExpression(
        DynamicContext outerDynamicContext,
        Iterator<DynamicContext> outerDynamicContextIterator,
        ExecutionParameters<TNode> executionParameters,
        Func<Iterator<DynamicContext>, ISequence> createReturnSequence);

    // TODO: DoFlworExpressionUpdating
    // TODO: EvaluateWithUpdateList

    public override ISequence Evaluate(
        DynamicContext? dynamicContext,
        ExecutionParameters<TNode> executionParameters)
    {
        return DoFlworExpression(
            dynamicContext!,
            IteratorUtils.SingleValueIterator(dynamicContext)!,
            executionParameters,
            dynamicContextIterator =>
            {
                if (ReturnExpression is FlworExpression<TNode> expression)
                    // We are in a FLWOR, the return is also a FLWOR, keep piping dynamiccontext generators
                    return expression.DoFlworExpressionInternal(
                        dynamicContext!,
                        dynamicContextIterator,
                        executionParameters
                    );

                Iterator<AbstractValue>? currentSequenceIterator = null;
                return SequenceFactory.CreateFromIterator<AbstractValue>(
                    hint =>
                    {
                        while (true)
                        {
                            if (currentSequenceIterator == null)
                            {
                                var temp = dynamicContextIterator(IterationHint.None);

                                if (temp.IsDone) return IteratorResult<AbstractValue>.Done();
                                currentSequenceIterator =
                                    ReturnExpression.EvaluateMaybeStatically(
                                        temp.Value,
                                        executionParameters
                                    ).GetValue();
                            }

                            var nextValue = currentSequenceIterator(hint);
                            if (nextValue.IsDone)
                            {
                                currentSequenceIterator = null;
                                continue;
                            }

                            return nextValue;
                        }
                    }
                );
            }
        );
    }

    public override void PerformStaticEvaluation(StaticContext<TNode> staticContext)
    {
        base.PerformStaticEvaluation(staticContext);
        IsUpdating = ReturnExpression.IsUpdating;

        if (ChildExpressions.Where(childExpression => childExpression != ReturnExpression)
            .Any(childExpression => childExpression.IsUpdating))
            throw new XPathException("XUST0001",
                "Can not execute an updating expression in a non-updating context.");
    }

    private ISequence DoFlworExpressionInternal(
        DynamicContext outerDynamicContext,
        Iterator<DynamicContext> outerDynamicContextIterator,
        ExecutionParameters<TNode> executionParameters)
    {
        return DoFlworExpression(
            outerDynamicContext,
            outerDynamicContextIterator,
            executionParameters,
            dynamicContextIterator =>
            {
                if (ReturnExpression is FlworExpression<TNode>)
                    return ((FlworExpression<TNode>)ReturnExpression).DoFlworExpressionInternal(
                        outerDynamicContext,
                        dynamicContextIterator,
                        executionParameters
                    );
                Iterator<AbstractValue>? currentSequenceIterator = null;
                return SequenceFactory.CreateFromIterator(
                    _ =>
                    {
                        while (true)
                        {
                            if (currentSequenceIterator == null)
                            {
                                var temp = dynamicContextIterator(IterationHint.None);

                                if (temp.IsDone) return IteratorResult<AbstractValue>.Done();
                                currentSequenceIterator =
                                    ReturnExpression.EvaluateMaybeStatically(
                                        temp.Value,
                                        executionParameters
                                    ).GetValue();
                            }

                            var nextValue = currentSequenceIterator(IterationHint.None);
                            if (nextValue.IsDone)
                            {
                                currentSequenceIterator = null;
                                continue;
                            }

                            return nextValue;
                        }
                    }
                );
            }
        );
    }
}