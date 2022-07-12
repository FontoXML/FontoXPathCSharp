using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;

namespace FontoXPathCSharp.Functions;

public class BuiltinDeclarationType
{
    public readonly ParameterType[] ArgumentTypes;
    public readonly FunctionSignature<ISequence> CallFunction;
    public readonly string LocalName;
    public readonly string NamespaceUri;
    public readonly SequenceType ReturnType;

    public BuiltinDeclarationType(ParameterType[] argumentTypes, FunctionSignature<ISequence> callFunction,
        string localName, string namespaceUri, SequenceType returnType)
    {
        ArgumentTypes = argumentTypes;
        CallFunction = callFunction;
        LocalName = localName;
        NamespaceUri = namespaceUri;
        ReturnType = returnType;
    }
}