using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions.Axes;

public class PrecedingSiblingAxis<TNode> : AbstractExpression<TNode>
{
    private readonly AbstractTestExpression<TNode> _testExpression;

    public PrecedingSiblingAxis(AbstractTestExpression<TNode> testExpression) : base(
        new AbstractExpression<TNode>[] { testExpression },
        new OptimizationOptions(false))
    {
        _testExpression = testExpression;
    }

    private static Iterator<AbstractValue> CreateSiblingIterator(IDomFacade<TNode> domFacade, TNode? node)
    {
        return _ =>
        {
            node = node != null ? domFacade.GetPreviousSibling(node) : default;

            return node == null
                ? IteratorResult<AbstractValue>.Done()
                : IteratorResult<AbstractValue>.Ready(new NodeValue<TNode>(node, domFacade));
        };
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode> executionParameters)
    {
        var domFacade = executionParameters.DomFacade;
        var contextItem = ContextNodeUtils<TNode>.ValidateContextNode(dynamicContext!.ContextItem!);

        return SequenceFactory.CreateFromIterator(CreateSiblingIterator(domFacade, contextItem.Value))
            .Filter((item, _, _) =>
                _testExpression.EvaluateToBoolean(dynamicContext, item, executionParameters));
    }
}