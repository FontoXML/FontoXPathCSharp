using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

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
        // TODO: implement better sorting. This is just used to make sure filtering out duplicates works as expected
        Array.Sort<AbstractValue>(nodeValues, (left, right) =>
        {
            var leftXml = left.GetAs<NodeValue>(ValueType.Node)!.Value.OuterXml;
            var rightXml = right.GetAs<NodeValue>(ValueType.Node)!.Value.OuterXml;
            return string.CompareOrdinal(leftXml, rightXml);
        });

        return nodeValues.Select((x, i) => (x, i))
            .Where(tuple =>
            {
                if (tuple.i == 0) 
                    return true;

                var firstNode = tuple.x.GetAs<NodeValue>(ValueType.Node)!.Value;
                var secondNode =
                    nodeValues[tuple.i - 1].GetAs<NodeValue>(ValueType.Node)!.Value;

                return firstNode != secondNode;
            })
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