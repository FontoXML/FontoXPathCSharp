using System.Collections;
using System.Reflection;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.EvaluationUtils;
using FontoXPathCSharp.Expressions.Functions;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;

namespace FontoXPathCSharp;

public delegate TReturn FunctionCallback<TNode, out TReturn>(
    DynamicContextAdapter<TNode>? domFacade
) where TNode : notnull;

public delegate TReturn FunctionCallback<TNode, in T1, out TReturn>(
    DynamicContextAdapter<TNode>? domFacade,
    T1 arg1
) where TNode : notnull;

public delegate TReturn FunctionCallback<TNode, in T1, in T2, out TReturn>(
    DynamicContextAdapter<TNode>? domFacade,
    T1 arg1,
    T2 arg2
) where TNode : notnull;

public delegate TReturn FunctionCallback<TNode, in T1, in T2, in T3, out TReturn>(
    DynamicContextAdapter<TNode>? domFacade,
    T1 arg1,
    T2 arg2,
    T3 arg3
) where TNode : notnull;

public delegate TReturn FunctionCallback<TNode, in T1, in T2, in T3, in T4, out TReturn>(
    DynamicContextAdapter<TNode>? domFacade,
    T1 arg1,
    T2 arg2,
    T3 arg3,
    T4 arg4
) where TNode : notnull;

public record DynamicContextAdapter<TNode>(
    object? CurrentContext,
    IDomFacade<TNode> DomFacade
) where TNode : notnull;

public class CustomXPathFunctionException : Exception
{
    public CustomXPathFunctionException(Exception innerError, string localName, string? namespaceUri)
        : base(GenerateMessage(innerError.StackTrace, innerError.Message, localName, namespaceUri))
    {
    }

    private static string GenerateMessage(string? stack, string innerErrorMessage, string localName,
        string namespaceUri)
    {
        if (stack != null)
        {
            // On some browsers, error.stack includes error.message, on others it does not. We make sure
            // we only have the stack without message
            if (stack.Contains(innerErrorMessage))
                stack = stack[(stack.IndexOf(innerErrorMessage, StringComparison.Ordinal) + innerErrorMessage.Length)..]
                    .Trim();

            // Some browsers show the entire call stack and some browsers only include the last 10
            // calls. We force it at the last 10 calls to prevent that recursive custom xpath
            // functions include the full call stack multiple times.
            var stackLines = stack.Split('\n');
            stackLines = stackLines.Take(10).ToArray();

            // We always indent our XQuery stack trace lines with 2 spaces. For easier readability
            // we ensure these are indented with 4 spaces (some browsers already do this)
            stackLines = stackLines
                .Select(line => line.StartsWith("    ") || line.StartsWith('\t') ? line : $"    {line}")
                .ToArray();

            stack = string.Join('\n', stackLines);
        }

        var message = $"Custom XPath function Q{{{namespaceUri}}}{localName} raised:\n{innerErrorMessage}\n{stack}";
        return message;
    }
}

public static class RegisterCustomXPathFunction<TNode> where TNode : notnull
{
    public static void RegisterFunction<TReturn>(
        QName qName,
        string[] signatureNames,
        string returnTypeName,
        FunctionCallback<TNode, TReturn> callback)
    {
        RegisterFunction(qName,
            signatureNames,
            returnTypeName,
            callback,
            (dynamicContextAdapter, callbackFunction, _) =>
                callbackFunction(dynamicContextAdapter));
    }

    private static T ParamConvert<T>(object param)
    {
        return param switch
        {
            T t => t,
            Array a => ReflectionCast.CastToArrayDynamically<T>(a),
            _ => (T)Convert.ChangeType(param, typeof(T))
        };
    }

    public static void RegisterFunction<T1, TReturn>(
        QName qName,
        string[] signatureNames,
        string returnTypeName,
        FunctionCallback<TNode, T1, TReturn> callback)
    {
        // argCaster1 ??= ParamConvert<T1>;
        RegisterFunction(qName,
            signatureNames,
            returnTypeName,
            callback,
            (dynamicContextAdapter, callbackFunction, args) =>
                callbackFunction(
                    dynamicContextAdapter,
                    ParamConvert<T1>(args[0]!)
                ));
    }

    public static void RegisterFunction<T1, T2, TReturn>(
        QName qName,
        string[] signatureNames,
        string returnTypeName,
        FunctionCallback<TNode, T1, T2, TReturn> callback)
    {
        RegisterFunction(qName,
            signatureNames,
            returnTypeName,
            callback,
            (dynamicContextAdapter, callbackFunction, args) =>
                callbackFunction(
                    dynamicContextAdapter,
                    ParamConvert<T1>(args[0]!),
                    ParamConvert<T2>(args[1]!)
                ));
    }

    public static void RegisterFunction<T1, T2, T3, TReturn>(
        QName qName,
        string[] signatureNames,
        string returnTypeName,
        FunctionCallback<TNode, T1, T2, T3, TReturn> callback)
    {
        RegisterFunction(qName,
            signatureNames,
            returnTypeName,
            callback,
            (dynamicContextAdapter, callbackFunction, args) =>
                callbackFunction(
                    dynamicContextAdapter,
                    ParamConvert<T1>(args[0]!),
                    ParamConvert<T2>(args[1]!),
                    ParamConvert<T3>(args[2]!)
                ));
    }

    public static void RegisterFunction<T1, T2, T3, T4, TReturn>(
        QName qName,
        string[] signatureNames,
        string returnTypeName,
        FunctionCallback<TNode, T1, T2, T3, T4, TReturn> callback)
    {
        RegisterFunction(qName,
            signatureNames,
            returnTypeName,
            callback,
            (dynamicContextAdapter, callbackFunction, args) =>
                callbackFunction(
                    dynamicContextAdapter,
                    ParamConvert<T1>(args[0]!),
                    ParamConvert<T2>(args[1]!),
                    ParamConvert<T3>(args[2]!),
                    ParamConvert<T4>(args[3]!)
                ));
    }

    public static void RegisterFunction<TCallback, TReturn>(
        QName qName,
        string[] signatureNames,
        string returnTypeName,
        TCallback callback,
        Func<DynamicContextAdapter<TNode>, TCallback, object?[], TReturn?> callbackRunner)
    {
        if (string.IsNullOrEmpty(qName.NamespaceUri))
            throw new XPathException(
                "XQST0060",
                "Functions declared in a module or as an external function must reside in a namespace."
            );

        var signature = signatureNames.Select(n => new ParameterType(SequenceType.StringToSequenceType(n))).ToArray();
        var returnType = SequenceType.StringToSequenceType(returnTypeName);

        FunctionSignature<ISequence, TNode> callFunction =
            (_, executionParameters, _, args) =>
            {
                var newArgs = args.Select((argument, index) => AdaptToValues<TNode>.AdaptXPathValueToValue(
                    argument,
                    signature[index],
                    executionParameters
                )).ToArray();

                // Adapt the domFacade into another object to prevent passing everything. The closure compiler might rename some variables otherwise.
                // Since the interface for domFacade (IDomFacade) is marked as extern, it will not be changed
                var dynamicContextAdapter = new DynamicContextAdapter<TNode>(
                    executionParameters.CurrentContext,
                    executionParameters.DomFacade.Unwrap()
                );

                TReturn? result;

                try
                {
                    result = callbackRunner(dynamicContextAdapter, callback, newArgs);
                }
                catch (Exception error)
                {
                    throw new CustomXPathFunctionException(error, qName.LocalName, qName.NamespaceUri);
                }

                if (result is AbstractValue value)
                    // If this symbol is present, the value has already undergone type conversion.
                    return SequenceFactory.CreateFromValue(value);

                return AdaptToValues<TNode>.AdaptValueToSequence(executionParameters.DomFacade, result, returnType);
            };

        FunctionRegistry<TNode>.RegisterFunction(
            qName.NamespaceUri,
            qName.LocalName,
            signature,
            returnType,
            callFunction
        );
    }
}