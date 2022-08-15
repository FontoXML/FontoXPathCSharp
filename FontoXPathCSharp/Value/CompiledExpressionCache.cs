using FontoXPathCSharp.Expressions;
using NamespaceResolverFunc = System.Func<string, string?>;
using FunctionNameResolverFunc =
    System.Func<FontoXPathCSharp.Types.LexicalQualifiedName, int, FontoXPathCSharp.Types.ResolvedQualifiedName?>;

namespace FontoXPathCSharp.Value;

public record StaticCompilationResult<TNode>(StaticContext<TNode> StaticContext, AbstractExpression<TNode> Expression);

public record CachedExpression<TNode>(AbstractExpression<TNode> Expression, bool RequiresStaticCompilation);

public class CompiledExpressionCache<TSelector, TNode> where TSelector : notnull
{
    public static readonly CompiledExpressionCache<TSelector, TNode> Instance = new();

    private readonly Dictionary<TSelector, AbstractExpression<TNode>> _cache = new();

    public void StoreStaticCompilationResultInCache(
        TSelector selectorExpression,
        string language,
        ExecutionSpecificStaticContext<TNode> executionStaticContext,
        Dictionary<string, string> moduleImports,
        AbstractExpression<TNode> compiledExpression,
        bool debug,
        string defaultFunctionNamespaceUri)
    {
        //TODO: Correctly use the other parameters of this function
        _cache[selectorExpression] = compiledExpression;
    }

    public CachedExpression<TNode>? GetStaticCompilationResultFromCache(
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
        return _cache.ContainsKey(xpathSource) ? new CachedExpression<TNode>(_cache[xpathSource], false) : null;
    }
}