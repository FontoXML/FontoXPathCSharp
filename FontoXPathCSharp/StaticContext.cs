using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Functions;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Types;

namespace FontoXPathCSharp;

public class StaticContext<TNode> : AbstractContext<TNode>
{
    private readonly AbstractContext<TNode> _parentContext;

    private readonly Dictionary<string, string>[] _registeredNamespaceUriByPrefix;

    private readonly int _scopeDepth;
    private Dictionary<string, FunctionProperties<TNode>> _registeredFunctionsByHash;

    public StaticContext(AbstractContext<TNode> parentContext)
    {
        _parentContext = parentContext;

        _scopeDepth = 0;

        _registeredNamespaceUriByPrefix = new[]
        {
            new Dictionary<string, string>()
        };

        _registeredFunctionsByHash = new Dictionary<string, FunctionProperties<TNode>>();

        registeredDefaultFunctionNamespaceURI = null;

        // NOTE: not sure if these default values are correct   
        registeredVariableDeclarationByHashKey = parentContext?.RegisteredVariableDeclarationByHashKey ??
                                                 new Dictionary<string,
                                                     Func<DynamicContext, ExecutionParameters<TNode>, ISequence>>();
        registeredVariableBindingByHashKey =
            parentContext?.RegisteredVariableBindingByHashKey ?? new Dictionary<string, string>();

        // TODO: this should not be done here but lets populate the static context with the default functions right here for now
        foreach (var function in BuiltInFunctions<TNode>.Declarations)
        {
            if (function.CallFunction == null)
                throw new Exception("The callback needs to be declared before the declaration itself.");

            FunctionRegistry<TNode>.RegisterFunction(function.NamespaceUri, function.LocalName, function.ArgumentTypes,
                function.ReturnType, function.CallFunction);

            var functionProperties =
                new FunctionProperties<TNode>(function.ArgumentTypes, function.ArgumentTypes.Length,
                    function.CallFunction,
                    false, function.LocalName, function.NamespaceUri, function.ReturnType);
            RegisterFunctionDefinition(functionProperties!);
        }
    }

    public override string ToString()
    {
        var nsUriString = "[ " + string.Join(", ", _registeredNamespaceUriByPrefix.Select(nsUri =>
            $"[{string.Join(", ", nsUri.Select(kvp => $"[{kvp.Key.ToString()}]: {kvp.Value.ToString()}"))}]")) + " ]";
        var funcString = $"[\n{string.Join(",\n", _registeredFunctionsByHash.Select(f => $"[{f.Key.ToString()}]"))} ]";


        return "Static Context: {\n" +
               $"Parent Context: {_parentContext}\n" +
               $"Registered Namespace URI By Prefix: {nsUriString}\n" +
               $"Scope Depth: {_scopeDepth}\n" +
               $"Registered Functions By Hash: {funcString}\n}}";
    }

    public StaticContext<TNode> Clone()
    {
        var contextAtThisPoint = new StaticContext<TNode>(_parentContext);
        contextAtThisPoint._registeredFunctionsByHash =
            _registeredFunctionsByHash.ToDictionary(e => e.Key, e => e.Value);
        return contextAtThisPoint;
    }

    private static string GetSignatureHash(string? namespaceUri, string localName, int arity)
    {
        return $"Q{{{namespaceUri ?? string.Empty}}}{localName}~{arity}";
    }

    public override FunctionProperties<TNode>? LookupFunction(string? namespaceUri, string localName, int arity,
        bool skipExternal = false)
    {
        var hashKey = GetSignatureHash(namespaceUri, localName, arity);

        if (_registeredFunctionsByHash.ContainsKey(hashKey))
        {
            var foundFunction = _registeredFunctionsByHash[hashKey];
            if (!skipExternal || !foundFunction.IsExternal) return foundFunction;
        }

        return _parentContext.LookupFunction(namespaceUri, localName, arity, skipExternal);
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
        return overrides
            .Reverse()
            .Where(o => o.ContainsKey(key))
            .Select(o => o[key])
            .FirstOrDefault();
    }

    public override string? ResolveNamespace(string? prefix, bool useExternalResolver = true)
    {
        var uri = LookupInOverrides(_registeredNamespaceUriByPrefix, prefix ?? "");
        return uri ?? _parentContext?.ResolveNamespace(prefix, useExternalResolver);
    }

    public void RegisterNamespace(string prefix, string namespaceUri)
    {
        _registeredNamespaceUriByPrefix[_scopeDepth][prefix] = namespaceUri;
    }

    public static void EnhanceWithModule(string uri)
    {
        throw new NotImplementedException("EnhanceStaticContextWithModule not implemented yet.");
    }

    public void RegisterFunctionDefinition(FunctionProperties<TNode> properties)
    {
        var hashKey = GetSignatureHash(properties.NamespaceUri, properties.LocalName, properties.Arity);

        if (_registeredFunctionsByHash.ContainsKey(hashKey))
            throw new XPathException($"XQT0049 {properties.NamespaceUri} {properties.LocalName}");

        _registeredFunctionsByHash[hashKey] = properties;
    }
}