using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Expressions.Functions;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Types;

namespace FontoXPathCSharp;

public class StaticContext<TNode> : AbstractContext<TNode> where TNode : notnull
{
    private readonly AbstractContext<TNode>? _parentContext;
    private readonly List<Dictionary<string, string>> _registeredNamespaceUriByPrefix;

    private Dictionary<string, FunctionProperties<TNode>> _registeredFunctionsByHash;
    
    private int _scopeCount;

    private int _scopeDepth;


    public StaticContext(AbstractContext<TNode>? parentContext)
    {
        _parentContext = parentContext;

        _scopeDepth = 0;
        _scopeCount = 0;

        _registeredNamespaceUriByPrefix = new List<Dictionary<string, string>>();

        _registeredFunctionsByHash = new Dictionary<string, FunctionProperties<TNode>>();
        RegisteredDefaultFunctionNamespaceUri = null;

        RegisteredVariableDeclarationByHashKey =
            parentContext != null
                ? parentContext.RegisteredVariableDeclarationByHashKey
                : new Dictionary<string, Func<DynamicContext, ExecutionParameters<TNode>, ISequence>>();
        RegisteredVariableBindingByHashKey =
            parentContext != null
                ? parentContext.RegisteredVariableBindingByHashKey
                : new List<Dictionary<string, string>>();

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
            RegisterFunctionDefinition(functionProperties);
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

    private static string CreateHashKey(string? namespaceUri, string localName)
    {
        return $"Q{{{namespaceUri ?? string.Empty}}}{localName}";
    }

    private static string CreateSignatureHash(string? namespaceUri, string localName, int arity)
    {
        return CreateHashKey(namespaceUri, localName) + "~" + arity;
    }

    public override FunctionProperties<TNode>? LookupFunction(string? namespaceUri, string localName, int arity,
        bool skipExternal = false)
    {
        var hashKey = CreateSignatureHash(namespaceUri, localName, arity);

        if (_registeredFunctionsByHash.ContainsKey(hashKey))
        {
            var foundFunction = _registeredFunctionsByHash[hashKey];
            if (!skipExternal || !foundFunction.IsExternal) return foundFunction;
        }

        return _parentContext!.LookupFunction(namespaceUri, localName, arity, skipExternal);
    }

    public override string? LookupVariable(string? namespaceUri, string localName)
    {
        var hash = CreateHashKey(namespaceUri, localName);
        var varNameInCurrentScope = LookupInOverrides(
            RegisteredVariableBindingByHashKey!,
            hash
        );
        if (varNameInCurrentScope != null) return varNameInCurrentScope;
        return _parentContext?.LookupVariable(namespaceUri, localName);
    }

    public override ResolvedQualifiedName? ResolveFunctionName(LexicalQualifiedName lexicalQName, int arity)
    {
        if (lexicalQName.Prefix == "" && RegisteredDefaultFunctionNamespaceUri != null)
            return new ResolvedQualifiedName(lexicalQName.LocalName, RegisteredDefaultFunctionNamespaceUri);

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
        return uri ?? _parentContext?.ResolveNamespace(prefix ?? "", useExternalResolver);
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
        var hashKey = CreateSignatureHash(properties.NamespaceUri, properties.LocalName, properties.Arity);

        if (_registeredFunctionsByHash.ContainsKey(hashKey))
            throw new XPathException("XQT0049", $"{properties.NamespaceUri} {properties.LocalName}");

        _registeredFunctionsByHash[hashKey] = properties;
    }

    public void IntroduceScope()
    {
        _scopeCount++;
        _scopeDepth++;

        _registeredNamespaceUriByPrefix.Add(new Dictionary<string, string>());
        RegisteredVariableBindingByHashKey!.Add(new Dictionary<string, string>());
    }

    public void RemoveScope()
    {
        _registeredNamespaceUriByPrefix.RemoveRange(
            _scopeDepth,
            _registeredNamespaceUriByPrefix.Count - _scopeDepth
        );
        RegisteredVariableBindingByHashKey!.RemoveRange(
            _scopeDepth,
            RegisteredVariableBindingByHashKey.Count - _scopeDepth
        );

        _scopeDepth--;
    }

    public string RegisterVariable(string? namespaceUri, string localName)
    {
        var hash = CreateHashKey(namespaceUri ?? "", localName);
        var registration = $"{hash}[{_scopeCount}]";
        RegisteredVariableBindingByHashKey![_scopeDepth][hash] = registration;
        return registration;
    }

    public IEnumerable<string> GetVariableBindings()
    {
        return RegisteredVariableDeclarationByHashKey!.Keys;
    }

    public Func<DynamicContext, ExecutionParameters<TNode>, ISequence>? GetVariableDeclaration(string hashKey)
    {
        return RegisteredVariableDeclarationByHashKey!.ContainsKey(hashKey)
            ? RegisteredVariableDeclarationByHashKey[hashKey]
            : null;
    }
}