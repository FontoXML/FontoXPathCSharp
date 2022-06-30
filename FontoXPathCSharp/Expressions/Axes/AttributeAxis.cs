using System.Xml;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Types.Node;
using FontoXPathCSharp.Value;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.Axes;

public class AttributeAxis : AbstractExpression
{
    private readonly AbstractTestExpression _selector;

    public AttributeAxis(AbstractTestExpression selector) : base(new AbstractExpression[] { selector },
        new OptimizationOptions(false))
    {
        _selector = selector;
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
    {
        var domfacade = executionParameters?.DomFacade;
        var contextItem = ContextNodeUtils.ValidateContextNode(dynamicContext!.ContextItem!);

        if (contextItem.GetValueType() != ValueType.Element)
        {
            return SequenceFactory.CreateEmpty();
        }

        // var matchingAttributes = domfacade?.Attributes?.Cast<XmlAttribute>()
        //     .Where(attr => attr.NamespaceURI != BuiltInNamespaceUris.XmlnsNamespaceUri.GetUri())
        //     .Select(attr => new NodeValue(attr))
        //     .Where(attribPointer => _selector.EvaluateToBoolean(dynamicContext, attribPointer, executionParameters));

        var matchingAttributes = new List<NodeValue>();
        foreach (XmlAttribute attr in contextItem.Value.Attributes)
        {
            if (attr.NamespaceURI == BuiltInNamespaceUris.XmlnsNamespaceUri.GetUri()) continue;
            var nodeValue = new NodeValue(attr);
            if (_selector.EvaluateToBoolean(dynamicContext, nodeValue, executionParameters)) matchingAttributes.Add(nodeValue);
        }

        return SequenceFactory.CreateFromArray(matchingAttributes.ToArray());
    }
}