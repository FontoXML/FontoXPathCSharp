using FontoXPathCSharp.Types.Node;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions;

public class NameTest : AbstractTestExpression
{
    private readonly QName _name;

    public NameTest(QName name)
    {
        _name = name;
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
        // var domFacade = executionParameters.DomFacade;
        // var nodeIsElement = SubtypeUtils.IsSubtypeOf(value.GetValueType(), ValueType.Element);
        // var nodeIsAttribute = SubtypeUtils.IsSubtypeOf(value.GetValueType(), ValueType.Attribute);
        //
        // Console.WriteLine($"Node is: {value.GetValueType()}, Node is element: {nodeIsElement}, Node is attribute: {nodeIsAttribute}");
        //
        // if (!nodeIsElement && !nodeIsAttribute) {
        //     return false;
        // }

        var node = value.GetAs<NodeValue>(ValueType.Node)?.Value;
        
        // var node = value.GetAs<ElementValue>(ValueType.Element)?.Value ?? value.GetAs<AttributeValue>(ValueType.Attribute)?.Value;
        //
        // Console.WriteLine("Value: " + value.GetValueType());
        // Console.WriteLine("Node: " + node);
        //
        if (_name.Prefix == null && _name.NamespaceUri != "" && _name.LocalName == "*") return true;

        if (_name.Prefix == "*")
        {
            if (_name.LocalName == "*") return true;

            return _name.LocalName == node?.LocalName;
        }

        if (_name.LocalName == "*")
            return true;

        if (_name.LocalName != node?.LocalName)
            return false;

        return true;
    }
}