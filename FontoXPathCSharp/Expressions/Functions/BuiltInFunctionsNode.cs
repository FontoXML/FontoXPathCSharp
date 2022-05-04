using System.Xml;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.Functions;

public static class BuiltInFunctionsNode
{
    private static readonly FunctionSignature<ISequence> FnNodeName = (context, parameters, staticContext, args) =>
    {
        var firstArg = args[0];
        var pointerValue = firstArg.First();
        if (pointerValue == null)
        {
            return new EmptySequence();
        }

        // TODO: replace this with a node pointer
        var node = pointerValue.GetAs<NodeValue>(ValueType.Node)!;
        var nodeValue = node.Value();
        switch (nodeValue.NodeType)
        {
            case XmlNodeType.Element:
            case XmlNodeType.Attribute:
                return new SingletonSequence(new QNameValue(new QName(nodeValue.LocalName, nodeValue.NamespaceURI,
                    nodeValue.Prefix)));
            case XmlNodeType.ProcessingInstruction:
                throw new NotImplementedException("We need to get the target here somehow");
            default:
                return new EmptySequence();
        }
    };

    public static readonly BuiltinDeclarationType[] Declarations =
    {
        new(new[] {new ParameterType(ValueType.Node, SequenceMultiplicity.ZeroOrOne)},
            callFunction: FnNodeName, localName: "node-name", namespaceUri: "http://www.w3.org/2005/xpath-functions",
            returnType: new SequenceType(ValueType.XsQName, SequenceMultiplicity.ZeroOrOne)),
        new(Array.Empty<ParameterType>(),
            callFunction: BuiltInFunctions.ContextItemAsFirstArgument(FnNodeName), localName: "node-name",
            namespaceUri: "http://www.w3.org/2005/xpath-functions",
            returnType: new SequenceType(ValueType.XsQName, SequenceMultiplicity.ZeroOrOne)),
    };
}