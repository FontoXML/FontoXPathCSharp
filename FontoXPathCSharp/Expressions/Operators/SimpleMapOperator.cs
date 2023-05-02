using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions.Operators;

public class SimpleMapOperator<TNode> : AbstractExpression<TNode> where TNode : notnull
{
    private AbstractExpression<TNode> _expression1;
    private AbstractExpression<TNode> _expression2;

    public SimpleMapOperator(AbstractExpression<TNode> expression1, AbstractExpression<TNode> expression2)
        : base(new Specificity().Add(expression1.Specificity),
            new[] { expression1, expression2 },
            new OptimizationOptions(expression1.CanBeStaticallyEvaluated && expression2.CanBeStaticallyEvaluated))
    {
        _expression1 = expression1;
        _expression2 = expression2;
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode>? executionParameters)
    {
        var sequence = _expression1.EvaluateMaybeStatically(dynamicContext, executionParameters);
        var childContextIterator = dynamicContext.CreateSequenceIterator(sequence);
        IteratorResult<DynamicContext>? childContext = null;
        ISequence? sequenceValueIterator = null;
        var done = false;

        return SequenceFactory.CreateFromIterator(hint =>
        {
            while (!done)
            {
                if (childContext == null)
                {
                    childContext = childContextIterator(hint);
                    if (childContext.IsDone)
                    {
                        done = true;
                        return IteratorResult<AbstractValue>.Done();
                    }
                }

                if (sequenceValueIterator == null)
                {
                    sequenceValueIterator =
                        _expression2.EvaluateMaybeStatically(
                            childContext.Value,
                            executionParameters
                        );
                }

                var value = sequenceValueIterator.GetValue()(hint);

                if (value.IsDone)
                {
                    sequenceValueIterator = null;
                    childContext = null;
                    continue;
                }

                return value;
            }

            throw new Exception("This point should never be reached");
        });
    }
}