using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Expressions.Util;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.Tests;

public class NameTest<TNode> : AbstractTestExpression<TNode> where TNode : notnull
{
    private readonly NodeType? _kind;
    private readonly QName _name;

    public NameTest(QName name, NodeType? kind = null) : base(CreateSpecificity(name))
    {
        _name = name;
        _kind = kind;
    }

    private static Specificity CreateSpecificity(QName name)
    {
        var specificityMap = new Dictionary<SpecificityKind, int>();
        if (name.LocalName != "*") specificityMap.Add(SpecificityKind.NodeName, 1);
        specificityMap.Add(SpecificityKind.NodeType, 1);

        return new Specificity(specificityMap);
    }

    public override void PerformStaticEvaluation(StaticContext<TNode> staticContext)
    {
        if (_name.NamespaceUri != null || _name.Prefix == "*") return;
        _name.NamespaceUri = staticContext.ResolveNamespace(_name.Prefix ?? "");

        if (_name.NamespaceUri == "" && _name.Prefix != "")
            throw new XPathException("XPST0081", $"The prefix {_name.Prefix} could not be resolved");
    }

    public override string ToString()
    {
        return $"NameTest[ \"{_name}\" ]";
    }

    protected internal override bool EvaluateToBoolean(DynamicContext? _,
        AbstractValue value,
        ExecutionParameters<TNode>? executionParameters)
    {
        var domFacade = executionParameters!.DomFacade;
        var node = value.GetAs<NodeValue<TNode>>().Value;
        var nodeIsElement = value.GetValueType().IsSubtypeOf(ValueType.Element);
        var nodeIsAttribute = value.GetValueType().IsSubtypeOf(ValueType.Attribute);

        if (!nodeIsElement && !nodeIsAttribute) return false;

        if (_kind != null &&
            ((_kind == NodeType.Text && !nodeIsElement) ||
             (_kind == NodeType.Attribute && !nodeIsAttribute))) return false;

        if (_name.Prefix == null && _name.NamespaceUri != "" && _name.LocalName == "*") return true;

        if (_name.Prefix == "*")
        {
            if (_name.LocalName == "*") return true;

            return _name.LocalName == domFacade.GetLocalName(node);
        }

        if (_name.LocalName != "*" && _name.LocalName != domFacade.GetLocalName(node)) return false;

        var resolvedNamespaceUri = string.IsNullOrEmpty(_name.Prefix)
            ? nodeIsElement
                ? _name.NamespaceUri
                : null
            : _name.NamespaceUri;

        return (domFacade.GetNamespaceUri(node) == "" ? null : domFacade.GetNamespaceUri(node)) ==
               (resolvedNamespaceUri == "" ? null : resolvedNamespaceUri);
    }

    public override string GetBucket()
    {
        if (_name.LocalName == "*")
        {
            if (_kind == null) return BucketConstants.Type1OrType2;
            return BucketConstants.TypePrefix + BucketUtils.GetBucketTypeId((NodeType)_kind);
        }

        return BucketConstants.NamePrefix + _name.LocalName;
    }
}