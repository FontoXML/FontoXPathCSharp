using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.ExpressionResults;
using NamespaceResolverFunc = System.Func<string, string?>;
using FunctionNameResolverFunc =
    System.Func<FontoXPathCSharp.Types.LexicalQualifiedName, int, FontoXPathCSharp.Types.ResolvedQualifiedName?>;

namespace FontoXPathCSharp;

public static class CompileXPath
{
    private static ExpressionResult CreateExpressionFromSource<TSelector>(
        TSelector xpathSource,
        CompilationOptions compilationOptions,
        NamespaceResolverFunc namespaceResolver,
        Dictionary<string, AbstractValue> variables,
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
            return fromCache.RequiresStaticCompilation
                ? new CompiledExpressionResult(fromCache.Expression)
                : new StaticallyAnalyzedExpressionResult(fromCache.Expression);

        var ast =
            typeof(TSelector) == typeof(string)
                ? ParseExpression.ParseXPathOrXQueryExpression((string)(object)xpathSource!, compilationOptions)
                : XmlToAst.ConvertXmlToAst(xpathSource);

        return new ParsedExpressionResult(ast);
    }

    public static StaticCompilationResult StaticallyCompileXPath<TSelector>(
        TSelector selector,
        CompilationOptions compilationOptions,
        NamespaceResolverFunc namespaceResolver,
        Dictionary<string, AbstractValue> variables,
        Dictionary<string, string> moduleImports,
        string defaultFunctionNamespaceUri,
        FunctionNameResolverFunc functionNameResolver) where TSelector : notnull
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

        if (typeof(TSelector) == typeof(string))
            selector = (TSelector)(object)NormalizeEndOfLines((string)(object)selector);

        var result = CreateExpressionFromSource(
            selector,
            compilationOptions,
            namespaceResolver,
            variables,
            moduleImports,
            defaultFunctionNamespaceUri,
            functionNameResolver
        );


        switch (result.CacheState)
        {
            case CacheState.StaticAnalyzed:
                return new StaticCompilationResult(rootStaticContext,
                    ((StaticallyAnalyzedExpressionResult)result).Expression);
            case CacheState.Compiled:
            {
                var compiledResult = (CompiledExpressionResult)result;
                compiledResult.Expression.PerformStaticEvaluation(rootStaticContext);
                var language = compilationOptions.AllowXQuery ? "XQuery" : "XPath";

                CompiledExpressionCache.StoreStaticCompilationResultInCache(selector, language, specificStaticContext,
                    moduleImports, compiledResult.Expression, compilationOptions.Debug, defaultFunctionNamespaceUri);
                return new StaticCompilationResult(rootStaticContext, compiledResult.Expression);
            }

            case CacheState.Parsed:
            {
                var parsedResult = (ParsedExpressionResult)result;
                var expressionFromAst = BuildExpressionFromAst(
                    parsedResult.Ast,
                    compilationOptions,
                    rootStaticContext
                );
                expressionFromAst.PerformStaticEvaluation(rootStaticContext);

                if (!compilationOptions.DisableCache)
                {
                    var language = compilationOptions.AllowXQuery ? "XQuery" : "XPath";
                    CompiledExpressionCache.StoreStaticCompilationResultInCache(
                        selector,
                        language,
                        specificStaticContext,
                        moduleImports,
                        expressionFromAst,
                        compilationOptions.Debug,
                        defaultFunctionNamespaceUri
                    );
                }

                return new StaticCompilationResult(rootStaticContext, expressionFromAst);
            }
        }

        throw new NotImplementedException("StaticallyCompileXPath not finished yet.");
    }

    private static AbstractExpression BuildExpressionFromAst(
        Ast ast,
        CompilationOptions compilationOptions,
        StaticContext rootStaticContext)
    {
        //TODO: AST Annotation
        // AnnotateAst(ast, new AnnotationContext(rootStaticContext));

        var mainModule = ast.GetFirstChild(AstNodeName.MainModule);

        if (mainModule == null) throw new Exception("Can not execute a library module.");

        var queryBodyContents = mainModule.FollowPath(AstNodeName.QueryBody, AstNodeName.All);

        var prolog = mainModule.GetFirstChild(AstNodeName.Prolog);
        if (prolog != null)
            if (!compilationOptions.AllowXQuery)
                throw new Exception(
                    "XPST0003: Use of XQuery functionality is not allowed in XPath context"
                );
        // TODO: Implement prolog processing
        // processProlog(prolog, rootStaticContext);

        return CompileAstToExpression.CompileAst(queryBodyContents!, compilationOptions);
        // return compileAstToExpression(queryBodyContents, compilationOptions);
    }

    private static string NormalizeEndOfLines(string selector)
    {
        Console.WriteLine("Warning, end of line normalization regex might not be correct yet.");
        return selector.Replace("(\x0D\x0A)|(\x0D(?!\x0A))g", "" + 0xa);
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

// public class ExecutionSpecificStaticContext : AbstractContext
// {
//     private readonly FunctionNameResolverFunc _functionNameResolver;
//     private readonly NamespaceResolverFunc _namespaceResolver;
//     private readonly Dictionary<string, (string, string)> _referredNamespaceByName;
//     private readonly Dictionary<string, string> _referredVariableByName;
//
//     private readonly List<ResolvedFunction> _resolvedFunctions;
//
//     private readonly Dictionary<string, string> _variableBindingByName;
//     private bool _executionContextWasRequired;
//
//     public ExecutionSpecificStaticContext(NamespaceResolverFunc namespaceResolver,
//         Dictionary<string, IExternalValue> variableByName, string defaultFunctionNamespaceUri,
//         FunctionNameResolverFunc functionNameResolver)
//     {
//         _namespaceResolver = namespaceResolver;
//         _variableBindingByName = variableByName.Keys.Aggregate(new Dictionary<string, string>(),
//             (bindings, variableName) =>
//             {
//                 if (variableByName.ContainsKey(variableName))
//                     bindings[variableName] = GenerateGlobalVariableBindingName(variableName);
//                 return bindings;
//             }
//         );
//
//         _referredVariableByName = new Dictionary<string, string>();
//         _referredNamespaceByName = new Dictionary<string, (string, string)>();
//
//         registeredDefaultFunctionNamespaceURI = defaultFunctionNamespaceUri;
//
//         _functionNameResolver = functionNameResolver;
//         _resolvedFunctions = new List<ResolvedFunction>();
//
//         _executionContextWasRequired = false;
//     }
//
//     private static string GenerateGlobalVariableBindingName(string variableName)
//     {
//         return $"Q{{}}{variableName}[0]";
//     }
//
//     public List<(string, string)> GetReferredNamespaces()
//     {
//         return _referredNamespaceByName.Values.ToList();
//     }
//
//     public List<string> GetReferredVariables()
//     {
//         return _referredVariableByName.Values.ToList();
//     }
//
//     public List<ResolvedFunction> GetResolvedFunctions()
//     {
//         return _resolvedFunctions;
//     }
//
//     public override FunctionProperties? LookupFunction(string? namespaceUri, string localName, int arity,
//         bool skipExternal)
//     {
//         return FunctionRegistry.GetFunctionByArity(namespaceUri, localName, arity);
//     }
//
//     public override string? LookupVariable(string? namespaceUri, string localName)
//     {
//         _executionContextWasRequired = true;
//         if (namespaceUri != null) return null;
//
//         var bindingName = _variableBindingByName[localName];
//
//         if (!_referredVariableByName.ContainsKey(localName)) _referredVariableByName.Add(localName, localName);
//
//         return bindingName;
//     }
//
//     public override ResolvedQualifiedName? ResolveFunctionName(LexicalQualifiedName lexicalQName, int arity)
//     {
//         var resolvedQName = _functionNameResolver(lexicalQName, arity);
//
//         if (resolvedQName != null)
//         {
//             _resolvedFunctions.Add(new ResolvedFunction(lexicalQName, arity, resolvedQName));
//         }
//         else if (lexicalQName.Prefix == "")
//         {
//             if (registeredDefaultFunctionNamespaceURI != null)
//             {
//                 return new ResolvedQualifiedName(lexicalQName.LocalName, registeredDefaultFunctionNamespaceURI);
//             }
//         }
//         else
//         {
//             var namespaceUri = ResolveNamespace(lexicalQName.Prefix, true);
//             if (namespaceUri != null) return new ResolvedQualifiedName(lexicalQName.LocalName, namespaceUri);
//         }
//
//         return resolvedQName;
//     }
//
//     public override string? ResolveNamespace(string? prefix, bool useExternalResolver)
//     {
//         if (!useExternalResolver) return null;
//
//         var knownNamespaceUri = StaticallyKnownNamespaceUtils.GetStaticallyKnownNamespaceByPrefix(prefix);
//         if (knownNamespaceUri != null) return knownNamespaceUri;
//
//         _executionContextWasRequired = true;
//
//         var uri = _namespaceResolver(prefix);
//
//         if (!_referredNamespaceByName.ContainsKey(prefix) && uri != null)
//             _referredNamespaceByName.Add(prefix, (uri, prefix));
//
//         return uri;
//     }
//}