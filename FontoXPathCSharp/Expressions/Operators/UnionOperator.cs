using System.Xml;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;

namespace FontoXPathCSharp.Expressions.Operators;

public class UnionOperator : AbstractExpression
{
    private AbstractExpression[] _subExpressions;
    
    public UnionOperator(AbstractExpression[] childExpressions) : base(childExpressions, new OptimizationOptions(childExpressions.All(e => e.CanBeStaticallyEvaluated)))
    {
        _subExpressions = childExpressions;
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
    {
        if (_subExpressions.All(e => e.ResultOrder == ResultOrdering.Sorted))
        {
            throw new NotImplementedException("Returning sorted sequence unions not implemented yet");
        }
        return ExpressionUtils.ConcatSequences(
            _subExpressions.Select(e =>
                e.EvaluateMaybeStatically(dynamicContext, executionParameters)
            )
        ).MapAll(allValues => {
            if (allValues.Any(nodeValue => !SubtypeUtils.IsSubtypeOf(nodeValue.GetValueType(), Value.Types.ValueType.Node))) {
                throw new XPathException("XPTY0004: The sequences to union are not of type node()*");
            }

            var sortedValues = SortNodeValues(executionParameters?.DomFacade, allValues);
            return SequenceFactory.CreateFromArray(sortedValues);
        });
    }

    // Probably belongs in a utility function class.
    private AbstractValue[] SortNodeValues(XmlNode? domFacade, AbstractValue[] allValues)
    {
        var sortedListValues = allValues.ToList();
        // TODO: Add proper comparator later.
        sortedListValues.Sort();
        // TODO: Do duplicate pruning.
        return sortedListValues.ToArray();
    }
}