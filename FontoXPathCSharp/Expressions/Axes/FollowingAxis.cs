using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions.Axes;

public class FollowingAxis<TNode> : AbstractExpression<TNode>
{
    private readonly AbstractTestExpression<TNode> _testExpression;

    public FollowingAxis(AbstractTestExpression<TNode> testExpression) : base(
        new AbstractExpression<TNode>[] { testExpression },
        new OptimizationOptions(false))
    {
        _testExpression = testExpression;
    }

    private static Iterator<AbstractValue> CreateFollowingGenerator(IDomFacade<TNode> domFacade, TNode node)
    {
        var nodeStack = new List<TNode>();

        for (var ancestorNode = node;
             ancestorNode != null && !domFacade.IsDocument(ancestorNode);
             ancestorNode = domFacade.GetParentNode(ancestorNode))
        {
            var previousSibling = domFacade.GetNextSibling(ancestorNode);
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
                    nephewGenerator = AxesUtils<TNode>.CreateDescendantIterator(domFacade, nodeStack.First(), false);

                    var toReturn =
                        IteratorResult<AbstractValue>.Ready(new NodeValue<TNode>(nodeStack.First(), domFacade));

                    var nextNode = domFacade.GetNextSibling(nodeStack.First());
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

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode> executionParameters)
    {
        var domFacade = executionParameters.DomFacade;
        var contextItem = ContextNodeUtils<TNode>.ValidateContextNode(dynamicContext!.ContextItem!);

        return SequenceFactory.CreateFromIterator(CreateFollowingGenerator(domFacade, contextItem.Value)).Filter(
            (item, _, _) =>
                _testExpression.EvaluateToBoolean(dynamicContext, item, executionParameters));
    }
}