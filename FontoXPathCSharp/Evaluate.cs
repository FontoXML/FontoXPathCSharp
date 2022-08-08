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
        IDomFacade<TNode>? domFacade = null,
        Dictionary<string, AbstractValue>? variables = null,
        Options<TNode>? options = null)
    {
        return EvaluateXPath<bool, TSelector, TNode>(selector, new NodeValue<TNode>((TNode)contextItem!, domFacade!),
            domFacade, variables, options);
    }


    public static TNode? EvaluateXPathToFirstNode<TSelector, TContextItem, TNode>(
        TSelector selector,
        TContextItem? contextItem,
        IDomFacade<TNode>? domFacade = null,
        Dictionary<string, AbstractValue>? variables = null,
        Options<TNode>? options = null)
    {
        AbstractValue? contextItemValue = null;

        if (typeof(TContextItem).IsAssignableFrom(typeof(TNode)))
        {
            if (contextItem != null && domFacade == null)
                throw new Exception("Cannot have a null domFacade when contextItem is an XML node.");
            contextItemValue = new NodeValue<TNode>((TNode)(object)contextItem!, domFacade!);
        }

        if (typeof(TContextItem).IsAssignableFrom(typeof(AbstractValue)))
            contextItemValue = (AbstractValue)(object)contextItem!;

        if (contextItem == null)
            throw new NotImplementedException(
                "EvaluateXPathToFirstNode: Cannot create a context item from an object that is neither an TNode nor an AbstractValue.");

        return EvaluateXPath<TNode?, TSelector, TNode>(
            selector,
            contextItemValue,
            domFacade,
            variables,
            options)!;
    }

    public static IEnumerable<TNode> EvaluateXPathToNodes<TSelector, TNode>(
        TSelector selector,
        TNode? contextItem,
        IDomFacade<TNode>? domFacade = null,
        Dictionary<string, AbstractValue>? variables = null,
        Options<TNode>? options = null) where TNode : notnull
    {
        AbstractValue? contextItemValue = null;

        if (contextItem != null && domFacade == null)
            throw new Exception("Cannot have a null domFacade when contextItem is an XML node.");
        contextItemValue = new NodeValue<TNode>((TNode)(object)contextItem!, domFacade!);

        return EvaluateXPath<IEnumerable<TNode>, TSelector, TNode>(
            selector,
            contextItemValue,
            domFacade,
            variables,
            options)!;
    }

    public static int EvaluateXPathToInt<TSelector, TNode>(
        TSelector selector,
        TNode contextItem,
        IDomFacade<TNode>? domFacade = null,
        Dictionary<string, AbstractValue>? variables = null,
        Options<TNode>? options = null)
    {
        return EvaluateXPath<int, TSelector, TNode>(selector, new NodeValue<TNode>((TNode)contextItem!, domFacade!),
            domFacade, variables, options);
    }

    public static IEnumerable<int> EvaluateXPathToInts<TSelector, TNode>(
        TSelector selector,
        AbstractValue? contextItem,
        IDomFacade<TNode>? domFacade = null,
        Dictionary<string, AbstractValue>? variables = null,
        Options<TNode>? options = null)
    {
        return EvaluateXPath<IEnumerable<int>, TSelector, TNode>(selector, contextItem, domFacade, variables,
            options)!;
    }

    public static string? EvaluateXPathToString<TSelector, TNode>(
        TSelector selector,
        TNode contextItem,
        IDomFacade<TNode> domFacade,
        Dictionary<string, AbstractValue>? variables = null,
        Options<TNode>? options = null)
    {
        if (domFacade == null) throw new ArgumentException("DomFacade cannot be null");
        return EvaluateXPathToString(selector,
            contextItem != null ? new NodeValue<TNode>(contextItem, domFacade) : null, domFacade, variables, options);
    }

    private static string? EvaluateXPathToString<TSelector, TNode>(
        TSelector selector,
        AbstractValue? contextItem,
        IDomFacade<TNode>? domFacade = null,
        Dictionary<string, AbstractValue>? variables = null,
        Options<TNode>? options = null)
    {
        return EvaluateXPath<string, TSelector, TNode>(selector, contextItem, domFacade, variables, options);
    }

    public static TReturn? EvaluateXPath<TReturn, TSelector, TNode>(
        TSelector selector,
        AbstractValue? contextItem,
        IDomFacade<TNode>? domFacade,
        Dictionary<string, AbstractValue>? variables,
        Options<TNode>? options)
    {
        variables ??= new Dictionary<string, AbstractValue>();
        options ??= new Options<TNode>(_ => null);

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
            Console.WriteLine("Error with selector: " + selector);
            throw;
        }


        if (expression.IsUpdating)
            throw new Exception(
                "XUST0001: Updating expressions should be evaluated as updating expressions"
            );


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