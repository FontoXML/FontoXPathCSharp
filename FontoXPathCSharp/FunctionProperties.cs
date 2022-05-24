using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;

namespace FontoXPathCSharp;

public class FunctionProperties
{
    public readonly ParameterType[] ArgumentTypes;
    public readonly int Arity;
    public readonly FunctionDefinitionType<ISequence> CallFunction;
    public readonly bool IsExternal;
    public readonly bool IsUpdating;
    public readonly string LocalName;
    public readonly string NamespaceUri;
    public readonly SequenceType ReturnType;

    public FunctionProperties(ParameterType[] argumentTypes, int arity, FunctionDefinitionType<ISequence> callFunction,
        string localName, string namespaceUri, SequenceType returnType)
    {
        ArgumentTypes = argumentTypes;
        Arity = arity;
        CallFunction = callFunction;
        IsExternal = false;
        IsUpdating = false;
        LocalName = localName;
        NamespaceUri = namespaceUri;
        ReturnType = returnType;
    }
}