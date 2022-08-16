using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.EvaluationUtils;
using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp;

public class Evaluate
{
    public static bool EvaluateXPathToBoolean<TSelector, TNode>(
        TSelector selector,
        TNode contextItem,
        IDomFacade<TNode> domFacade,
        Options<TNode> options,
        Dictionary<string, object>? variables = null) where TNode : notnull
    {
        return EvaluateXPath<bool, TSelector, TNode>(
            selector,
            ParameterUtils.VerifyContextNode(contextItem, domFacade),
            domFacade,
            options,
            variables
        );
    }

    public static bool EvaluateXPathToBoolean<TSelector, TNode>(
        TSelector selector,
        object contextItem,
        IDomFacade<TNode> domFacade,
        Options<TNode> options,
        Dictionary<string, object>? variables = null) where TNode : notnull
    {
        return EvaluateXPath<bool, TSelector, TNode>(
            selector,
            ParameterUtils.ConvertToAbstractValue(contextItem),
            domFacade,
            options,
            variables
        );
    }


    public static TNode? EvaluateXPathToFirstNode<TSelector, TNode>(
        TSelector selector,
        TNode contextItem,
        IDomFacade<TNode> domFacade,
        Options<TNode> options,
        Dictionary<string, object>? variables = null) where TNode : notnull
    {
        return EvaluateXPath<TNode?, TSelector, TNode>(
            selector,
            ParameterUtils.VerifyContextNode(contextItem, domFacade),
            domFacade,
            options,
            variables
        );
    }

    public static TNode? EvaluateXPathToFirstNode<TSelector, TNode>(
        TSelector selector,
        object contextItem,
        IDomFacade<TNode> domFacade,
        Options<TNode> options,
        Dictionary<string, object>? variables = null) where TNode : notnull
    {
        return EvaluateXPath<TNode?, TSelector, TNode>(
            selector,
            ParameterUtils.ConvertToAbstractValue(contextItem),
            domFacade,
            options,
            variables
        );
    }

    public static IEnumerable<TNode> EvaluateXPathToNodes<TSelector, TNode>(
        TSelector selector,
        TNode contextItem,
        IDomFacade<TNode> domFacade,
        Options<TNode> options,
        Dictionary<string, object>? variables = null) where TNode : notnull
    {
        return EvaluateXPath<IEnumerable<TNode>, TSelector, TNode>(
            selector,
            ParameterUtils.VerifyContextNode(contextItem, domFacade),
            domFacade,
            options,
            variables
        ) ?? Array.Empty<TNode>();
    }

    public static IEnumerable<TNode> EvaluateXPathToNodes<TSelector, TNode>(
        TSelector selector,
        object? contextItem,
        IDomFacade<TNode> domFacade,
        Options<TNode> options,
        Dictionary<string, object>? variables = null) where TNode : notnull
    {
        return EvaluateXPath<IEnumerable<TNode>, TSelector, TNode>(
            selector,
            ParameterUtils.ConvertToAbstractValue(contextItem),
            domFacade,
            options,
            variables
        ) ?? Array.Empty<TNode>();
    }

    public static int EvaluateXPathToInt<TSelector, TNode>(
        TSelector selector,
        TNode contextItem,
        IDomFacade<TNode> domFacade,
        Options<TNode> options,
        Dictionary<string, object>? variables = null) where TNode : notnull
    {
        return EvaluateXPath<int, TSelector, TNode>(
            selector,
            ParameterUtils.VerifyContextNode(contextItem, domFacade),
            domFacade,
            options,
            variables
        );
    }

    public static int EvaluateXPathToInt<TSelector, TNode>(
        TSelector selector,
        object? contextItem,
        IDomFacade<TNode> domFacade,
        Options<TNode> options,
        Dictionary<string, object>? variables = null) where TNode : notnull
    {
        return EvaluateXPath<int, TSelector, TNode>(
            selector,
            ParameterUtils.ConvertToAbstractValue(contextItem),
            domFacade,
            options,
            variables
        );
    }

    public static IEnumerable<int> EvaluateXPathToInts<TSelector, TNode>(
        TSelector selector,
        TNode contextItem,
        IDomFacade<TNode> domFacade,
        Options<TNode> options,
        Dictionary<string, object>? variables = null) where TNode : notnull
    {
        return EvaluateXPath<IEnumerable<int>, TSelector, TNode>(
            selector,
            ParameterUtils.VerifyContextNode(contextItem, domFacade),
            domFacade,
            options,
            variables
        ) ?? Array.Empty<int>();
    }

    public static IEnumerable<int> EvaluateXPathToInts<TSelector, TNode>(
        TSelector selector,
        object? contextItem,
        IDomFacade<TNode> domFacade,
        Options<TNode> options,
        Dictionary<string, object>? variables = null) where TNode : notnull
    {
        return EvaluateXPath<IEnumerable<int>, TSelector, TNode>(
            selector,
            ParameterUtils.ConvertToAbstractValue(contextItem),
            domFacade,
            options,
            variables
        ) ?? Array.Empty<int>();
    }

    public static string? EvaluateXPathToString<TSelector, TNode>(
        TSelector selector,
        TNode contextItem,
        IDomFacade<TNode> domFacade,
        Options<TNode> options,
        Dictionary<string, object>? variables = null) where TNode : notnull
    {
        return EvaluateXPath<string, TSelector, TNode>(
            selector,
            ParameterUtils.VerifyContextNode(contextItem, domFacade),
            domFacade,
            options,
            variables
        );
    }

    private static string? EvaluateXPathToString<TSelector, TNode>(
        TSelector selector,
        object? contextItem,
        IDomFacade<TNode> domFacade,
        Options<TNode> options,
        Dictionary<string, object>? variables = null)
    {
        return EvaluateXPath<string, TSelector, TNode>(
            selector,
            ParameterUtils.ConvertToAbstractValue(contextItem),
            domFacade,
            options,
            variables
        );
    }

    public static TReturn? EvaluateXPath<TReturn, TSelector, TNode>(
        TSelector selector,
        AbstractValue? contextItem,
        IDomFacade<TNode> domFacade,
        Options<TNode> options,
        Dictionary<string, object>? variablesMap)
    {
        variablesMap ??= new Dictionary<string, object>();

        var variables = ParameterUtils.ConvertToAbstractValueVariables(variablesMap);

        DynamicContext? dynamicContext;
        ExecutionParameters<TNode> executionParameters;
        AbstractExpression<TNode>? expression;

        try
        {
            var context = new EvaluationContext<TSelector, TNode>(
                selector,
                contextItem,
                domFacade,
                variables,
                options,
                new CompilationOptions(
                    options.LanguageId == Language.LanguageId.XQUERY_UPDATE_3_1_LANGUAGE,
                    options.LanguageId is Language.LanguageId.XQUERY_3_1_LANGUAGE
                        or Language.LanguageId.XQUERY_UPDATE_3_1_LANGUAGE,
                    options.Debug,
                    options.DisableCache));
            dynamicContext = context.DynamicContext;
            executionParameters = context.ExecutionParameters;
            expression = context.Expression;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error with selector: {selector}\nInfo:{ex.Message}");
            throw;
        }


        if (expression.IsUpdating)
            throw new XPathException("XUST0001", "Updating expressions should be evaluated as updating expressions");


        if (typeof(TReturn) == typeof(bool) && contextItem != null &&
            contextItem.GetValueType().IsSubtypeOf(ValueType.Node))
        {
            //TODO: Bucket stuff
        }

        var rawResults = expression.EvaluateMaybeStatically(dynamicContext, executionParameters);

        var toReturn =
            XdmReturnValue<TSelector, TReturn, TNode>.ConvertXmdReturnValue(selector, rawResults, executionParameters);

        return toReturn;
    }
}