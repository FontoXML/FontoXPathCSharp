using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;

namespace FontoXPathCSharp;

public class FunctionProperties<TNode> where TNode : notnull
{
    public FunctionProperties(ParameterType[] argumentTypes,
        int arity,
        FunctionSignature<ISequence, TNode> callFunction,
        bool isUpdating,
        string localName,
        string namespaceUri,
        SequenceType returnType,
        bool isExternal = false)
    {
        ArgumentTypes = argumentTypes;
        Arity = arity;
        CallFunction = callFunction ?? throw new Exception("FUNCTIONPROPERTIES: CALL FUNCTION IS NULL");
        IsUpdating = isUpdating;
        LocalName = localName;
        NamespaceUri = namespaceUri;
        ReturnType = returnType;
        IsExternal = isExternal;
    }

    public ParameterType[] ArgumentTypes { get; }
    public int Arity { get; }
    public FunctionSignature<ISequence, TNode> CallFunction { get; }
    public bool IsUpdating { get; }
    public string LocalName { get; }
    public string NamespaceUri { get; }
    public SequenceType ReturnType { get; }
    public bool IsExternal { get; }
}