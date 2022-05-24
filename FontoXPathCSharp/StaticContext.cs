using FontoXPathCSharp.Expressions;
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

public class StaticContext : AbstractContext
{
    private readonly AbstractContext _parentContext;
    private Dictionary<string, FunctionProperties> _registeredFunctionsByHash;

    private readonly Dictionary<string, string>[] _registeredNamespaceURIByPrefix;
    private int _scopeCount;

    private readonly int _scopeDepth;

    public StaticContext(AbstractContext parentContext)
    {
        _parentContext = parentContext;

        _scopeDepth = 0;
        _scopeCount = 0;

        _registeredNamespaceURIByPrefix = new[]
        {
            new Dictionary<string, string>()
        };

        _registeredFunctionsByHash = new Dictionary<string, FunctionProperties>();

        registeredDefaultFunctionNamespaceURI = null;
        registeredVariableDeclarationByHashKey = parentContext.RegisteredVariableDeclarationByHashKey;
        registeredVariableBindingByHashKey = parentContext.RegisteredVariableBindingByHashKey;
    }

    public StaticContext()
    {
        throw new NotImplementedException();
    }

    public StaticContext Clone()
    {
        var contextAtThisPoint = new StaticContext(_parentContext);
        contextAtThisPoint._registeredFunctionsByHash =
            _registeredFunctionsByHash.ToDictionary(e => e.Key, e => e.Value);
        return contextAtThisPoint;
    }

    public override FunctionProperties? LookupFunction(string? namespaceUri, string localName, int arity,
        bool skipExternal)
    {
        var hashKey = $"Q{{{namespaceUri ?? ""}}}{localName}";

        if (_registeredFunctionsByHash.TryGetValue(hashKey, out var foundFunction)) return foundFunction;

        // TODO: look in parent context
        return null;
    }

    public override string? LookupVariable(string? namespaceUri, string localName)
    {
        throw new NotImplementedException("LookupVariable Not Yet Implemented for StaticContext");
    }

    public override ResolvedQualifiedName? ResolveFunctionName(LexicalQualifiedName lexicalQName, int arity)
    {
        throw new NotImplementedException("ResolveFunctionName Not Yet Implemented for StaticContext");
    }

    public override string? ResolveNamespace(string prefix, bool useExternalResolver)
    {
        throw new NotImplementedException("ResolveNamespace Not Yet Implemented for StaticContext");
    }

    public void RegisterNamespace(string prefix, string namespaceUri)
    {
        _registeredNamespaceURIByPrefix[_scopeDepth][prefix] = namespaceUri;
    }

    public void EnhanceWithModule(string uri)
    {
        throw new NotImplementedException("enhanceStaticContextWithModule not implemented yet.");
    }

    public void RegisterFunctionDefinition(FunctionProperties properties)
    {
        var hashKey = $"Q{{{properties.NamespaceUri ?? ""}}}{properties.LocalName}";

        if (_registeredFunctionsByHash.ContainsKey(hashKey))
            throw new XPathException($"XQT0049 {properties.NamespaceUri} {properties.LocalName}");

        _registeredFunctionsByHash[hashKey] = properties;
    }
}