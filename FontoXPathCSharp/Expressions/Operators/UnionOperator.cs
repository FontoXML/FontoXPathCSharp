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

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode> executionParameters)
    {
        if (_subExpressions.All(e => e.ExpectedResultOrder == ResultOrdering.Sorted))
            throw new NotImplementedException("Returning sorted sequence unions not implemented yet");
        return SequenceUtils.ConcatSequences(
            _subExpressions.Select(e =>
                e.EvaluateMaybeStatically(dynamicContext, executionParameters)
            )
        ).MapAll(allValues =>
        {
            if (allValues.Any(nodeValue => !nodeValue.GetValueType().IsSubtypeOf(ValueType.Node)))
                throw new XPathException("XPTY0004", "The sequences to union are not of type node()*");

            var sortedValues = DocumentOrderUtils<TNode>.SortNodeValues(executionParameters.DomFacade,
                allValues.Cast<NodeValue<TNode>>().ToList());
            return SequenceFactory.CreateFromArray(sortedValues.Cast<AbstractValue>().ToArray());
        });
    }
}