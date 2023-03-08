using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Expressions.DataTypes;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.Operators;

public class IntersectExcept<TNode> : AbstractExpression<TNode> where TNode : notnull
{
    private readonly AbstractExpression<TNode> _expression1;
    private readonly AbstractExpression<TNode> _expression2;
    private readonly AstNodeName _operation;

    public IntersectExcept(
        AstNodeName operation,
        AbstractExpression<TNode> expression1,
        AbstractExpression<TNode> expression2) : base(
        expression1.Specificity.CompareTo(expression2.Specificity) > 0
            ? expression1.Specificity
            : expression2.Specificity,
        new[] { expression1, expression2 },
        new OptimizationOptions(expression1.CanBeStaticallyEvaluated && expression2.CanBeStaticallyEvaluated))
    {
        _operation = operation;
        _expression1 = expression1;
        _expression2 = expression2;
    }

    private static ISequence EnsureSortedSequence(AstNodeName operation, DomFacade<TNode> domFacade,
        ISequence sequence,
        ResultOrdering expectedResultOrder)
    {
        return sequence.MapAll(values =>
        {
            if (values.Any(value => !value.GetValueType().IsSubtypeOf(ValueType.Node)))
                throw new XPathException("XPTY0004", $"Sequences given to {operation} should only contain nodes.");

            return expectedResultOrder switch
            {
                ResultOrdering.Sorted => SequenceFactory.CreateFromArray(values),
                ResultOrdering.ReverseSorted => SequenceFactory.CreateFromArray(values.Reverse().ToArray()),
                _ => SequenceFactory.CreateFromArray(DocumentOrderUtils<TNode>
                    .SortNodeValues(domFacade, values.Cast<NodeValue<TNode>>().ToList()).Cast<AbstractValue>()
                    .ToArray())
            };
        });
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode>? executionParameters)
    {
        var domFacade = executionParameters!.DomFacade;
        var firstResult = EnsureSortedSequence(
            _operation,
            domFacade,
            _expression1.EvaluateMaybeStatically(dynamicContext, executionParameters),
            _expression1.ExpectedResultOrder
        );
        var secondResult = EnsureSortedSequence(
            _operation,
            domFacade,
            _expression2.EvaluateMaybeStatically(dynamicContext, executionParameters),
            _expression2.ExpectedResultOrder
        );

        var firstIterator = firstResult.GetValue();
        var secondIterator = secondResult.GetValue();

        AbstractValue? firstValue = null;
        AbstractValue? secondValue = null;

        var done = false;
        var secondIteratorDone = false;
        return SequenceFactory.CreateFromIterator(
            _ =>
            {
                if (done) return IteratorResult<AbstractValue>.Done();
                while (!secondIteratorDone)
                {
                    if (firstValue == null)
                    {
                        var itrResult = firstIterator(IterationHint.None);
                        if (itrResult.IsDone)
                        {
                            // Since ∅ \ X = ∅ and ∅ ∩ X = ∅, we are done.
                            done = true;
                            return IteratorResult<AbstractValue>.Done();
                        }

                        firstValue = itrResult.Value;
                    }

                    if (secondValue == null)
                    {
                        var itrResult = secondIterator(IterationHint.None);
                        if (itrResult.IsDone)
                        {
                            secondIteratorDone = true;
                            break;
                        }

                        secondValue = itrResult.Value;
                    }

                    if (SortedSequenceUtils<TNode>.IsSameNodeValue(firstValue, secondValue))
                    {
                        var toReturn = IteratorResult<AbstractValue>.Ready(firstValue!);
                        firstValue = null;
                        secondValue = null;
                        if (_operation == AstNodeName.IntersectOp) return toReturn;
                        continue;
                    }

                    var comparisonResult = DocumentOrderUtils<TNode>.CompareNodePositions(
                        domFacade,
                        firstValue!.GetAs<NodeValue<TNode>>(),
                        secondValue!.GetAs<NodeValue<TNode>>()
                    );
                    if (comparisonResult < 0)
                    {
                        var toReturn = IteratorResult<AbstractValue>.Ready(firstValue);
                        firstValue = null;
                        if (_operation == AstNodeName.ExceptOp) return toReturn;
                    }
                    else
                    {
                        secondValue = null;
                    }
                }

                // The second array is empty.
                if (_operation == AstNodeName.ExceptOp)
                {
                    // Since X \ ∅ = X, we can output all items of X
                    if (firstValue != null)
                    {
                        var toReturn = IteratorResult<AbstractValue>.Ready(firstValue);
                        firstValue = null;
                        return toReturn;
                    }

                    return firstIterator(IterationHint.None);
                }

                // Since X ∩ ∅ = ∅, we are done.
                done = true;
                return IteratorResult<AbstractValue>.Done();
            }
        );
    }
}