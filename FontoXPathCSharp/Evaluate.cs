using System.Xml;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.EvaluationUtils;
using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp;

public class Evaluate
{
    public static bool EvaluateXPathToBoolean<TSelectorType>(TSelectorType selector, XmlNode? contextItem,
        IDomFacade? domFacade = null, Dictionary<string, AbstractValue>? variables = null, Options? options = null)
    {
        return EvaluateXPath<bool, TSelectorType>(selector, contextItem, domFacade, variables, options);
    }

    
    public static XmlNode EvaluateXPathToFirstNode<TSelectorType>(TSelectorType selector, XmlNode? contextItem,
        IDomFacade? domFacade = null, Dictionary<string, AbstractValue>? variables = null, Options? options = null)
    {
        return EvaluateXPath<XmlNode, TSelectorType>(selector, contextItem, domFacade, variables, options);
    }
    
    
    public static IEnumerable<XmlNode> EvaluateXPathToNodes<TSelectorType>(TSelectorType selector, XmlNode? contextItem,
        IDomFacade? domFacade = null, Dictionary<string, AbstractValue>? variables = null, Options? options = null)
    {
        return EvaluateXPath<IEnumerable<XmlNode>, TSelectorType>(selector, contextItem, domFacade, variables, options);
    }

    public static int EvaluateXPathToInt<TSelectorType>(TSelectorType selector, XmlNode? contextItem,
        IDomFacade? domFacade = null, Dictionary<string, AbstractValue>? variables = null, Options? options = null)
    {
        return EvaluateXPath<int, TSelectorType>(selector, contextItem, domFacade, variables, options);
    }

    public static IEnumerable<int> EvaluateXPathToInts<TSelectorType>(TSelectorType selector, XmlNode? contextItem,
        IDomFacade? domFacade = null, Dictionary<string, AbstractValue>? variables = null, Options? options = null)
    {
        return EvaluateXPath<IEnumerable<int>, TSelectorType>(selector, contextItem, domFacade, variables, options);
    }

    public static string EvaluateXPathToString<TSelectorType>(TSelectorType selector, XmlNode? contextItem,
        IDomFacade? domFacade = null, Dictionary<string, AbstractValue>? variables = null, Options? options = null)
    {
        return EvaluateXPath<string, TSelectorType>(selector, contextItem, domFacade, variables, options);
    }

    public static TReturn EvaluateXPath<TReturn, TSelector>(
        TSelector selector,
        XmlNode? contextItem,
        IDomFacade? domFacade,
        Dictionary<string, AbstractValue>? variables,
        Options? options)
    {
        variables ??= new Dictionary<string, AbstractValue>();
        options ??= new Options();

        DynamicContext? dynamicContext;
        ExecutionParameters? executionParameters;
        AbstractExpression? expression;

        try
        {
            var context = new EvaluationContext<TSelector>(
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
        catch (Exception)
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
            XdmReturnValue.ConvertXmdReturnValue<TSelector, TReturn>(selector, rawResults, executionParameters);

        return toReturn;
    }
}