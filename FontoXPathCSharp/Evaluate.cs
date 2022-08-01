using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.EvaluationUtils;
using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp;

public class Evaluate
{
    public static bool EvaluateXPathToBoolean<TSelectorType, TNode>(
        TSelectorType selector,
        AbstractValue? contextItem,
        IDomFacade<TNode>? domFacade = null,
        Dictionary<string, AbstractValue>? variables = null,
        Options<TNode>? options = null)
    {
        return EvaluateXPath<bool, TSelectorType, TNode>(selector, contextItem, domFacade, variables, options);
    }


    public static TNode? EvaluateXPathToFirstNode<TSelectorType, TNode>(
        TSelectorType selector,
        AbstractValue? contextItem,
        IDomFacade<TNode>? domFacade = null,
        Dictionary<string, AbstractValue>? variables = null,
        Options<TNode>? options = null)
    {
        return EvaluateXPath<TNode, TSelectorType, TNode>(selector, contextItem, domFacade, variables, options);
    }


    public static IEnumerable<TNode> EvaluateXPathToNodes<TSelectorType, TNode>(
        TSelectorType selector,
        AbstractValue? contextItem,
        IDomFacade<TNode>? domFacade = null,
        Dictionary<string, AbstractValue>? variables = null,
        Options<TNode>? options = null)
    {
        return EvaluateXPath<IEnumerable<TNode>, TSelectorType, TNode>(selector, contextItem, domFacade, variables,
            options)!;
    }

    public static int EvaluateXPathToInt<TSelectorType, TNode>(
        TSelectorType selector,
        AbstractValue? contextItem,
        IDomFacade<TNode>? domFacade = null,
        Dictionary<string, AbstractValue>? variables = null,
        Options<TNode>? options = null)
    {
        return EvaluateXPath<int, TSelectorType, TNode>(selector, contextItem, domFacade, variables, options);
    }

    public static IEnumerable<int> EvaluateXPathToInts<TSelectorType, TNode>(
        TSelectorType selector,
        AbstractValue? contextItem,
        IDomFacade<TNode>? domFacade = null,
        Dictionary<string, AbstractValue>? variables = null,
        Options<TNode>? options = null)
    {
        return EvaluateXPath<IEnumerable<int>, TSelectorType, TNode>(selector, contextItem, domFacade, variables,
            options)!;
    }

    public static string? EvaluateXPathToString<TSelectorType, TNode>(
        TSelectorType selector,
        AbstractValue? contextItem,
        IDomFacade<TNode>? domFacade = null,
        Dictionary<string, AbstractValue>? variables = null,
        Options<TNode>? options = null)
    {
        return EvaluateXPath<string, TSelectorType, TNode>(selector, contextItem, domFacade, variables, options);
    }

    public static TReturn? EvaluateXPath<TReturn, TSelector, TNode>(
        TSelector selector,
        AbstractValue? contextItem,
        IDomFacade<TNode>? domFacade,
        Dictionary<string, AbstractValue>? variables,
        Options<TNode>? options)
    {
        variables ??= new Dictionary<string, AbstractValue>();
        options ??= new Options<TNode>();

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


        if (typeof(TReturn) == typeof(bool) && contextItem != null /* add check to see if nodeType is in contextItem*/)
        {
        }

        var rawResults = expression.EvaluateMaybeStatically(dynamicContext, executionParameters);

        var toReturn =
            XdmReturnValue<TSelector, TReturn, TNode>.ConvertXmdReturnValue(selector, rawResults, executionParameters);

        return toReturn;
    }
}