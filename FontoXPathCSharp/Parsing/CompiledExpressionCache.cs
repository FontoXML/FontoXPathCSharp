using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Parsing;

public record ExpressionCacheResult<TNode>(AbstractExpression<TNode> Expression, bool RequiresStaticCompilation);

public class CacheEntry<TNode>
{
    public readonly AbstractExpression<TNode> CompiledExpression;
    public readonly string DefaultFunctionNamespaceUri;
    public readonly List<(string, string)> ModuleImports;
    public readonly List<(string, string)> ReferredNamespaces;
    public readonly List<string> ReferredVariables;
    public readonly List<ResolvedFunction> ResolvedFunctions;

    public CacheEntry(
        List<(string, string)> referredNamespaces,
        List<string> referredVariables,
        AbstractExpression<TNode> compiledExpression,
        List<(string, string)> moduleImports,
        string defaultFunctionNamespaceUri,
        List<ResolvedFunction> resolvedFunctions)
    {
        ReferredNamespaces = referredNamespaces;
        ReferredVariables = referredVariables;
        CompiledExpression = compiledExpression;
        ModuleImports = moduleImports;
        DefaultFunctionNamespaceUri = defaultFunctionNamespaceUri;
        ResolvedFunctions = resolvedFunctions;
    }
}

public class CompiledExpressionCache<TSelector, TNode> where TSelector : notnull
{
    public static readonly CompiledExpressionCache<TSelector, TNode> Instance = new();

    private readonly Dictionary<TSelector, Dictionary<string, List<CacheEntry<TNode>>>>
        _compiledExpressionCache = new();

    private static string GenerateLanguageKey(string language, bool debug)
    {
        return language + (debug ? "_DEBUG" : "");
    }

    public void StoreStaticCompilationResultInCache(
        TSelector selectorExpression,
        string language,
        ExecutionSpecificStaticContext<TNode> executionStaticContext,
        Dictionary<string, string> moduleImports,
        AbstractExpression<TNode> compiledExpression,
        bool debug,
        string defaultFunctionNamespaceUri)
    {
        Dictionary<string, List<CacheEntry<TNode>>>? cachesForExpression;
        if (!_compiledExpressionCache.ContainsKey(selectorExpression))
        {
            cachesForExpression = new Dictionary<string, List<CacheEntry<TNode>>>();
            _compiledExpressionCache.Add(selectorExpression, cachesForExpression);
        }
        else
        {
            cachesForExpression = _compiledExpressionCache[selectorExpression];
        }

        var languageKey = GenerateLanguageKey(language, debug);
        List<CacheEntry<TNode>> cachesForLanguage;
        if (!cachesForExpression.ContainsKey(language))
        {
            var newEntry = new List<CacheEntry<TNode>>();
            cachesForExpression[languageKey] = newEntry;
            cachesForLanguage = newEntry;
        }
        else
        {
            cachesForLanguage = cachesForExpression[languageKey];
        }

        cachesForLanguage.Add(
            new CacheEntry<TNode>(
                executionStaticContext.GetReferredNamespaces(),
                executionStaticContext.GetReferredVariables(),
                compiledExpression,
                moduleImports.Keys.Select(moduleImportPrefix => (
                    moduleImports[moduleImportPrefix],
                    moduleImportPrefix
                )).ToList(),
                defaultFunctionNamespaceUri,
                executionStaticContext.GetResolvedFunctions()
            )
        );
    }

    public AbstractExpression<TNode>? GetAnyStaticCompilationResultFromCache(
        TSelector selectorExpression,
        string? language,
        bool debug)
    {
        if (!_compiledExpressionCache.ContainsKey(selectorExpression)) return null;

        var cachesForExpression = _compiledExpressionCache[selectorExpression];

        if (language == null)
            return cachesForExpression.Keys
                .Where(lang => cachesForExpression.ContainsKey(lang) && cachesForExpression[lang].Count > 0)
                .Select(lang => cachesForExpression[lang].First().CompiledExpression)
                .FirstOrDefault();

        var languageKey = !string.IsNullOrEmpty(language) ? GenerateLanguageKey(language, debug) : null;

        if (languageKey == null || !cachesForExpression.ContainsKey(languageKey)) return null;

        var cachesForLanguage = cachesForExpression[languageKey];
        return cachesForLanguage.Count != 0
            ? cachesForLanguage.First().CompiledExpression
            : null;
    }

    public ExpressionCacheResult<TNode>? GetStaticCompilationResultFromCache(
        TSelector selectorExpression,
        string language,
        NamespaceResolver namespaceResolver,
        Dictionary<string, AbstractValue> variables,
        Dictionary<string, string> moduleImports,
        bool debug,
        string defaultFunctionNamespaceUri,
        FunctionNameResolver functionNameResolver)
    {
        if (!_compiledExpressionCache.ContainsKey(selectorExpression)) return null;

        var cachesForExpression = _compiledExpressionCache[selectorExpression];

        var languageKey = GenerateLanguageKey(language, debug);
        if (!cachesForExpression.ContainsKey(languageKey)) return null;
        var cachesForLanguage = cachesForExpression[languageKey];

        var cacheWithCorrectContext = cachesForLanguage.Find(
            cache =>
                cache.DefaultFunctionNamespaceUri == defaultFunctionNamespaceUri
                && cache.ReferredNamespaces.All(nsRef => namespaceResolver(nsRef.Item2) == nsRef.Item1)
                && cache.ReferredVariables.All(variables.ContainsKey)
                && cache.ModuleImports.All(moduleImport => moduleImports[moduleImport.Item2] == moduleImport.Item1)
                && cache.ResolvedFunctions.All(resolvedFunction =>
                {
                    var newResolvedFunction = functionNameResolver(
                        resolvedFunction.LexicalQName,
                        resolvedFunction.Arity
                    );
                    return newResolvedFunction != null &&
                           newResolvedFunction.NamespaceUri ==
                           resolvedFunction.ResolvedQName.NamespaceUri &&
                           newResolvedFunction.LocalName == resolvedFunction.ResolvedQName.LocalName;
                })
        );

        return cacheWithCorrectContext != null
            ? new ExpressionCacheResult<TNode>(cacheWithCorrectContext.CompiledExpression, false)
            : null;
    }
}