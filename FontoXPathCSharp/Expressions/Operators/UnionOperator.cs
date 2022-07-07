using System.Xml;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.Operators;

public class UnionOperator : AbstractExpression
{
    private readonly AbstractExpression[] _subExpressions;

    public UnionOperator(AbstractExpression[] childExpressions) : base(childExpressions,
        new OptimizationOptions(childExpressions.All(e => e.CanBeStaticallyEvaluated)))
    {
        _subExpressions = childExpressions;
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
    {
        if (_subExpressions.All(e => e.ResultOrder == ResultOrdering.Sorted))
            throw new NotImplementedException("Returning sorted sequence unions not implemented yet");
        return ExpressionUtils.ConcatSequences(
            _subExpressions.Select(e =>
                e.EvaluateMaybeStatically(dynamicContext, executionParameters)
            )
        ).MapAll(allValues =>
        {
            if (allValues.Any(nodeValue => !nodeValue.GetValueType().IsSubtypeOf(ValueType.Node)))
                throw new XPathException("XPTY0004: The sequences to union are not of type node()*");

            var sortedValues = SortNodeValues(executionParameters?.DomFacade, allValues);
            return SequenceFactory.CreateFromArray(sortedValues);
        });
    }

    // Probably belongs in a utility function class.
    private AbstractValue[] SortNodeValues(XmlNode? domFacade, IEnumerable<AbstractValue> allValues)
    {
        return allValues.OrderBy(e => e).Distinct().ToArray();
        // TODO: Add proper comparator later.
        // TODO: Do proper duplicate pruning.
    }
}