using System.Xml;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions.Axes;

public class FollowingAxis : AbstractExpression
{
    private readonly AbstractTestExpression _testExpression;

    public FollowingAxis(AbstractTestExpression testExpression) : base(new AbstractExpression[] { testExpression },
        new OptimizationOptions(false))
    {
        _testExpression = testExpression;
    }

    private static Iterator<AbstractValue> CreateFollowingGenerator(XmlNode node)
    {
        var nodeStack = new List<XmlNode>();

        for (var ancestorNode = node;
             ancestorNode != null && ancestorNode.NodeType != XmlNodeType.Document;
             ancestorNode = ancestorNode.ParentNode)
        {
            var previousSibling = ancestorNode.NextSibling;
            if (previousSibling != null)
                nodeStack.Add(previousSibling);
        }

        Iterator<AbstractValue>? nephewGenerator = null;
        return _ =>
        {
            while (nephewGenerator != null || nodeStack.Count > 0)
            {
                if (nephewGenerator == null)
                {
                    nephewGenerator = AxesUtils.CreateDescendantIterator(nodeStack.First(), false);

                    var toReturn = IteratorResult<AbstractValue>.Ready(new NodeValue(nodeStack.First()));

                    var nextNode = nodeStack.First().NextSibling;
                    if (nextNode == null)
                        nodeStack.RemoveAt(0);
                    else
                        nodeStack[0] = nextNode;

                    return toReturn;
                }

                var nephew = nephewGenerator(IterationHint.None);
                if (!nephew.IsDone)
                    return nephew;

                nephewGenerator = null;
            }

            return IteratorResult<AbstractValue>.Done();
        };
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
    {
        var contextItem = ContextNodeUtils.ValidateContextNode(dynamicContext!.ContextItem!);

        return SequenceFactory.CreateFromIterator(CreateFollowingGenerator(contextItem.Value)).Filter((item, _, _) =>
            _testExpression.EvaluateToBoolean(dynamicContext, item, executionParameters));
    }
}