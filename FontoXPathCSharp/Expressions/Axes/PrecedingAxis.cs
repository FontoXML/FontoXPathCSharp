using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions.Axes;

public class PrecedingAxis<TNode> : AbstractExpression<TNode>
{
    private readonly AbstractTestExpression<TNode> _testExpression;

    public PrecedingAxis(AbstractTestExpression<TNode> testExpression) : base(
        new AbstractExpression<TNode>[] { testExpression },
        new OptimizationOptions(false))
    {
        _testExpression = testExpression;
    }

    private static Iterator<AbstractValue> CreatePrecedingGenerator(IDomFacade<TNode> domFacade, TNode node)
    {
        var nodeStack = new List<TNode>();

        for (var ancestorNode = node;
             ancestorNode != null && !domFacade.IsDocument(node);
             ancestorNode = domFacade.GetParentNode(ancestorNode))
        {
            var previousSibling = domFacade.GetPreviousSibling(ancestorNode);
            if (previousSibling == null)
                continue;
            nodeStack.Add(previousSibling);
        }

        Iterator<AbstractValue>? nephewGenerator = null;
        return _ =>
        {
            while (nephewGenerator != null || nodeStack.Any())
            {
                nephewGenerator ??= AxesUtils<TNode>.CreateDescendantIterator(domFacade, nodeStack.First(), true);

                var nephew = nephewGenerator(IterationHint.None);

                if (!nephew.IsDone)
                    return nephew;

                nephewGenerator = null;

                var nextNode = domFacade.GetPreviousSibling(nodeStack.First());
                var toReturn = IteratorResult<AbstractValue>.Ready(new NodeValue<TNode>(nodeStack.First(), domFacade));
                if (nextNode == null)
                    nodeStack.RemoveAt(0);
                else
                    nodeStack[0] = nextNode;

                return toReturn;
            }

            return IteratorResult<AbstractValue>.Done();
        };
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode> executionParameters)
    {
        var domFacade = executionParameters.DomFacade;
        var contextItem = ContextNodeUtils<TNode>.ValidateContextNode(dynamicContext!.ContextItem!);

        return SequenceFactory.CreateFromIterator(CreatePrecedingGenerator(domFacade, contextItem.Value))
            .Filter((item, _, _) =>
                _testExpression.EvaluateToBoolean(dynamicContext, item, executionParameters));
    }
}