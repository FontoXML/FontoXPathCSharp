using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.EvaluationUtils;
using FontoXPathCSharp.Expressions.Functions;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

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

public delegate TReturn FunctionCallback<TNode, in T1, in T2, in T3, in T4, in T5, out TReturn>(
    DynamicContextAdapter<TNode>? domFacade,
    T1 arg1,
    T2 arg2,
    T3 arg3,
    T4 arg4,
    T5 arg5
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

public class RegisterCustomXPathFunction<TNode> where TNode : notnull
{
    // public static void RegisterFunction(
    //     string name,
    //     string[] signatureNames,
    //     string returnTypeName,
    //     FunctionCallback<TNode> callback)
    // {
    //     RegisterFunction(SplitFunctionName(name), signatureNames, returnTypeName, callback);
    // }

    private static void RegisterFunction<TCallback>(
        QName qName,
        string[] signatureNames,
        string returnTypeName,
        TCallback callback,
        Func<DynamicContextAdapter<TNode>, TCallback, object?[], object?> callbackRunner)
    {
        if (qName.NamespaceUri == null)
            throw new XPathException(
                "XQST0060",
                "Functions declared in a module or as an external function must reside in a namespace."
            );

        var signature = signatureNames.Select(n => new ParameterType(SequenceType.StringToSequenceType(n))).ToArray();
        var returnType = SequenceType.StringToSequenceType(returnTypeName);

        FunctionSignature<ISequence, TNode> callFunction =
            (_, executionParameters, _, args) =>
            {
                var newArgs = args.Select((argument, index) => AdaptXPathValueToValue(
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

                var result = callbackRunner(dynamicContextAdapter, callback, newArgs);

                if (result != null && result is AbstractValue)
                    // If this symbol is present, the value has already undergone type conversion.
                    return SequenceFactory.CreateFromValue(result as AbstractValue);

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
            {
                try
                {
                    return callbackFunction(dynamicContextAdapter);
                }
                catch (Exception error)
                {
                    throw new CustomXPathFunctionException(error, qName.LocalName, qName.NamespaceUri);
                }
            });
    }

    public static void RegisterFunction<T1, TReturn>(
        QName qName,
        string[] signatureNames,
        string returnTypeName,
        FunctionCallback<TNode, T1, TReturn> callback)
    {
        RegisterFunction(qName,
            signatureNames,
            returnTypeName,
            callback,
            (dynamicContextAdapter, callbackFunction, args) =>
            {
                try
                {
                    return callbackFunction(dynamicContextAdapter, (T1)args[0]!);
                }
                catch (Exception error)
                {
                    throw new CustomXPathFunctionException(error, qName.LocalName, qName.NamespaceUri);
                }
            });
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
            {
                try
                {
                    return callbackFunction(dynamicContextAdapter, (T1)args[0]!, (T2)args[1]!);
                }
                catch (Exception error)
                {
                    throw new CustomXPathFunctionException(error, qName.LocalName, qName.NamespaceUri);
                }
            });
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
            {
                try
                {
                    return callbackFunction(dynamicContextAdapter, (T1?)args[0]!, (T2?)args[1]!, (T3?)args[2]!);
                }
                catch (Exception error)
                {
                    throw new CustomXPathFunctionException(error, qName.LocalName, qName.NamespaceUri);
                }
            });
    }

    public static void RegisterFunction<T1, T2, T3, T4, TReturn>(
        QName qName,
        string[] signatureNames,
        string returnTypeName,
        FunctionCallback<TNode, T1?, T2?, T3?, T4?, TReturn> callback)
    {
        RegisterFunction(qName,
            signatureNames,
            returnTypeName,
            callback,
            (dynamicContextAdapter, callbackFunction, args) =>
            {
                try
                {
                    return callbackFunction(dynamicContextAdapter, (T1)args[0]!, (T2)args[1]!, (T3)args[2]!, (T4)args[3]!);
                }
                catch (Exception error)
                {
                    throw new CustomXPathFunctionException(error, qName.LocalName, qName.NamespaceUri);
                }
            });
    }

    public static object? AdaptXPathValueToValue(
        ISequence valueSequence,
        SequenceType sequenceType,
        ExecutionParameters<TNode> executionParameters)
    {
        return sequenceType.Multiplicity switch
        {
            SequenceMultiplicity.ZeroOrOne when valueSequence.IsEmpty() => null,
            SequenceMultiplicity.ZeroOrOne => TransformValues<TNode>.TransformXPathItemToObject(valueSequence.First()!,
                    executionParameters)
                (IterationHint.None)
                .Value,
            SequenceMultiplicity.ZeroOrMore or SequenceMultiplicity.OneOrMore => valueSequence.GetAllValues()
                .Select(value =>
                {
                    if (value.GetValueType().IsSubtypeOf(ValueType.Attribute))
                        throw new Exception("Cannot pass attribute nodes to custom functions");

                    return TransformValues<TNode>.TransformXPathItemToObject(value, executionParameters)(IterationHint
                        .None).Value;
                }),
            _ => TransformValues<TNode>
                .TransformXPathItemToObject(valueSequence.First()!, executionParameters)(IterationHint.None).Value
        };
    }
}