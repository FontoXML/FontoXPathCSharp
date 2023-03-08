using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions;

public class AbsolutePathExpression<TNode> : AbstractExpression<TNode> where TNode : notnull
{
    private readonly AbstractExpression<TNode>? _relativePathExpression;

    public AbsolutePathExpression(AbstractExpression<TNode>? relativePathExpression) :
        base(
            relativePathExpression != null ? relativePathExpression.Specificity : new Specificity(),
            relativePathExpression != null
                ? new[] { relativePathExpression }
                : Array.Empty<AbstractExpression<TNode>>(),
            new OptimizationOptions(
                false,
                false,
                ResultOrdering.Sorted)
        )
    {
        _relativePathExpression = relativePathExpression;
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode>? executionParameters)
    {
        if (dynamicContext?.ContextItem == null)
            throw new XPathException("XPDY0002", "Context is absent, it needs to be present to use paths.");

        var node = dynamicContext.ContextItem.GetAs<NodeValue<TNode>>().Value;
        var domFacade = executionParameters!.DomFacade;

        var documentNode = node;

        while (!domFacade.IsDocument(documentNode))
        {
            documentNode = domFacade.GetParentNode(documentNode);

            if (documentNode == null)
                throw new XPathException(
                    "XPDY0050",
                    "The root node of the context node is not a document node."
                );
        }


        var contextSequence = SequenceFactory.CreateFromValue(new NodeValue<TNode>(documentNode, domFacade));

        return _relativePathExpression != null
            ? _relativePathExpression.EvaluateMaybeStatically(
                dynamicContext.ScopeWithFocus(0, contextSequence.First(), contextSequence),
                executionParameters)
            : contextSequence;
    }
}