using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Sequences;

namespace FontoXPathCSharp;

public class StaticContext : AbstractContext
{
    private readonly AbstractContext? _parentContext;

    // TODO: this should include updating functions as well
    private Dictionary<string, FunctionProperties> _registeredFunctionsByHash;

    private readonly Dictionary<string, string>[] _registeredNamespaceUriByPrefix;
    private int _scopeCount;

    private readonly int _scopeDepth;

    public StaticContext(AbstractContext? parentContext)
    {
        _parentContext = parentContext;

        _scopeDepth = 0;
        _scopeCount = 0;

        _registeredNamespaceUriByPrefix = new[]
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
    }

    public StaticContext Clone()
    {
        var contextAtThisPoint = new StaticContext(_parentContext)
        {
            _registeredFunctionsByHash = _registeredFunctionsByHash.ToDictionary(e => e.Key, e => e.Value)
        };
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

        if (_registeredFunctionsByHash.TryGetValue(hashKey, out var foundFunction))
        {
            // TODO: add external support
            // if (!skipExternal && !foundFunction.IsExternal)
            {
                return foundFunction;
            }
        }


        return _parentContext?.LookupFunction(namespaceUri, localName, arity, skipExternal);
    }

    public override string? LookupVariable(string? namespaceUri, string localName)
    {
        throw new NotImplementedException("LookupVariable Not Yet Implemented for StaticContext");
    }

    public override ResolvedQualifiedName? ResolveFunctionName(LexicalQualifiedName lexicalQName, int arity)
    {
        if (lexicalQName.Prefix == "" && registeredDefaultFunctionNamespaceURI != null)
        {
            return new ResolvedQualifiedName(lexicalQName.LocalName, registeredDefaultFunctionNamespaceURI);
        }

        if (lexicalQName.Prefix != null)
        {
            var namespaceUri = ResolveNamespace(lexicalQName.Prefix, false);
            if (namespaceUri != null)
            {
                return new ResolvedQualifiedName(lexicalQName.LocalName, namespaceUri);
            }
        }

        return _parentContext?.ResolveFunctionName(lexicalQName, arity);
    }

    private static string? LookupInOverrides(IReadOnlyList<Dictionary<string, string>> overrides, string key)
    {
        for (var i = overrides.Count - 1; i >= 0; --i)
        {
            if (overrides[i].ContainsKey(key))
            {
                return overrides[i][key];
            }
        }

        return null;
    }

    public override string? ResolveNamespace(string? prefix, bool useExternalResolver)
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

    public void RegisterFunctionDefinition(FunctionProperties properties)
    {
        var hashKey = GetSignatureHash(properties.NamespaceUri, properties.LocalName, properties.Arity);

        if (_registeredFunctionsByHash.ContainsKey(hashKey))
            throw new XPathException($"XQT0049 {properties.NamespaceUri} {properties.LocalName}");

        _registeredFunctionsByHash[hashKey] = properties;
    }
}