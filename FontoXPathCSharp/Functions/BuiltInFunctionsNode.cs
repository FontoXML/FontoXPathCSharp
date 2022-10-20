using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Functions;

public static class BuiltInFunctionsNode<TNode> where TNode : notnull
{
    public static readonly FunctionSignature<ISequence, TNode> FnNodeName = (_, executionParameters, _, args) =>
    {
        var firstArg = args[0];
        var pointerValue = firstArg.First();
        if (pointerValue == null) return SequenceFactory.CreateEmpty();

        var domFacade = executionParameters.DomFacade;

        // TODO: replace this with a node pointer
        var node = pointerValue.GetAs<NodeValue<TNode>>();
        var nodeValue = node.Value;

        return node.GetValueType() switch
        {
            ValueType.Element => SequenceFactory.CreateFromValue(new QNameValue(new QName(
                domFacade.GetLocalName(nodeValue),
                domFacade.GetNamespaceUri(nodeValue), domFacade.GetPrefix(nodeValue)))),
            ValueType.Attribute => SequenceFactory.CreateFromValue(new QNameValue(new QName(
                domFacade.GetLocalName(nodeValue),
                domFacade.GetNamespaceUri(nodeValue), domFacade.GetPrefix(nodeValue)))),
            ValueType.ProcessingInstruction => throw new NotImplementedException(
                "We need to get the target here somehow"),
            _ => SequenceFactory.CreateEmpty()
        };
    };

    public static readonly BuiltinDeclarationType<TNode>[] Declarations =
    {
        new(new[] { new ParameterType(ValueType.Node, SequenceMultiplicity.ZeroOrOne) },
            FnNodeName, "node-name", BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsQName, SequenceMultiplicity.ZeroOrOne)),
        new(Array.Empty<ParameterType>(),
            BuiltInFunctions<TNode>.ContextItemAsFirstArgument(FnNodeName), "node-name",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsQName, SequenceMultiplicity.ZeroOrOne))
    };
}