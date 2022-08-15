using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.Axes;

public class ChildAxis<TNode> : AbstractExpression<TNode>
{
    //TODO: Bucket stuff
    private readonly AbstractTestExpression<TNode> _childExpression;

    public ChildAxis(AbstractTestExpression<TNode> childExpression) : base(
        new AbstractExpression<TNode>[] { childExpression },
        new OptimizationOptions(false))
    {
        _childExpression = childExpression;
    }

    public override string ToString()
    {
        return $"ChildAxis[ {_childExpression} ]";
    }


    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode> executionParameters)
    {
        var domFacade = executionParameters.DomFacade;
        var contextNode = ContextNodeUtils<TNode>.ValidateContextNode(dynamicContext?.ContextItem);
        var nodeType = contextNode.GetValueType();

        if (nodeType != ValueType.Element && nodeType != ValueType.DocumentNode) return SequenceFactory.CreateEmpty();

        var node = default(TNode);
        var isDone = false;

        return SequenceFactory.CreateFromIterator(_ =>
        {
            while (!isDone)
            {
                if (node == null)
                {
                    node = domFacade.GetFirstChild(contextNode.Value);
                    if (node == null)
                    {
                        isDone = true;
                        continue;
                    }

                    return IteratorResult<AbstractValue>.Ready(new NodeValue<TNode>(node, domFacade));
                }

                node = domFacade.GetNextSibling(node);
                if (node == null)
                {
                    isDone = true;
                    continue;
                }

                return IteratorResult<AbstractValue>.Ready(new NodeValue<TNode>(node, domFacade));
            }

            return IteratorResult<AbstractValue>.Done();
        }).Filter((item, _, _) => _childExpression.EvaluateToBoolean(
            dynamicContext,
            item,
            executionParameters)
        );
    }
}