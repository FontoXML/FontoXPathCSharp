using System.Xml;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions.Axes;

public class PrecedingAxis : AbstractExpression
{
    private readonly AbstractTestExpression _testExpression;

    public PrecedingAxis(AbstractTestExpression testExpression) : base(new AbstractExpression[] { testExpression },
        new OptimizationOptions(false))
    {
        _testExpression = testExpression;
    }

    private static Iterator<AbstractValue> CreatePrecedingIterator(XmlNode node)
    {
        var nodeStack = new List<XmlNode>();

        for (var ancestorNode = node;
             ancestorNode != null && ancestorNode.NodeType != XmlNodeType.Document;
             ancestorNode = ancestorNode.ParentNode)
        {
            var previousSibling = ancestorNode.PreviousSibling;
            if (previousSibling == null)
                continue;
            nodeStack.Add(previousSibling);
        }

        Iterator<AbstractValue>? nephewGenerator = null;
        return _ =>
        {
            while (nephewGenerator != null || nodeStack.Any())
            {
                nephewGenerator ??= AxesUtils.CreateDescendantIterator(nodeStack.First(), true);

                var nephew = nephewGenerator(IterationHint.None);

                if (!nephew.IsDone)
                    return nephew;

                nephewGenerator = null;

                var nextNode = nodeStack.First().PreviousSibling;
                var toReturn = IteratorResult<AbstractValue>.Ready(new NodeValue(nodeStack.First()));
                if (nextNode == null)
                    nodeStack.RemoveAt(0);
                else
                    nodeStack[0] = nextNode;

                return toReturn;
            }

            return IteratorResult<AbstractValue>.Done();
        };
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
    {
        var contextItem = ContextNodeUtils.ValidateContextNode(dynamicContext!.ContextItem!);

        return SequenceFactory.CreateFromIterator(CreatePrecedingIterator(contextItem.Value))
            .Filter((item, _, _) =>
                _testExpression.EvaluateToBoolean(dynamicContext, item, executionParameters));
    }
}