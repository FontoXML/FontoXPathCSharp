using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions.Axes;

public class PrecedingAxis<TNode> : AbstractExpression<TNode> where TNode : notnull
{
    private readonly string? _bucket;
    private readonly AbstractTestExpression<TNode> _testExpression;

    public PrecedingAxis(AbstractTestExpression<TNode> testExpression) : base(
        testExpression.Specificity,
        new AbstractExpression<TNode>[] { testExpression },
        new OptimizationOptions(
            false,
            true,
            ResultOrdering.ReverseSorted)
    )
    {
        _testExpression = testExpression;
        var testBucket = testExpression.GetBucket();
        var onlyElementDescendants = testBucket != null &&
                                     (testBucket.StartsWith(BucketConstants.NamePrefix) ||
                                      testBucket == BucketConstants.Type1);
        _bucket = onlyElementDescendants ? BucketConstants.Type1 : null;
    }

    private static Iterator<AbstractValue> CreatePrecedingGenerator(IDomFacade<TNode> domFacade, TNode node,
        string? bucket)
    {
        var nodeStack = new List<TNode>();

        for (var ancestorNode = node;
             ancestorNode != null && !domFacade.IsDocument(node);
             ancestorNode = domFacade.GetParentNode(ancestorNode))
        {
            var previousSibling = domFacade.GetPreviousSibling(ancestorNode, bucket);
            if (previousSibling == null)
                continue;
            nodeStack.Add(previousSibling);
        }

        Iterator<AbstractValue>? nephewGenerator = null;
        return _ =>
        {
            while (nephewGenerator != null || nodeStack.Any())
            {
                nephewGenerator ??= AxesUtils<TNode>.CreateDescendantIterator(
                    domFacade,
                    nodeStack.First(),
                    true,
                    bucket
                );

                var nephew = nephewGenerator(IterationHint.None);

                if (!nephew.IsDone) return nephew;

                nephewGenerator = null;

                var nextNode = domFacade.GetPreviousSibling(nodeStack.First(), bucket);
                var toReturn = IteratorResult<AbstractValue>.Ready(new NodeValue<TNode>(nodeStack.First(), domFacade));
                if (nextNode == null) nodeStack.RemoveAt(0);
                else nodeStack[0] = nextNode;

                return toReturn;
            }

            return IteratorResult<AbstractValue>.Done();
        };
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode>? executionParameters)
    {
        var domFacade = executionParameters!.DomFacade;
        var contextItem = ContextNodeUtils<TNode>.ValidateContextNode(dynamicContext!.ContextItem!);

        return SequenceFactory.CreateFromIterator(CreatePrecedingGenerator(domFacade, contextItem, _bucket))
            .Filter((item, _, _) =>
                _testExpression.EvaluateToBoolean(dynamicContext, item, executionParameters));
    }
}