using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Types;

namespace FontoXPathCSharp;

public class ExecutionSpecificStaticContext : AbstractContext
{
    private readonly Func<LexicalQualifiedName, int, ResolvedQualifiedName?> _functionNameResolver;
    private readonly Func<string, string?> _namespaceResolver;
    private readonly Dictionary<string, (string, string)> _referredNamespaceByName;
    private readonly Dictionary<string, string> _referredVariableByName;

    private readonly List<ResolvedFunction> _resolvedFunctions;

    private readonly Dictionary<string, string> _variableBindingByName;
    private bool _executionContextWasRequired;

    public ExecutionSpecificStaticContext(Func<string, string?> namespaceResolver,
        Dictionary<string, IExternalValue> variableByName, string defaultFunctionNamespaceUri,
        Func<LexicalQualifiedName, int, ResolvedQualifiedName?> functionNameResolver)
    {
        _namespaceResolver = namespaceResolver;
        _variableBindingByName = variableByName.Keys.Aggregate(new Dictionary<string, string>(),
            (bindings, variableName) =>
            {
                if (variableByName.ContainsKey(variableName))
                    bindings[variableName] = GenerateGlobalVariableBindingName(variableName);
                return bindings;
            }
        );

        _referredVariableByName = new Dictionary<string, string>();
        _referredNamespaceByName = new Dictionary<string, (string, string)>();

        registeredDefaultFunctionNamespaceURI = defaultFunctionNamespaceUri;

        _functionNameResolver = functionNameResolver;
        _resolvedFunctions = new List<ResolvedFunction>();

        _executionContextWasRequired = false;
    }

    private string GenerateGlobalVariableBindingName(string variableName)
    {
        return $"Q{{}}{variableName}[0]";
    }

    public List<(string, string)> GetReferredNamespaces()
    {
        return _referredNamespaceByName.Values.ToList();
    }

    public List<string> GetReferredVariables()
    {
        return _referredVariableByName.Values.ToList();
    }

    public List<ResolvedFunction> GetResolvedFunctions()
    {
        return _resolvedFunctions;
    }

    public override FunctionProperties? LookupFunction(string namespaceUri, string localName, int arity,
        bool skipExternal)
    {
        throw new NotImplementedException("Function lookup not implemented yet.");
    }

    public override string? LookupVariable(string? namespaceUri, string localName)
    {
        _executionContextWasRequired = true;
        if (namespaceUri != null) return null;

        var bindingName = _variableBindingByName[localName];

        if (!_referredVariableByName.ContainsKey(localName)) _referredVariableByName.Add(localName, localName);

        return bindingName;
    }

    public override ResolvedQualifiedName? ResolveFunctionName(LexicalQualifiedName lexicalQName, int arity)
    {
        var resolvedQName = _functionNameResolver(lexicalQName, arity);

        if (resolvedQName != null)
        {
            _resolvedFunctions.Add(new ResolvedFunction(lexicalQName, arity, resolvedQName));
        }
        else
        {
            var namespaceUri = ResolveNamespace(lexicalQName.Prefix);
            if (namespaceUri != null) return new ResolvedQualifiedName(namespaceUri, lexicalQName.LocalName);
        }

        return resolvedQName;
    }

    public override string? ResolveNamespace(string prefix, bool useExternalResolver = true)
    {
        if (!useExternalResolver) return null;

        var knownNamespaceUri = StaticallyKnownNamespaceUtils.GetStaticallyKnownNamespaceByPrefix(prefix);
        if (knownNamespaceUri != null) return knownNamespaceUri;

        _executionContextWasRequired = true;

        var uri = _namespaceResolver(prefix);

        if (!_referredNamespaceByName.ContainsKey(prefix) && uri != null)
            _referredNamespaceByName.Add(prefix, (uri, prefix));

        return uri;
    }
}