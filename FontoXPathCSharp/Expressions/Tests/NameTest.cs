using System.Xml;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.Tests;

public class NameTest : AbstractTestExpression
{
    private readonly XmlNodeType? _kind;
    private readonly QName _name;

    public NameTest(QName name, XmlNodeType? kind = null)
    {
        _name = name;
        _kind = kind;
    }

    public override void PerformStaticEvaluation(StaticContext staticContext)
    {
        if (_name.NamespaceUri != null || _name.Prefix == "*") return;
        _name.NamespaceUri = staticContext.ResolveNamespace(_name.Prefix ?? "");

        if (_name.NamespaceUri == null && _name.Prefix != null)
            throw new Exception($"XPST0081: The prefix {_name.Prefix} could not be resolved");
    }

    public override string ToString()
    {
        return $"NameTest[ \"{_name}\" ]";
    }

    protected internal override bool EvaluateToBoolean(
        DynamicContext? _,
        AbstractValue value,
        ExecutionParameters? executionParameters)
    {
        var node = value.GetAs<NodeValue>()?.Value;
        var nodeIsElement = value.GetValueType().IsSubtypeOf(ValueType.Element);
        var nodeIsAttribute = value.GetValueType().IsSubtypeOf(ValueType.Attribute);

        if (node == null || (!nodeIsElement && !nodeIsAttribute)) return false;

        if (_kind != null &&
            ((_kind == XmlNodeType.Text && !nodeIsElement) ||
             (_kind == XmlNodeType.Attribute && !nodeIsAttribute))) return false;

        if (_name.Prefix == null && _name.NamespaceUri != "" && _name.LocalName == "*") return true;

        if (_name.Prefix == "*")
        {
            if (_name.LocalName == "*") return true;

            return _name.LocalName == node.LocalName;
        }

        if (_name.LocalName != "*" && _name.LocalName != node.LocalName) return false;

        var resolvedNamespaceUri = _name.Prefix == null
            ? nodeIsElement
                ? _name.NamespaceUri
                : null
            : _name.NamespaceUri;

        return (node.NamespaceURI == "" ? null : node.NamespaceURI) ==
               (resolvedNamespaceUri == "" ? null : resolvedNamespaceUri);
    }
}