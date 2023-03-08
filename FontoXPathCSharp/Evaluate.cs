using System.Globalization;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.EvaluationUtils;
using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Expressions.Util;
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
        Dictionary<string, object>? variables = null) where TNode : notnull where TSelector : notnull
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
        Dictionary<string, object>? variables = null) where TNode : notnull where TSelector : notnull
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
        Dictionary<string, object>? variables = null) where TNode : notnull where TSelector : notnull
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
        Dictionary<string, object>? variables = null) where TNode : notnull where TSelector : notnull
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
        Dictionary<string, object>? variables = null) where TNode : notnull where TSelector : notnull
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
        Dictionary<string, object>? variables = null) where TNode : notnull where TSelector : notnull
    {
        return EvaluateXPath<IEnumerable<TNode>, TSelector, TNode>(
            selector,
            ParameterUtils.ConvertToAbstractValue(contextItem),
            domFacade,
            options,
            variables
        ) ?? Array.Empty<TNode>();
    }

    public static long EvaluateXPathToInt<TSelector, TNode>(
        TSelector selector,
        TNode contextItem,
        IDomFacade<TNode> domFacade,
        Options<TNode> options,
        Dictionary<string, object>? variables = null) where TNode : notnull where TSelector : notnull
    {
        return EvaluateXPath<long, TSelector, TNode>(
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
        Dictionary<string, object>? variables = null) where TNode : notnull where TSelector : notnull
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
        Dictionary<string, object>? variables = null) where TNode : notnull where TSelector : notnull
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
        Dictionary<string, object>? variables = null) where TNode : notnull where TSelector : notnull
    {
        return EvaluateXPath<IEnumerable<int>, TSelector, TNode>(
            selector,
            ParameterUtils.ConvertToAbstractValue(contextItem),
            domFacade,
            options,
            variables
        ) ?? Array.Empty<int>();
    }

    public static string EvaluateXPathToString<TSelector, TNode>(
        TSelector selector,
        TNode contextItem,
        IDomFacade<TNode> domFacade,
        Options<TNode> options,
        Dictionary<string, object>? variables = null) where TNode : notnull where TSelector : notnull
    {
        return EvaluateXPath<string, TSelector, TNode>(
            selector,
            ParameterUtils.VerifyContextNode(contextItem, domFacade),
            domFacade,
            options,
            variables
        ) ?? "";
    }

    // ReSharper disable once UnusedMember.Local
    private static string EvaluateXPathToString<TSelector, TNode>(
        TSelector selector,
        object? contextItem,
        IDomFacade<TNode> domFacade,
        Options<TNode> options,
        Dictionary<string, object>? variables = null) where TNode : notnull where TSelector : notnull
    {
        return EvaluateXPath<string, TSelector, TNode>(
            selector,
            ParameterUtils.ConvertToAbstractValue(contextItem),
            domFacade,
            options,
            variables
        ) ?? "";
    }

    public static object? EvaluateXPathToAny<TSelector, TNode>(
        TSelector selector,
        TNode contextItem,
        IDomFacade<TNode> domFacade,
        Options<TNode> options,
        Dictionary<string, object>? variables = null) where TNode : notnull where TSelector : notnull
    {
        return EvaluateXPath<object, TSelector, TNode>(
            selector,
            ParameterUtils.VerifyContextNode(contextItem, domFacade),
            domFacade,
            options,
            variables
        );
    }

    public static object? EvaluateXPathToAny<TSelector, TNode>(
        TSelector selector,
        object? contextItem,
        IDomFacade<TNode> domFacade,
        Options<TNode> options,
        Dictionary<string, object>? variables = null) where TNode : notnull where TSelector : notnull
    {
        return EvaluateXPath<object, TSelector, TNode>(
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
        Dictionary<string, object>? variablesMap) where TNode : notnull where TSelector : notnull
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

        variablesMap ??= new Dictionary<string, object>();

        var variables = ParameterUtils.ConvertToAbstractValueVariables(variablesMap);

        DynamicContext? dynamicContext;
        ExecutionParameters<TNode> executionParameters;
        AbstractExpression<TNode>? expression;

        var context = new EvaluationContext<TSelector, TNode>(
            selector,
            contextItem,
            domFacade,
            variables,
            options,
            new CompilationOptions(
                options.LanguageId == Language.LanguageId.XqueryUpdate31Language,
                options.LanguageId is Language.LanguageId.Xquery31Language
                    or Language.LanguageId.XqueryUpdate31Language,
                options.Debug,
                options.DisableCache));
        dynamicContext = context.DynamicContext;
        executionParameters = context.ExecutionParameters;
        expression = context.Expression;


        if (expression.IsUpdating)
            throw new XPathException("XUST0001", "Updating expressions should be evaluated as updating expressions");


        if (typeof(TReturn) == typeof(bool) &&
            contextItem != null &&
            contextItem.GetValueType().IsSubtypeOf(ValueType.Node))
        {
            var selectorBucket = expression.GetBucket();
            var bucketsForNode = BucketUtils.GetBucketsForNode(contextItem.GetAs<NodeValue<TNode>>().Value, domFacade);
            if (selectorBucket != null && !bucketsForNode.Contains(selectorBucket))
                // We are sure that this selector will never match, without even running it
                return (TReturn)(object)false;
        }

        var rawResults = expression.EvaluateMaybeStatically(dynamicContext, executionParameters);

        var toReturn =
            XdmReturnValue<TSelector, TReturn, TNode>.ConvertXmdReturnValue(selector, rawResults, executionParameters);

        return toReturn;
    }
}