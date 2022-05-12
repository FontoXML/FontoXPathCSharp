using System.Xml;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Functions;

public static class BuiltInFunctionsNode
{
    private static readonly FunctionDefinitionType<ISequence> FnNodeName = (_, _, _, args) =>
    {
        var firstArg = args[0];
        var pointerValue = firstArg.First();
        if (pointerValue == null) return SequenceFactory.CreateEmpty();

        // TODO: replace this with a node pointer
        var node = pointerValue.GetAs<NodeValue>(ValueType.Node)!;
        var nodeValue = node.Value();
        switch (nodeValue.NodeType)
        {
            case XmlNodeType.Element:
            case XmlNodeType.Attribute:
                return SequenceFactory.CreateFromValue(new QNameValue(new QName(nodeValue.LocalName, nodeValue.NamespaceURI,
                    nodeValue.Prefix)));
            case XmlNodeType.ProcessingInstruction:
                throw new NotImplementedException("We need to get the target here somehow");
            default:
                return SequenceFactory.CreateEmpty();
        }
    };

    public static readonly BuiltinDeclarationType[] Declarations =
    {
        new(new[] {new ParameterType(ValueType.Node, SequenceMultiplicity.ZeroOrOne)},
            FnNodeName, "node-name", "http://www.w3.org/2005/xpath-functions",
            new SequenceType(ValueType.XsQName, SequenceMultiplicity.ZeroOrOne)),
        new(Array.Empty<ParameterType>(),
            BuiltInFunctions.ContextItemAsFirstArgument(FnNodeName), "node-name",
            "http://www.w3.org/2005/xpath-functions",
            new SequenceType(ValueType.XsQName, SequenceMultiplicity.ZeroOrOne))
    };
}