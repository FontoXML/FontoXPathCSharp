using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions.Operators;

public class OrOperator : AbstractExpression
{
    private readonly AbstractExpression[] _subExpressions;

    public OrOperator(AbstractExpression[] expressions) : base(expressions,
        new OptimizationOptions(expressions.All(e => e.CanBeStaticallyEvaluated)))
    {
        // TODO: Adding specificity to expressions
        // var maxSpecificity = expressions.Aggregate(new Specificity(),
        //     (currentMaxSpecificity, selector) => {
        //     if (currentMaxSpecificity.CompareTo(selector.specificity) > 0) {
        //         return currentMaxSpecificity;
        //     }
        //     return selector;
        // });
        _subExpressions = expressions;
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
    {
        var i = 0;
        ISequence? resultSequence = null;
        var done = false;
        // TODO: Implement bucket stuff
        // string[] contextItemBuckets = null;
        // if (dynamicContext != null) {
        //     const contextItem = dynamicContext.contextItem;
        //     if (contextItem !== null && isSubtypeOf(contextItem.type, ValueType.NODE)) {
        //         contextItemBuckets = getBucketsForPointer(
        //             contextItem.value,
        //             executionParameters.domFacade
        //         );
        //     }
        // }

        return SequenceFactory.CreateFromIterator(_ =>
        {
            if (!done)
            {
                while (i < _subExpressions.Length)
                {
                    if (resultSequence == null)
                    {
                        var subExpression = _subExpressions[i];
                        // TODO: Implement bucket stuff
                        // if (contextItemBuckets != null && subExpression.getBucket() != = null)
                        // {
                        //     if (!contextItemBuckets.includes(subExpression.getBucket()))
                        //     {
                        //         // This subExpression may NEVER match the given node
                        //         // We do not even have to evaluate the expression
                        //         i++;
                        //         continue;
                        //     }
                        // }

                        resultSequence = subExpression.EvaluateMaybeStatically(
                            dynamicContext,
                            executionParameters
                        );
                    }

                    var ebv = resultSequence.GetEffectiveBooleanValue();
                    if (ebv)
                    {
                        done = true;
                        return IteratorResult<AbstractValue>.Ready(AtomicValue.TrueBoolean);
                    }

                    resultSequence = null;
                    i++;
                }

                done = true;
                return IteratorResult<AbstractValue>.Ready(AtomicValue.FalseBoolean);
            }

            return IteratorResult<AbstractValue>.Done();
        });
    }
}