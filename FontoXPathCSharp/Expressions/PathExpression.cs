using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions;

public class PathExpression : AbstractExpression
{
    private readonly AbstractExpression[] _stepExpressions;

    public PathExpression(AbstractExpression[] stepExpressions) : base(stepExpressions, new OptimizationOptions(false))
    {
        _stepExpressions = stepExpressions;
    }

    public override string ToString()
    {
        return $"PathExpr[ {string.Join(", ", _stepExpressions.Select(x => x.ToString()))} ]";
    }

    private static AbstractValue[] SortNodeValues(AbstractValue[] nodeValues)
    {
        // TODO: Add sorting
        return nodeValues
            .DistinctBy(value => value.GetAs<NodeValue>().Value)
            .ToArray();
    }


    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
    {
        var result = _stepExpressions.Aggregate(SequenceFactory.CreateFromValue(dynamicContext!.ContextItem!),
            (intermediateResultNodesSequence, selector) =>
            {
                var resultInOrderOfEvaluation = intermediateResultNodesSequence
                    .SelectMany(c =>
                    {
                        dynamicContext.ContextItem = c;
                        return selector.Evaluate(dynamicContext, executionParameters);
                    }).ToArray();

                return SequenceFactory.CreateFromArray(SortNodeValues(resultInOrderOfEvaluation));
            });


        return result;
    }
}