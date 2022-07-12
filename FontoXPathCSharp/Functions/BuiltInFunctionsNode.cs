using System.Xml;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Functions;

public static class BuiltInFunctionsNode
{
    public static readonly FunctionSignature<ISequence> FnNodeName = (_, _, _, args) =>
    {
        var firstArg = args[0];
        var pointerValue = firstArg.First();
        if (pointerValue == null) return SequenceFactory.CreateEmpty();

        // TODO: replace this with a node pointer
        var node = pointerValue.GetAs<NodeValue>()!;
        var nodeValue = node.Value;

        return nodeValue.NodeType switch
        {
            XmlNodeType.Element => SequenceFactory.CreateFromValue(new QNameValue(new QName(nodeValue.LocalName,
                nodeValue.NamespaceURI, nodeValue.Prefix))),
            XmlNodeType.Attribute => SequenceFactory.CreateFromValue(new QNameValue(new QName(nodeValue.LocalName,
                nodeValue.NamespaceURI, nodeValue.Prefix))),
            XmlNodeType.ProcessingInstruction => throw new NotImplementedException(
                "We need to get the target here somehow"),
            _ => SequenceFactory.CreateEmpty()
        };
    };

    public static readonly BuiltinDeclarationType[] Declarations =
    {
        new(new[] { new ParameterType(ValueType.Node, SequenceMultiplicity.ZeroOrOne) },
            FnNodeName, "node-name", BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsQName, SequenceMultiplicity.ZeroOrOne)),
        new(Array.Empty<ParameterType>(),
            BuiltInFunctions.ContextItemAsFirstArgument(FnNodeName), "node-name",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsQName, SequenceMultiplicity.ZeroOrOne))
    };
}