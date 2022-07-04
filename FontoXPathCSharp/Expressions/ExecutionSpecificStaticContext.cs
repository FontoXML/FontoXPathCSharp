using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Functions;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp;

public class ExecutionSpecificStaticContext : AbstractContext
{
    private readonly Func<LexicalQualifiedName, int, ResolvedQualifiedName?> _functionNameResolver;
    private readonly Func<string, string?> _namespaceResolver;
    private readonly Dictionary<string, (string, string)> _referredNamespaceByName;
    private readonly Dictionary<string, string> _referredVariableByName;

    private readonly List<ResolvedFunction> _resolvedFunctions;

    private readonly Dictionary<string, string> _variableBindingByName;

    public ExecutionSpecificStaticContext(Func<string, string?> namespaceResolver,
        Dictionary<string, AbstractValue> variableByName, string defaultFunctionNamespaceUri,
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
    }

    public override string ToString()
    {
        var refNsString =
            $"[ {string.Join(", ", _referredNamespaceByName.Select(ns => $"{{[{ns.Key}]:{ns.Value}}}"))} ]";
        var refVarString = $"[ {string.Join(", ", _referredVariableByName.Select(v => $"{{[{v.Key}]:{v.Value}}}"))} ]";
        var resFuncString = $"[ {string.Join(", ", _resolvedFunctions.Select(f => f.ToString()))} ]";
        var varBindString = $"[ {string.Join(", ", _variableBindingByName.Select(v => $"{{[{v.Key}]:{v.Value}}}"))} ]";

        return "Execution Specific Static Context: {\n" +
               $"Function Name Resolver: {_functionNameResolver.Method}\n" +
               $"Namespace Resolver: {_namespaceResolver.Method}\n" +
               $"Referred Namespaces By Name: {refNsString}\n" +
               $"Referred Variable By Name: {refVarString}\n" +
               $"Resolved Functions: {resFuncString}\n" +
               $"Variable Bindings By Name: {varBindString}\n}}";
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

    public override FunctionProperties? LookupFunction(string? namespaceUri, string localName, int arity,
        bool _skipExternal)
    {
        // NOTE: `namespaceUri != null` was added to get rid of nullable warning
        return namespaceUri != null ? FunctionRegistry.GetFunctionByArity(namespaceUri, localName, arity) : null;
    }

    public override string? LookupVariable(string? namespaceUri, string localName)
    {
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
        else if (lexicalQName.Prefix == "")
        {
            if (registeredDefaultFunctionNamespaceURI != null)
                return new ResolvedQualifiedName(lexicalQName.LocalName, registeredDefaultFunctionNamespaceURI);
        }
        else
        {
            // NOTE: `lexicalQName == null` was added to get rid of nullable warning
            if (lexicalQName.Prefix == null) return null;
            var namespaceUri = ResolveNamespace(lexicalQName.Prefix, true);
            if (namespaceUri != null) return new ResolvedQualifiedName(lexicalQName.LocalName, namespaceUri);
        }

        return resolvedQName;
    }

    public override string? ResolveNamespace(string prefix, bool useExternalResolver)
    {
        if (!useExternalResolver) return null;

        var knownNamespaceUri = StaticallyKnownNamespaceUtils.GetStaticallyKnownNamespaceByPrefix(prefix);
        if (knownNamespaceUri != null) return knownNamespaceUri;

        var uri = _namespaceResolver(prefix);

        if (!_referredNamespaceByName.ContainsKey(prefix) && uri != null)
            _referredNamespaceByName.Add(prefix, (uri, prefix));

        return uri;
    }
}