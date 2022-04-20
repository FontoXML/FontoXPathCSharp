using System.Net.NetworkInformation;
using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Types;
using NamespaceResolverFunc = System.Func<string, string?>;
using FunctionNameResolverFunc = System.Func<LexicalQualifiedName, int, ResolvedQualifiedName?>;

namespace FontoXPathCSharp;

public struct StaticCompilationResult
{
    AbstractExpression _expression;
    StaticContext _context;

    public StaticContext StaticContext => _context;
    public AbstractExpression Expression => _expression;
}
public static class CompileXPath<TSelector>
{
    public static StaticCompilationResult StaticallyCompileXPath(
        AbstractExpression expression,
        CompilationOptions compilationOptions,
        NamespaceResolverFunc namespaceResolver,
        Dictionary<string, IExternalValue> variables,
        Dictionary<string, string> moduleImports,
        string defaultFunctionNamespaceUri,
        FunctionNameResolverFunc functionNameResolver)
    {
        var specificStaticContext = new ExecutionSpecificStaticContext(namespaceResolver, variables, defaultFunctionNamespaceUri, functionNameResolver);
        var rootStaticContext = new StaticContext(specificStaticContext);

        foreach (var modulePrefix in moduleImports.Keys)
        {
            var moduleUri = moduleImports[modulePrefix];
            rootStaticContext.EnhanceWithModule(moduleUri);
            rootStaticContext.RegisterNamespace(modulePrefix, moduleUri);
        }

        throw new NotImplementedException("StaticallyCompileXPath not finished yet.");
    }
}
public class ResolvedFunction
{
    private int _arity;
    private LexicalQualifiedName _lexicalQName;
    private ResolvedQualifiedName _resolvedQName;
    public ResolvedFunction(LexicalQualifiedName lexicalQualifiedName, int arity, ResolvedQualifiedName resolvedQualifiedName)
    {
        _arity = arity;
        _lexicalQName = lexicalQualifiedName;
        _resolvedQName = resolvedQualifiedName;
    }

    public int Arity => _arity;
    public LexicalQualifiedName LexicalQName => _lexicalQName;
    public ResolvedQualifiedName ResolvedQName => _resolvedQName;
}
public class ExecutionSpecificStaticContext : AbstractContext
{
    private readonly NamespaceResolverFunc _namespaceResolver;
    private readonly FunctionNameResolverFunc _functionNameResolver;

    private readonly Dictionary<string, string> _variableBindingByName;
    private readonly Dictionary<string, string> _referredVariableByName;
    private readonly Dictionary<string, (string, string)> _referredNamespaceByName;

    private readonly List<ResolvedFunction> _resolvedFunctions;
    private bool _executionContextWasRequired;

    public ExecutionSpecificStaticContext(NamespaceResolverFunc namespaceResolver, Dictionary<string, IExternalValue> variableByName, string defaultFunctionNamespaceUri, FunctionNameResolverFunc functionNameResolver)
    {
        _namespaceResolver = namespaceResolver;
        _variableBindingByName = variableByName.Keys.Aggregate(new Dictionary<string, string>(), (bindings, variableName) =>
            {
                if (variableByName.ContainsKey(variableName)) bindings[variableName] = generateGlobalVariableBindingName(variableName);
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
    private string generateGlobalVariableBindingName(string variableName) => $"Q{{}}{variableName}[0]";

    public List<(string, string)> GetReferredNamespaces() => _referredNamespaceByName.Values.ToList();

    public List<string> GetReferredVariables() => _referredVariableByName.Values.ToList();

    public List<ResolvedFunction> GetResolvedFunctions() => _resolvedFunctions;

    override public FunctionProperties? LookupFunction(string namespaceUri, string localName, int arity, bool skipExternal)
    {
        throw new NotImplementedException("Function lookup not implemented yet.");
    }

    override public string? LookupVariable(string? namespaceUri, string localName)
    {
        _executionContextWasRequired = true;
        if (namespaceUri != null)
        {
            return null;
        }

        var bindingName = _variableBindingByName[localName];

        if (!_referredVariableByName.ContainsKey(localName))
        {
            _referredVariableByName.Add(localName, localName);
        }

        return bindingName;

    }

    override public ResolvedQualifiedName? ResolveFunctionName(LexicalQualifiedName lexicalQName, int arity)
    {
        var resolvedQName = _functionNameResolver(lexicalQName, arity);

        if (resolvedQName != null)
        {
            _resolvedFunctions.Add(new ResolvedFunction(lexicalQName, arity, resolvedQName));
        }
        else
        {
            var namespaceUri = ResolveNamespace(lexicalQName.Prefix);
            if (namespaceUri != null)
            {
                return new ResolvedQualifiedName(namespaceUri, lexicalQName.LocalName);
            }
        }
        return resolvedQName;
    }
    override public string? ResolveNamespace(string prefix, bool useExternalResolver = true)
    {
        if (!useExternalResolver) return null;

        var knownNamespaceUri = StaticallyKnownNamespaceUtils.GetStaticallyKnownNamespaceByPrefix(prefix);
        if (knownNamespaceUri != null)
        {
            return knownNamespaceUri;
        }
        _executionContextWasRequired = true;

        var uri = _namespaceResolver(prefix);

        if (!_referredNamespaceByName.ContainsKey(prefix) && uri != null)
        {
            _referredNamespaceByName.Add(prefix, (uri, prefix));
        }

        return uri;
    }
}
