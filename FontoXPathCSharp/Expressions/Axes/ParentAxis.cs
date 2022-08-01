using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions.Axes;

public class ParentAxis<TNode> : AbstractExpression<TNode>
{
    private readonly AbstractTestExpression<TNode> _parentExpression;

    public ParentAxis(AbstractTestExpression<TNode> parentExpression) : base(
        new AbstractExpression<TNode>[] { parentExpression },
        new OptimizationOptions(false))
    {
        _parentExpression = parentExpression;
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode> executionParameters)
    {
        var domFacade = executionParameters.DomFacade;
        var contextNode = ContextNodeUtils<TNode>.ValidateContextNode(dynamicContext.ContextItem);
        var parentNode = domFacade.GetParentNode(contextNode.Value);

        if (parentNode == null) return SequenceFactory.CreateEmpty();

        var parentNodeValue = new NodeValue<TNode>(parentNode, domFacade);
        var nodeIsMatch = _parentExpression.EvaluateToBoolean(
            dynamicContext,
            parentNodeValue,
            executionParameters
        );

        return !nodeIsMatch ? SequenceFactory.CreateEmpty() : SequenceFactory.CreateFromValue(parentNodeValue);
    }
}