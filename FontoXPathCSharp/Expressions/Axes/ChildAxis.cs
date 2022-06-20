using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Expressions.Axes;
using FontoXPathCSharp.Value;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

using System.Xml;
namespace FontoXPathCSharp.Expressions;


public class ChildAxis : AbstractExpression
{
    private readonly AbstractTestExpression _selector;

    public ChildAxis(AbstractTestExpression selector) : base(new AbstractExpression[] { selector },
        new OptimizationOptions(false))
    {
        _selector = selector;
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
    {
        ContextNodeUtils.ValidateContextNode(dynamicContext!.ContextItem!);
        var contextNode = dynamicContext.ContextItem.GetAs<NodeValue>(ValueType.Node)!.Value();

        if (contextNode?.NodeType is XmlNodeType.Element)
        {
            var element = (System.Xml.XmlElement)contextNode;
            var children = element.ChildNodes;
            var filteredChildren = new List<NodeValue>();
            for (var i = 0; i < children.Count; ++i)
            {
                var child = children[i]!;
                var childNodeValue = new NodeValue(child);
                var childDynamicContext = new DynamicContext(childNodeValue, i);
                if (this._selector.EvaluateToBoolean(childDynamicContext, childNodeValue, executionParameters))
                {
                    filteredChildren.Add(childNodeValue);
                }
            }
            return SequenceFactory.CreateFromArray(filteredChildren.ToArray());
        }
        if (contextNode?.NodeType is XmlNodeType.Document)
        {
            var element = (System.Xml.XmlDocument)contextNode;
            var children = element.ChildNodes;
            var filteredChildren = new List<NodeValue>();
            for (var i = 0; i < children.Count; ++i)
            {
                var child = children[i]!;
                var childNodeValue = new NodeValue(child);
                var childDynamicContext = new DynamicContext(childNodeValue, i);
                if (this._selector.EvaluateToBoolean(childDynamicContext, childNodeValue, executionParameters))
                {
                    filteredChildren.Add(childNodeValue);
                }
            }
            return SequenceFactory.CreateFromArray(filteredChildren.ToArray());
        }


        return SequenceFactory.CreateEmpty();
    }
}