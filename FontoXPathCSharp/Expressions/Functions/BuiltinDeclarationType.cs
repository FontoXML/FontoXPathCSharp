using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;

namespace FontoXPathCSharp.Expressions.Functions;

public class BuiltinDeclarationType<TNode> where TNode : notnull
{
    public readonly ParameterType[] ArgumentTypes;
    public readonly FunctionSignature<ISequence, TNode> CallFunction;
    public readonly string LocalName;
    public readonly string NamespaceUri;
    public readonly SequenceType ReturnType;

    public BuiltinDeclarationType(
        ParameterType[] argumentTypes, 
        FunctionSignature<ISequence, TNode> callFunction,
        string localName, 
        string namespaceUri, 
        SequenceType returnType)
    {
        ArgumentTypes = argumentTypes;
        CallFunction = callFunction;
        LocalName = localName;
        NamespaceUri = namespaceUri;
        ReturnType = returnType;
    }
}