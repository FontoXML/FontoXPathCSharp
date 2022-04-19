using System.Xml;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.EvaluationUtils;
using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Types.Node;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp;

public class Evaluate
{
    public static bool EvaluateXPathToBoolean<TSelectorType>(TSelectorType selector, IExternalValue? contextItem,
        IDomFacade? domFacade, Dictionary<string, IExternalValue> variables, Options? options)
    {
        return EvaluateXPath<bool, bool, TSelectorType>(selector, contextItem, domFacade, variables, options);
    }

    public static TReturn EvaluateXPath<TNode, TReturn, TSelector>(
        TSelector selector,
        IExternalValue? contextItem,
        IDomFacade? domFacade,
        Dictionary<string, IExternalValue>? variables,
        Options? options)
    {
        options ??= new Options();

        DynamicContext? dynamicContext = null;
        ExecutionParameters? executionParameters = null;
        AbstractExpression? expression = null;

        try
        {
            var context = new EvaluationContext<TSelector>(
	            selector,
                contextItem,
	            domFacade,
	            variables,
	            options,
	            new CompilationOptions(
		            options.LanguageId == Language.LanguageID.XQUERY_UPDATE_3_1_LANGUAGE, 
		            options.LanguageId is Language.LanguageID.XQUERY_3_1_LANGUAGE or Language.LanguageID.XQUERY_UPDATE_3_1_LANGUAGE,
		            options.Debug,
		            options.DisableCache));
            dynamicContext = context.DynamicContext;
            executionParameters = context.ExecutionParameters;
            expression = context.Expression;
        }
        catch(Exception ex)
        {
	        Console.WriteLine("Error with selector: " + selector);
            throw;
        }

        if (expression.IsUpdating)
        {
	        throw new Exception(
		        "XUST0001: Updating expressions should be evaluated as updating expressions"
	        );
        }


        if (typeof(TReturn) == typeof(bool) && contextItem != null /* add check to see if nodeType is in contextItem*/)
        {
	        
        } 

        throw new NotImplementedException();
    }
}

/**
const evaluateXPath = <TNode extends Node, TReturnType extends keyof IReturnTypes<TNode>>(
	selector: EvaluableExpression,
	contextItem?: any | null,
	domFacade?: IDomFacade | null,
	variables?: {
		[s: string]: TypedExternalValue | UntypedExternalValue;
	} | null,
	returnType?: TReturnType,
	options?: Options | null
): IReturnTypes<TNode>[TReturnType] => {
	returnType = returnType || (ReturnType.ANY as any);
	if (!selector || (typeof selector !== 'string' && !('nodeType' in selector))) {
		throw new TypeError(
			"Failed to execute 'evaluateXPath': xpathExpression must be a string or an element depicting an XQueryX DOM tree."
		);
	}

	options = options || {};

	let dynamicContext: DynamicContext;
	let executionParameters: ExecutionParameters;
	let expression: Expression;
	try {
		const context = buildEvaluationContext(
			selector,
			contextItem,
			domFacade || null,
			variables || {},
			options,
			{
				allowUpdating: options['language'] === Language.XQUERY_UPDATE_3_1_LANGUAGE,
				allowXQuery:
					options['language'] === Language.XQUERY_3_1_LANGUAGE ||
					options['language'] === Language.XQUERY_UPDATE_3_1_LANGUAGE,
				debug: !!options['debug'],
				disableCache: !!options['disableCache'],
				annotateAst: !!options['annotateAst'],
			}
		);
		dynamicContext = context.dynamicContext;
		executionParameters = context.executionParameters;
		expression = context.expression;
	} catch (error) {
		printAndRethrowError(selector, error);
	}

	if (expression.isUpdating) {
		throw new Error(
			'XUST0001: Updating expressions should be evaluated as updating expressions'
		);
	}

	// Shortcut: if the xpathExpression defines buckets, the
	// contextItem is a node and we are evaluating to a bucket, we can
	// use it to return false if we are sure it won't match.
	if (returnType === ReturnType.BOOLEAN && contextItem && 'nodeType' in contextItem) {
		const selectorBucket = expression.getBucket();
		const bucketsForNode = getBucketsForNode(contextItem);
		if (selectorBucket !== null && !bucketsForNode.includes(selectorBucket)) {
			// We are sure that this selector will never match, without even running it
			return false as IReturnTypes<TNode>[TReturnType];
		}
	}

	try {
		markXPathStart(selector);

		const rawResults = expression.evaluateMaybeStatically(dynamicContext, executionParameters);
		const toReturn = convertXDMReturnValue<TNode, TReturnType>(
			selector,
			rawResults,
			returnType,
			executionParameters
		);
		markXPathEnd(selector);
		return toReturn;
	} catch (error) {
		printAndRethrowError(selector, error);
	}
};
*/