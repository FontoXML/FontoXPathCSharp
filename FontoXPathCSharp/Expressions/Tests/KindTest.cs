using System.Xml;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.Tests;

public class KindTest : AbstractTestExpression
{
    private readonly XmlNodeType _nodeType;

    public KindTest(XmlNodeType nodeType)
    {
        _nodeType = nodeType;
    }

    protected internal override bool EvaluateToBoolean(DynamicContext? dynamicContext, AbstractValue value,
        ExecutionParameters? executionParameters)
    {
        if (!value.GetValueType().IsSubtypeOf(ValueType.Node))
            return false;

        var nodeType = value.GetAs<NodeValue>().Value.NodeType;
        if (_nodeType == XmlNodeType.Text && nodeType == XmlNodeType.CDATA)
            // CDATA_SECTION_NODES should be regarded as text nodes, and CDATA does not exist in the XPath Data Model
            return true;

        return _nodeType == nodeType;
    }
}