using FontoXPathCSharp.Expressions.DataTypes;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.Operators;

public class UnionOperator<TNode> : AbstractExpression<TNode> where TNode : notnull
{
    private readonly AbstractExpression<TNode>[] _subExpressions;

    public UnionOperator(AbstractExpression<TNode>[] childExpressions) : base(
        childExpressions.Aggregate(new Specificity(),
            (currentMaxSpecificity, selector) => currentMaxSpecificity > selector.Specificity
                ? currentMaxSpecificity
                : selector.Specificity
        ),
        childExpressions,
        new OptimizationOptions(childExpressions.All(e => e.CanBeStaticallyEvaluated)))
    {
        _subExpressions = childExpressions;
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode>? executionParameters)
    {
        if (_subExpressions.All(e => e.ExpectedResultOrder == ResultOrdering.Sorted))
        {
            var i = 0;
            // Every sequence is locally sorted: we can merge them. This saves a lot of unneeded
            // sorting for sequences that are already naturally sorted.
            return SortedSequenceUtils<TNode>.MergeSortedSequences(executionParameters!.DomFacade,
                _ =>
                {
                    if (i >= _subExpressions.Length) return IteratorResult<ISequence>.Done();
                    return IteratorResult<ISequence>.Ready(
                        _subExpressions[i++].EvaluateMaybeStatically(
                            dynamicContext,
                            executionParameters
                        )
                    );
                }).Map((value, _, _) => !value.GetValueType().IsSubtypeOf(ValueType.Node) 
                ? throw new XPathException("XPTY0004", "The sequences to union are not of type node()*")
                : value);
        }

        return SequenceUtils.ConcatSequences(
            _subExpressions.Select(e =>
                e.EvaluateMaybeStatically(dynamicContext, executionParameters)
            )
        ).MapAll(allValues =>
        {
            if (allValues.Any(nodeValue => !nodeValue.GetValueType().IsSubtypeOf(ValueType.Node)))
                throw new XPathException("XPTY0004", "The sequences to union are not of type node()*");

            var sortedValues = DocumentOrderUtils<TNode>.SortNodeValues(executionParameters!.DomFacade,
                allValues.Cast<NodeValue<TNode>>().ToList());
            return SequenceFactory.CreateFromArray(sortedValues.Cast<AbstractValue>().ToArray());
        });
    }
}