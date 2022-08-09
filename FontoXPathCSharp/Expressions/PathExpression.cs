using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Expressions.DataTypes;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions;

public class PathExpression<TNode> : AbstractExpression<TNode>
{
    private readonly AbstractExpression<TNode>[] _stepExpressions;

    public PathExpression(AbstractExpression<TNode>[] stepExpressions) : base(stepExpressions,
        new OptimizationOptions(false))
    {
        _stepExpressions = stepExpressions;
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
        {
            if (resultValue.GetValueType().IsSubtypeOf(ValueType.Node)) resultContainsNodes = true;
            else resultContainsNonNodes = true;
        }

        if (resultContainsNonNodes && resultContainsNodes)
        {
            throw new XPathException("XPTY0018: The path operator should either return nodes or non-nodes. " +
                                     "Mixed sequences are not allowed."
            );
        }
        
        if (resultContainsNodes) {
            return DocumentOrderUtils<TNode>
                .SortNodeValues(domFacade, result.Cast<NodeValue<TNode>>().ToList())
                .Cast<AbstractValue>()
                .ToArray();
        }
        return result;
    }


    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode> executionParameters)
    {
        var domFacade = executionParameters.DomFacade;
        var result = _stepExpressions.Aggregate(SequenceFactory.CreateFromValue(dynamicContext!.ContextItem!),
            (intermediateResultNodesSequence, selector) =>
            {
                var resultInOrderOfEvaluation = intermediateResultNodesSequence
                    .SelectMany(c =>
                    {
                        dynamicContext.ContextItem = c;
                        return selector.Evaluate(dynamicContext, executionParameters);
                    }).ToArray();

                return SequenceFactory.CreateFromArray(SortResults(domFacade, resultInOrderOfEvaluation));
            });


        return result;
    }
}