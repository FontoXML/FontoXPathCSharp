using System.Diagnostics;
using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Types;
using NamespaceResolverFunc = System.Func<string, string?>;
using FunctionNameResolverFunc =
    System.Func<FontoXPathCSharp.Types.LexicalQualifiedName, int, FontoXPathCSharp.Types.ResolvedQualifiedName?>;

namespace FontoXPathCSharp.Value;

public enum CacheState
{
    Parsed,
    Compiled,
    StaticAnalyzed
}

public class StaticCompilationResult
{
    public StaticCompilationResult(StaticContext staticContext, AbstractExpression expression)
    {
        StaticContext = staticContext;
        Expression = expression;
    }

    public StaticContext StaticContext { get; }
    public AbstractExpression Expression { get; }
}

public abstract class ExpressionResult
{
    protected CacheState _cacheState;

    public CacheState CacheState => _cacheState;
}

public class CompiledExpressionResult : ExpressionResult
{
    public CompiledExpressionResult(AbstractExpression expression)
    {
        _cacheState = CacheState.Compiled;
        Expression = expression;
    }

    public AbstractExpression Expression { get; }
}

public class StaticAnalyizedExpressionResult : ExpressionResult
{
    public StaticAnalyizedExpressionResult(AbstractExpression expression)
    {
        _cacheState = CacheState.StaticAnalyzed;
        Expression = expression;
    }

    public AbstractExpression Expression { get; }
}

public class ParsedExpressionResult : ExpressionResult
{
    public ParsedExpressionResult(Ast ast)
    {
        _cacheState = CacheState.Parsed;
        Ast = ast;
    }

    public Ast Ast { get; }
}

public class CachedExpressionResult : ExpressionResult
{
    public CachedExpressionResult(AbstractExpression expression, CacheState cacheState)
    {
        _cacheState = cacheState;
        Expression = expression;
    }

    public AbstractExpression Expression { get; }
}

public class CachedExpression
{
    public CachedExpression(AbstractExpression expression, bool requiresStaticCompilation)
    {
        Expression = expression;
        RequiresStaticCompilation = requiresStaticCompilation;
    }

    public AbstractExpression Expression { get; }

    public bool RequiresStaticCompilation { get; }
}

public class CompiledExpressionCache
{
    public static void StoreStaticCompilationResultInCache<TSelector>(
        TSelector selectorExpression, 
        string language,
        ExecutionSpecificStaticContext executionStaticContext,
        Dictionary<string, string> moduleImports,
        AbstractExpression compiledExpression,
        bool debug,
        string defaultFunctionNamespaceUri)
    {
        Console.WriteLine("Storing static compilation results not implemented yet");
    }

    public static CachedExpression GetStaticCompilationResultFromCache<TSelector>(
        TSelector xpathSource,
        string language,
        NamespaceResolverFunc namespaceResolver,
        Dictionary<string, IExternalValue> variables,
        Dictionary<string, string> moduleImports,
        bool compilationOptionsDebug,
        string defaultFunctionNamespaceUri,
        FunctionNameResolverFunc functionNameResolver)
    {
        throw new NotImplementedException("Fetching compilation results from cache not supported yet.");
    }
}