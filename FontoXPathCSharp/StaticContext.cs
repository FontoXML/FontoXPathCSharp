using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;

namespace FontoXPathCSharp;

public struct FunctionProperties
{
    public readonly ParameterType[] ArgumentTypes;
    public readonly int Arity;
    public readonly FunctionSignature<ISequence> CallFunction;
    public readonly string LocalName;
    public readonly string NamespaceUri;
    public readonly SequenceType ReturnType;

    public FunctionProperties(ParameterType[] argumentTypes, int arity, FunctionSignature<ISequence> callFunction,
        string localName, string namespaceUri, SequenceType returnType)
    {
        ArgumentTypes = argumentTypes;
        Arity = arity;
        CallFunction = callFunction;
        LocalName = localName;
        NamespaceUri = namespaceUri;
        ReturnType = returnType;
    }
}

public class StaticContext
{
    private Dictionary<string, FunctionProperties> _registeredFunctionsByHash;

    public StaticContext()
    {
        _registeredFunctionsByHash = new Dictionary<string, FunctionProperties>();
    }

    public StaticContext Clone()
    {
        var staticContext = new StaticContext
        {
            _registeredFunctionsByHash = _registeredFunctionsByHash.ToDictionary(e => e.Key, e => e.Value)
        };
        return staticContext;
    }

    public FunctionProperties? LookupFunction(string? namespaceUri, string localName, int arity)
    {
        var hashKey = $"Q{{{namespaceUri ?? ""}}}{localName}";

        if (_registeredFunctionsByHash.TryGetValue(hashKey, out var foundFunction))
        {
            return foundFunction;
        }

        // TODO: look in parent context
        return null;
    }

    public void RegisterFunctionDefinition(FunctionProperties properties)
    {
        var hashKey = $"Q{{{properties.NamespaceUri ?? ""}}}{properties.LocalName}";

        if (_registeredFunctionsByHash.ContainsKey(hashKey))
        {
            throw new XPathException($"XQT0049 {properties.NamespaceUri} {properties.LocalName}");
        }

        _registeredFunctionsByHash[hashKey] = properties;
    }
}