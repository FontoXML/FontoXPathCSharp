using System.Xml;
using FontoXPathCSharp.Types.Node;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions;

public class NameTest : AbstractTestExpression
{
    private readonly int? _kind;
    private readonly QName _name;

    public NameTest(QName name, int? kind = null)
    {
        _name = name;
        _kind = kind;
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
        // TODO: This stuff does not work yet for some reason.
        var domFacade = executionParameters.DomFacade;
        
        var node = value.GetAs<NodeValue>(ValueType.Node)?.Value;
        var nodeIsElement = SubtypeUtils.IsSubtypeOf(value.GetValueType(), ValueType.Element);
        var nodeIsAttribute = SubtypeUtils.IsSubtypeOf(value.GetValueType(), ValueType.Attribute);

        if (node == null || (!nodeIsElement && !nodeIsAttribute))
        {
            return false;
        }

        // var node = value.GetAs<ElementValue>(ValueType.Element)?.Value ?? value.GetAs<AttributeValue>(ValueType.Attribute)?.Value;
        //
        // Console.WriteLine("Value: " + value.GetValueType());
        // Console.WriteLine("Node: " + node);
        //
        
        // This becomes necessary when we implement ElementTest
        if (_kind != null && ((_kind == 1 && !nodeIsElement) || (_kind == 2 && !nodeIsAttribute))) {
            return false;
        }

        if (_name.Prefix == null && _name.NamespaceUri != "" && _name.LocalName == "*") return true;

        if (_name.Prefix == "*")
        {
            if (_name.LocalName == "*") return true;

            return _name.LocalName == node.LocalName;
        }

        if (_name.LocalName != "*" && _name.LocalName != node.LocalName) return false;

        var resolvedNamespaceUri = _name.Prefix == "" 
            ? nodeIsElement 
                ? _name.NamespaceUri 
                : null
            : _name.NamespaceUri;
        
        return (node.NamespaceURI == "" ? null : node.NamespaceURI) == resolvedNamespaceUri;
    }
}