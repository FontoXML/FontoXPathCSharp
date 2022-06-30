using System.Xml;
using FontoXPathCSharp.Expressions.Axes;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions;

public class ChildAxis : AbstractExpression
{
    private readonly AbstractTestExpression _selector;

    public ChildAxis(AbstractTestExpression selector) : base(new AbstractExpression[] { selector },
        new OptimizationOptions(false))
    {
        _selector = selector;
    }

    public override string ToString()
    {
        return $"ChildAxis[ {_selector} ]";
    }


    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
    {
        var contextNode = ContextNodeUtils.ValidateContextNode(dynamicContext?.ContextItem);
        var nodeType = contextNode.GetValueType();

        switch (nodeType)
        {
            case ValueType.Element:
            {
                var element = (XmlElement)contextNode.Value;
                var children = element.ChildNodes;
                var filteredChildren = new List<AbstractValue>();
                for (var i = 0; i < children.Count; ++i)
                {
                    var child = children[i]!;
                    var childNodeValue = new NodeValue(child);
                    var childDynamicContext = new DynamicContext(childNodeValue, i);
                    if (_selector.EvaluateToBoolean(childDynamicContext, childNodeValue, executionParameters))
                        filteredChildren.Add(childNodeValue);
                }

                return SequenceFactory.CreateFromArray(filteredChildren.ToArray());
            }
            case ValueType.DocumentNode:
            {
                var element = (XmlDocument)contextNode.Value;
                var children = element.ChildNodes;
                var filteredChildren = new List<AbstractValue>();
                for (var i = 0; i < children.Count; ++i)
                {
                    var child = children[i]!;
                    //TODO: Document Value Type
                    var childNodeValue = new NodeValue(child);
                    var childDynamicContext = new DynamicContext(childNodeValue, i);
                    if (_selector.EvaluateToBoolean(childDynamicContext, childNodeValue, executionParameters))
                        filteredChildren.Add(childNodeValue);
                }

                return SequenceFactory.CreateFromArray(filteredChildren.ToArray());
            }
            default:
                return SequenceFactory.CreateEmpty();
        }
    }
}