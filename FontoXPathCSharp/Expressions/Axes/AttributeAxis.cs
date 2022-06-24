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

        if (contextItem != null && contextItem.GetValueType() != ValueType.Element)
        {
            return SequenceFactory.CreateEmpty();
        }

        var matchingAttributes = domfacade?.Attributes?.Cast<XmlAttribute>()
            .Where(attr => attr.NamespaceURI != BuiltInNamespaceUris.XmlnsNamespaceUri.GetUri())
            .Select(attr => new AttributeNodePointer(new Attr(attr.LocalName, attr.Name, attr.NamespaceURI,
                attr.Name, attr.Prefix, attr.Value)) as AbstractValue)
            .Where(attribPointer => _selector.EvaluateToBoolean(dynamicContext, attribPointer, executionParameters));

        return SequenceFactory.CreateFromArray((matchingAttributes ?? Array.Empty<AbstractValue>()).ToArray());
    }
}