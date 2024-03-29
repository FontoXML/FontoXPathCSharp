using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Expressions.DataTypes;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions;

public class PathExpression<TNode> : AbstractExpression<TNode> where TNode : notnull
{
    private readonly bool _requireSortedResults;
    private readonly AbstractExpression<TNode>[] _stepExpressions;

    public PathExpression(AbstractExpression<TNode>[] stepExpressions, bool requireSortedResults) : base(
        stepExpressions.Aggregate(
            new Specificity(),
            (specificity, selector) => specificity.Add(selector.Specificity)
        ),
        stepExpressions,
        new OptimizationOptions(
            false,
            stepExpressions.All(selector => selector.Peer),
            requireSortedResults ? ResultOrdering.Sorted : ResultOrdering.Unsorted,
            stepExpressions.All(selector => selector.Subtree))
    )
    {
        _stepExpressions = stepExpressions;
        _requireSortedResults = requireSortedResults;
    }

    public override string ToString()
    {
        return $"PathExpr[ {string.Join(", ", _stepExpressions.Select(x => x.ToString()))} ]";
    }

    private static AbstractValue[] SortResults(DomFacade<TNode> domFacade, AbstractValue[] result)
    {
        var resultContainsNodes = false;
        var resultContainsNonNodes = false;

        foreach (var resultValue in result)
            if (resultValue.GetValueType().IsSubtypeOf(ValueType.Node)) resultContainsNodes = true;
            else resultContainsNonNodes = true;

        if (resultContainsNonNodes && resultContainsNodes)
            throw new XPathException(
                "XPTY0018",
                "The path operator should either return nodes or non-nodes. Mixed sequences are not allowed."
            );

        if (resultContainsNodes)
        {
            var nodesList = result.Cast<NodeValue<TNode>>().ToList();
            return DocumentOrderUtils<TNode>
                .SortNodeValues(domFacade, nodesList)
                .Cast<AbstractValue>()
                .ToArray();
        }

        return result;
    }


    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode>? executionParameters)
    {
        var sequenceHasPeerProperty = true;
        var result = _stepExpressions.Reduce(
            (ISequence?)null,
            (intermediateResultNodesSequence, selector, index) =>
            {
                var childContextIterator = intermediateResultNodesSequence == null
                    ? IteratorUtils.SingleValueIterator(dynamicContext!)
                    : dynamicContext?.CreateSequenceIterator(intermediateResultNodesSequence);

                Iterator<ISequence> resultValuesInOrderOfEvaluation = hint =>
                {
                    var childContext = childContextIterator!(hint);

                    if (childContext.IsDone) return IteratorResult<ISequence>.Done();

                    if (
                        childContext.Value?.ContextItem != null &&
                        !childContext.Value.ContextItem.GetValueType().IsSubtypeOf(ValueType.Node)
                    )
                        if (index > 0)
                            // The result comes from the first expression: that's not allowed:
                            // XPTY0019.

                            // If the result comes from the zero-th expression, it is coming
                            // from outside. In that case, the axis step is supposed to error
                            // with XPTY0020
                            throw new XPathException(
                                "XPTY0019",
                                "The result of E1 in a path expression E1/E2 should not evaluate to a sequence of nodes."
                            );

                    return IteratorResult<ISequence>.Ready(
                        selector.EvaluateMaybeStatically(
                            childContext.Value,
                            executionParameters
                        )
                    );
                };

                ISequence? sortedResultSequence = null;

                if (!_requireSortedResults)
                {
                    sortedResultSequence =
                        SortedSequenceUtils<TNode>.ConcatSortedSequences(resultValuesInOrderOfEvaluation);
                }
                else {
                    switch (selector.ExpectedResultOrder)
                    {
                        case ResultOrdering.ReverseSorted:
                            var resultValuesInReverseOrder = resultValuesInOrderOfEvaluation;
                            resultValuesInOrderOfEvaluation = hint =>
                            {
                                var res = resultValuesInReverseOrder(hint);
                                if (res.IsDone) return res;

                                return IteratorResult<ISequence>.Ready(
                                    res.Value!.MapAll(items =>
                                        SequenceFactory.CreateFromArray(items.Reverse().ToArray())
                                    )
                                );
                            };
                            // Can't do fallthrough in C#, so it is copied.
                            if (selector.Subtree && sequenceHasPeerProperty)
                            {
                                sortedResultSequence = SortedSequenceUtils<TNode>.ConcatSortedSequences(
                                    resultValuesInOrderOfEvaluation
                                );
                                break;
                            }

                            // Only locally sorted
                            sortedResultSequence = SortedSequenceUtils<TNode>.MergeSortedSequences(
                                executionParameters!.DomFacade,
                                resultValuesInOrderOfEvaluation
                            );
                            break;
                        case ResultOrdering.Sorted:
                            if (selector.Subtree && sequenceHasPeerProperty)
                            {
                                sortedResultSequence = SortedSequenceUtils<TNode>.ConcatSortedSequences(
                                    resultValuesInOrderOfEvaluation
                                );
                                break;
                            }

                            // Only locally sorted
                            sortedResultSequence = SortedSequenceUtils<TNode>.MergeSortedSequences(
                                executionParameters!.DomFacade,
                                resultValuesInOrderOfEvaluation
                            );
                            break;
                        case ResultOrdering.Unsorted:
                            // The result should be sorted before we can continue
                            var concattedSequence = SortedSequenceUtils<TNode>.ConcatSortedSequences(
                                resultValuesInOrderOfEvaluation
                            );
                            return concattedSequence.MapAll(allValues =>
                                SequenceFactory.CreateFromArray(
                                    SortResults(executionParameters!.DomFacade, allValues)
                                )
                            );
                    }
                }

                sequenceHasPeerProperty = sequenceHasPeerProperty && selector.Peer;
                return sortedResultSequence!;
            }
        )!;

        return result;
    }

    public override string? GetBucket()
    {
        return _stepExpressions.First().GetBucket();
    }
}