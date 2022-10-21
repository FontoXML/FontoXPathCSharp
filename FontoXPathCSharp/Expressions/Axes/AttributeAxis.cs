using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.Axes;

public class AttributeAxis<TNode> : AbstractExpression<TNode> where TNode : notnull
{
    private readonly AbstractTestExpression<TNode> _attributeTestExpression;
    private readonly string? _filterBucket;

    public AttributeAxis(AbstractTestExpression<TNode> attributeTestExpression, string? filterBucket) : base(
        new Specificity(new Dictionary<SpecificityKind, int> { { SpecificityKind.Attribute, 1 } }),
        new AbstractExpression<TNode>[] { attributeTestExpression },
        new OptimizationOptions(false))
    {
        _attributeTestExpression = attributeTestExpression;
        _filterBucket = BucketUtils.IntersectBuckets(_attributeTestExpression.GetBucket(), filterBucket);
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode>? executionParameters)
    {
        var domFacade = executionParameters!.DomFacade;

        var contextItem = ContextNodeUtils<TNode>.ValidateContextNode(dynamicContext!.ContextItem!);

        if (contextItem.GetValueType() != ValueType.Element) return SequenceFactory.CreateEmpty();

        //TODO: LINQ this stuff properly
        var matchingAttributes = new List<NodeValue<TNode>>();
        foreach (var attr in domFacade.GetAllAttributes(contextItem.Value, _filterBucket))
        {
            if (domFacade.GetNamespaceUri(attr) == BuiltInNamespaceUris.XmlnsNamespaceUri.GetUri())
                continue;
            var nodeValue = new NodeValue<TNode>(attr, domFacade);
            var matches = _attributeTestExpression.EvaluateToBoolean(dynamicContext, nodeValue, executionParameters);
            if (matches) matchingAttributes.Add(nodeValue);
        }

        return SequenceFactory.CreateFromArray(matchingAttributes.Cast<AbstractValue>().ToArray());
    }
}