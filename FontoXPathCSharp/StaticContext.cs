using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Functions;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Types;

namespace FontoXPathCSharp;

public class StaticContext : AbstractContext
{
    private readonly AbstractContext _parentContext;

    private readonly Dictionary<string, string>[] _registeredNamespaceURIByPrefix;

    private readonly int _scopeDepth;
    private Dictionary<string, FunctionProperties> _registeredFunctionsByHash;

    public StaticContext(AbstractContext parentContext)
    {
        _parentContext = parentContext;

        _scopeDepth = 0;

        _registeredNamespaceURIByPrefix = new[]
        {
            new Dictionary<string, string>()
        };

        _registeredFunctionsByHash = new Dictionary<string, FunctionProperties>();

        registeredDefaultFunctionNamespaceURI = null;

        // NOTE: not sure if these default values are correct   
        registeredVariableDeclarationByHashKey = parentContext?.RegisteredVariableDeclarationByHashKey ??
                                                 new Dictionary<string,
                                                     Func<DynamicContext, ExecutionParameters, ISequence>>();
        registeredVariableBindingByHashKey =
            parentContext?.RegisteredVariableBindingByHashKey ?? new Dictionary<string, string>();

        // TODO: this should not be done here but lets populate the static context with the default functions right here for now
        foreach (var function in BuiltInFunctions.Declarations)
        {
            FunctionRegistry.RegisterFunction(function.NamespaceUri, function.LocalName, function.ArgumentTypes,
                function.ReturnType, function.CallFunction);

            var functionProperties =
                FunctionRegistry.GetFunctionByArity(function.NamespaceUri, function.LocalName,
                    function.ArgumentTypes.Length);
            RegisterFunctionDefinition(functionProperties!);
        }
    }

    public override string ToString()
    {
        var nsUriString = "[ " + string.Join(", ", _registeredNamespaceURIByPrefix.Select(nsUri =>
            $"[{string.Join(", ", nsUri.Select(kvp => $"[{kvp.Key.ToString()}]: {kvp.Value.ToString()}"))}]")) + " ]";
        var funcString = $"[\n{string.Join(",\n", _registeredFunctionsByHash.Select(f => $"[{f.Key.ToString()}]"))} ]";


        return "Static Context: {\n" +
               $"Parent Context: {_parentContext}\n" +
               $"Registered Namespace URI By Prefix: {nsUriString}\n" +
               $"Scope Depth: {_scopeDepth}\n" +
               $"Registered Functions By Hash: {funcString}\n}}";
    }

    public StaticContext Clone()
    {
        var contextAtThisPoint = new StaticContext(_parentContext);
        contextAtThisPoint._registeredFunctionsByHash =
            _registeredFunctionsByHash.ToDictionary(e => e.Key, e => e.Value);
        return contextAtThisPoint;
    }

    private static string GetSignatureHash(string? namespaceUri, string localName, int arity)
    {
        return $"Q{{{namespaceUri ?? ""}}}{localName}~{arity}";
    }

    public override FunctionProperties? LookupFunction(string? namespaceUri, string localName, int arity,
        bool skipExternal)
    {
        var hashKey = GetSignatureHash(namespaceUri, localName, arity);

        // TODO: add external support
        // if (!skipExternal && !foundFunction.IsExternal)
        if (_registeredFunctionsByHash.TryGetValue(hashKey, out var foundFunction)) return foundFunction;


        return _parentContext?.LookupFunction(namespaceUri, localName, arity, skipExternal);
    }

    public override string? LookupVariable(string? namespaceUri, string localName)
    {
        throw new NotImplementedException("LookupVariable Not Yet Implemented for StaticContext");
    }

    public override ResolvedQualifiedName? ResolveFunctionName(LexicalQualifiedName lexicalQName, int arity)
    {
        if (lexicalQName.Prefix == "" && registeredDefaultFunctionNamespaceURI != null)
            return new ResolvedQualifiedName(lexicalQName.LocalName, registeredDefaultFunctionNamespaceURI);

        if (lexicalQName.Prefix != null)
        {
            var namespaceUri = ResolveNamespace(lexicalQName.Prefix, false);
            if (namespaceUri != null) return new ResolvedQualifiedName(lexicalQName.LocalName, namespaceUri);
        }

        return _parentContext?.ResolveFunctionName(lexicalQName, arity);
    }

    private static string? LookupInOverrides(IEnumerable<Dictionary<string, string>> overrides, string key)
    {
        return (from o in overrides.Reverse()
                where o.ContainsKey(key)
                select o[key])
            .FirstOrDefault();
    }

    public override string? ResolveNamespace(string? prefix, bool useExternalResolver)
    {
        var uri = LookupInOverrides(_registeredNamespaceURIByPrefix, prefix ?? "");
        return uri ?? _parentContext?.ResolveNamespace(prefix, useExternalResolver);
    }

    public void RegisterNamespace(string prefix, string namespaceUri)
    {
        _registeredNamespaceURIByPrefix[_scopeDepth][prefix] = namespaceUri;
    }

    public static void EnhanceWithModule(string uri)
    {
        throw new NotImplementedException("EnhanceStaticContextWithModule not implemented yet.");
    }

    public void RegisterFunctionDefinition(FunctionProperties properties)
    {
        var hashKey = GetSignatureHash(properties.NamespaceUri, properties.LocalName, properties.Arity);

        if (_registeredFunctionsByHash.ContainsKey(hashKey))
            throw new XPathException($"XQT0049 {properties.NamespaceUri} {properties.LocalName}");

        _registeredFunctionsByHash[hashKey] = properties;
    }
}