using FontoXPathCSharp.Expressions;
using NamespaceResolverFunc = System.Func<string, string?>;
using FunctionNameResolverFunc =
    System.Func<FontoXPathCSharp.Types.LexicalQualifiedName, int, FontoXPathCSharp.Types.ResolvedQualifiedName?>;

namespace FontoXPathCSharp.Value;

public record StaticCompilationResult(StaticContext StaticContext, AbstractExpression Expression);

public record CachedExpression(AbstractExpression Expression, bool RequiresStaticCompilation);

public class CompiledExpressionCache<TSelector> where TSelector : notnull
{
    public static readonly CompiledExpressionCache<TSelector> Instance = new();

    private readonly Dictionary<TSelector, AbstractExpression> _cache = new();

    public void StoreStaticCompilationResultInCache(
        TSelector selectorExpression,
        string language,
        ExecutionSpecificStaticContext executionStaticContext,
        Dictionary<string, string> moduleImports,
        AbstractExpression compiledExpression,
        bool debug,
        string defaultFunctionNamespaceUri)
    {
        //TODO: Correctly use the other parameters of this function
        _cache[selectorExpression] = compiledExpression;
    }

    public CachedExpression? GetStaticCompilationResultFromCache(
        TSelector xpathSource,
        string language,
        NamespaceResolverFunc namespaceResolver,
        Dictionary<string, AbstractValue> variables,
        Dictionary<string, string> moduleImports,
        bool compilationOptionsDebug,
        string defaultFunctionNamespaceUri,
        FunctionNameResolverFunc functionNameResolver)
    {
        //TODO: Correctly use the other parameters of this function
        return _cache.ContainsKey(xpathSource) ? new CachedExpression(_cache[xpathSource], false) : null;
    }
}