using System.Diagnostics;
using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Functions;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;
using NamespaceResolverFunc = System.Func<string, string?>;
using FunctionNameResolverFunc = System.Func<LexicalQualifiedName, int, ResolvedQualifiedName?>;

namespace FontoXPathCSharp;

public static class CompileXPath
{
    public static StaticCompilationResult StaticallyCompileXPath<TSelector>(
        TSelector selector,
        CompilationOptions compilationOptions,
        NamespaceResolverFunc namespaceResolver,
        Dictionary<string, IExternalValue> variables,
        Dictionary<string, string> moduleImports,
        string defaultFunctionNamespaceUri,
        FunctionNameResolverFunc functionNameResolver)
    {
        var specificStaticContext = new ExecutionSpecificStaticContext(namespaceResolver, variables,
            defaultFunctionNamespaceUri, functionNameResolver);
        var rootStaticContext = new StaticContext(specificStaticContext);

        foreach (var modulePrefix in moduleImports.Keys)
        {
            var moduleUri = moduleImports[modulePrefix];
            StaticContext.EnhanceWithModule(moduleUri);
            rootStaticContext.RegisterNamespace(modulePrefix, moduleUri);
        }

        if (typeof(TSelector) == typeof(string)) selector = NormalizeEndOfLines(selector);

        var result = CreateExpressionFromSource(
            selector,
            compilationOptions,
            namespaceResolver,
            variables,
            moduleImports,
            defaultFunctionNamespaceUri,
            functionNameResolver
        );

        throw new NotImplementedException("StaticallyCompileXPath not finished yet.");
    }

    private static ExpressionResult CreateExpressionFromSource<TSelector>(
        TSelector xpathSource,
        CompilationOptions compilationOptions,
        NamespaceResolverFunc namespaceResolver,
        Dictionary<string, IExternalValue> variables,
        Dictionary<string, string> moduleImports,
        string defaultFunctionNamespaceUri,
        FunctionNameResolverFunc functionNameResolver)
    {
        var language = compilationOptions.AllowXQuery ? "XQuery" : "XPath";

        var fromCache = compilationOptions.DisableCache
            ? null
            : CompiledExpressionCache.GetStaticCompilationResultFromCache(
                xpathSource,
                language,
                namespaceResolver,
                variables,
                moduleImports,
                compilationOptions.Debug,
                defaultFunctionNamespaceUri,
                functionNameResolver);

        if (fromCache != null)
            return new CachedExpressionResult(fromCache.Expression,
                fromCache.RequiresStaticCompilation ? CacheState.Compiled : CacheState.StaticAnalyzed);

        var ast =
            typeof(TSelector) == typeof(string)
                ? ParseExpression.ParseXPathOrXQueryExpression(xpathSource, compilationOptions)
                : XmlToAst.ConvertXmlToAst(xpathSource);

        return new ParsedExpressionResult(ast);
    }

    private static TSelector NormalizeEndOfLines<TSelector>(TSelector selector)
    {
        var selectorString = (string) (object) selector!;
        Debug.WriteLine("Warning, end of line normalization regex might not be correct yet.");
        return (TSelector) (object) selectorString.Replace("(\x0D\x0A)|(\x0D(?!\x0A))g", "" + 0xa);
    }
}

public class ResolvedFunction
{
    public ResolvedFunction(LexicalQualifiedName lexicalQualifiedName, int arity,
        ResolvedQualifiedName resolvedQualifiedName)
    {
        Arity = arity;
        LexicalQName = lexicalQualifiedName;
        ResolvedQName = resolvedQualifiedName;
    }

    public int Arity { get; }

    public LexicalQualifiedName LexicalQName { get; }

    public ResolvedQualifiedName ResolvedQName { get; }
}

public class ExecutionSpecificStaticContext : AbstractContext
{
    private readonly FunctionNameResolverFunc _functionNameResolver;
    private readonly NamespaceResolverFunc _namespaceResolver;
    private readonly Dictionary<string, (string, string)> _referredNamespaceByName;
    private readonly Dictionary<string, string> _referredVariableByName;

    private readonly List<ResolvedFunction> _resolvedFunctions;

    private readonly Dictionary<string, string> _variableBindingByName;
    private bool _executionContextWasRequired;

    public ExecutionSpecificStaticContext(NamespaceResolverFunc namespaceResolver,
        Dictionary<string, IExternalValue> variableByName, string defaultFunctionNamespaceUri,
        FunctionNameResolverFunc functionNameResolver)
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

    private static string GenerateGlobalVariableBindingName(string variableName)
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
        bool skipExternal)
    {
        return FunctionRegistry.GetFunctionByArity(namespaceUri, localName, arity);
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
            var namespaceUri = ResolveNamespace(lexicalQName.Prefix, true);
            if (namespaceUri != null) return new ResolvedQualifiedName(namespaceUri, lexicalQName.LocalName);
        }

        return resolvedQName;
    }

    public override string? ResolveNamespace(string? prefix, bool useExternalResolver)
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