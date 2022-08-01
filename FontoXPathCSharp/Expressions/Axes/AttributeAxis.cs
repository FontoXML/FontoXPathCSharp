using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.Axes;

public class AttributeAxis<TNode> : AbstractExpression<TNode>
{
    private readonly AbstractTestExpression<TNode> _selector;

    public AttributeAxis(AbstractTestExpression<TNode> selector) : base(new AbstractExpression<TNode>[] { selector },
        new OptimizationOptions(false))
    {
        _selector = selector;
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode> executionParameters)
    {
        var domFacade = executionParameters.DomFacade;

        var contextItem = ContextNodeUtils<TNode>.ValidateContextNode(dynamicContext!.ContextItem!);

        if (contextItem.GetValueType() != ValueType.Element) return SequenceFactory.CreateEmpty();

        //TODO: LINQ this stuff properly
        var matchingAttributes = new List<NodeValue<TNode>>();
        foreach (var attr in domFacade.GetAllAttributes(contextItem.Value))
        {
            if (domFacade.GetNamespaceUri(attr) == BuiltInNamespaceUris.XmlnsNamespaceUri.GetUri())
                continue;
            var nodeValue = new NodeValue<TNode>(attr, domFacade);
            var matches = _selector.EvaluateToBoolean(dynamicContext, nodeValue, executionParameters);
            if (matches)
                matchingAttributes.Add(nodeValue);
        }

        return SequenceFactory.CreateFromArray(matchingAttributes.Cast<AbstractValue>().ToArray());
    }
}