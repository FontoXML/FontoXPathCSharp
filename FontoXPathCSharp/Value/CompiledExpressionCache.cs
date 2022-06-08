using System.Diagnostics;
using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Types;
using NamespaceResolverFunc = System.Func<string, string?>;
using FunctionNameResolverFunc =
    System.Func<FontoXPathCSharp.Types.LexicalQualifiedName, int, FontoXPathCSharp.Types.ResolvedQualifiedName?>;

namespace FontoXPathCSharp.Value;

public record StaticCompilationResult(StaticContext StaticContext, AbstractExpression Expression);

public record CachedExpression(AbstractExpression Expression, bool RequiresStaticCompilation);

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
        //TODO: Storing static compilation results in cache
        Console.WriteLine("Storing static compilation results in cache not implemented yet");
    }

    public static CachedExpression? GetStaticCompilationResultFromCache<TSelector>(
        TSelector xpathSource,
        string language,
        NamespaceResolverFunc namespaceResolver,
        Dictionary<string, IExternalValue> variables,
        Dictionary<string, string> moduleImports,
        bool compilationOptionsDebug,
        string defaultFunctionNamespaceUri,
        FunctionNameResolverFunc functionNameResolver)
    {
        //TODO: Fetching compilation results from cache
        Console.WriteLine("Fetching compilation results from cache not supported yet.");
        return null;
    }
}