using System.Xml;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions.Axes;

public class FollowingSiblingAxis : AbstractExpression
{
    private readonly AbstractTestExpression _testExpression;

    public FollowingSiblingAxis(AbstractTestExpression testExpression) : base(new AbstractExpression[] {testExpression},
        new OptimizationOptions(false))
    {
        _testExpression = testExpression;
    }

    private static Iterator<AbstractValue> CreateSiblingIterator(XmlNode? node)
    {
        return _ =>
        {
            node = node?.NextSibling;

            return node == null
                ? IteratorResult<AbstractValue>.Done()
                : IteratorResult<AbstractValue>.Ready(new NodeValue(node));
        };
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
    {
        var contextItem = ContextNodeUtils.ValidateContextNode(dynamicContext!.ContextItem!);

        return SequenceFactory.CreateFromIterator(CreateSiblingIterator(contextItem.Value))
            .Filter((item, _, _) =>
                _testExpression.EvaluateToBoolean(dynamicContext, item, executionParameters));
    }
}