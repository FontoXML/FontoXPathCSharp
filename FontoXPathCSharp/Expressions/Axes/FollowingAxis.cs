using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions.Axes;

public class FollowingAxis<TNode> : AbstractExpression<TNode>
{
    private readonly string? _bucket;
    private readonly AbstractTestExpression<TNode> _testExpression;

    public FollowingAxis(AbstractTestExpression<TNode> testExpression) : base(
        testExpression.Specificity,
        new AbstractExpression<TNode>[] { testExpression },
        new OptimizationOptions(false))
    {
        _testExpression = testExpression;
        var testBucket = testExpression.GetBucket();
        var onlyElementDescendants = testBucket != null &&
                                     (testBucket.StartsWith(BucketConstants.NamePrefix) ||
                                      testBucket == BucketConstants.Type1);
        _bucket = onlyElementDescendants ? BucketConstants.Type1 : null;
    }

    private static Iterator<AbstractValue> CreateFollowingGenerator(IDomFacade<TNode> domFacade, TNode node,
        string? bucket)
    {
        var nodeStack = new List<TNode>();

        for (var ancestorNode = node;
             ancestorNode != null && !domFacade.IsDocument(ancestorNode);
             ancestorNode = domFacade.GetParentNode(ancestorNode))
        {
            var previousSibling = domFacade.GetNextSibling(ancestorNode, bucket);
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
                    nephewGenerator = AxesUtils<TNode>.CreateDescendantIterator(
                        domFacade,
                        nodeStack.First(),
                        false,
                        bucket
                    );

                    var toReturn =
                        IteratorResult<AbstractValue>.Ready(new NodeValue<TNode>(nodeStack.First(), domFacade));

                    var nextNode = domFacade.GetNextSibling(nodeStack.First(), bucket);
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

        return SequenceFactory.CreateFromIterator(CreateFollowingGenerator(domFacade, contextItem.Value, _bucket))
            .Filter(
                (item, _, _) =>
                    _testExpression.EvaluateToBoolean(dynamicContext, item, executionParameters));
    }
}