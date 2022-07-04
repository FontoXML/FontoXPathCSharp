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

    private static AbstractValue[] SortNodeValues(IReadOnlyList<AbstractValue> nodeValues)
    {
        // TODO: actually implement sorting
        return nodeValues.Select((x, i) => (x, i))
            .Where(tuple => tuple.i == 0 || tuple.x.Equals(nodeValues[tuple.i - 1]))
            .Select(tuple => tuple.x)
            .ToArray();
    }


    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
    {
        var result = _stepExpressions.Aggregate(SequenceFactory.CreateFromArray(new[] {dynamicContext!.ContextItem!}),
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