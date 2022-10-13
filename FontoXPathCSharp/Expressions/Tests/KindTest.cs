using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.Tests;

public class KindTest<TNode> : AbstractTestExpression<TNode> where TNode : notnull
{
    private readonly NodeType _nodeType;

    public KindTest(NodeType nodeType) : base(
        new Specificity(new Dictionary<SpecificityKind, int> { { SpecificityKind.NodeType, 1 } }))
    {
        _nodeType = nodeType;
    }

    protected internal override bool EvaluateToBoolean(DynamicContext? dynamicContext, AbstractValue value,
        ExecutionParameters<TNode> executionParameters)
    {
        var domFacade = executionParameters.DomFacade;
        if (!value.GetValueType().IsSubtypeOf(ValueType.Node))
            return false;

        var node = value.GetAs<NodeValue<TNode>>().Value;

        if (_nodeType == NodeType.Text && domFacade.IsCharacterData(node))
            // CDATA_SECTION_NODES should be regarded as text nodes, and CDATA does not exist in the XPath Data Model
            return true;

        return _nodeType == domFacade.GetNodeType(node);
    }

    public override string GetBucket()
    {
        return BucketConstants.TypePrefix + BucketUtils.GetBucketTypeId(_nodeType);
    }
}