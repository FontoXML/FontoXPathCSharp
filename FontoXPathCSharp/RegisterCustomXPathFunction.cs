using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.EvaluationUtils;
using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp;

public delegate object? FunctionCallback<TNode>(
    DynamicContextAdapter<TNode> domFacade,
    params object[] functionArgs
) where TNode : notnull;

public record DynamicContextAdapter<TNode>(
    object? CurrentContext,
    IDomFacade<TNode> DomFacade
) where TNode : notnull;

public class CustomXPathFunctionException : Exception
{
    public CustomXPathFunctionException(Exception innerError, string localName, string namespaceUri)
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
    public static void RegisterFunction(
        string name,
        string[] signatureNames,
        string returnTypeName,
        FunctionCallback<TNode> callback)
    {
        var qName = SplitFunctionName(name);

        if (qName.NamespaceUri == null)
            throw new XPathException(
                "XQST0060",
                "Functions declared in a module or as an external function must reside in a namespace."
            );

        var signature = signatureNames.Select(SequenceType.StringToSequenceType).ToArray();
        var returnType = SequenceType.StringToSequenceType(returnTypeName);

        FunctionSignature<ISequence, TNode> callFunction =
            (dynamicContext, executionParameters, staticContext, args) =>
            {
                var newArguments = args.Select((argument, index) => AdaptXPathValueToValue(
                    argument,
                    signature[index],
                    executionParameters
                ));

                // Adapt the domFacade into another object to prevent passing everything. The closure compiler might rename some variables otherwise.
                // Since the interface for domFacade (IDomFacade) is marked as extern, it will not be changed
                var dynamicContextAdapter = new DynamicContextAdapter<TNode>(
                    executionParameters.CurrentContext,
                    executionParameters.DomFacade.Unwrap()
                );

                object? result;
                try
                {
                    result = callback(dynamicContextAdapter, newArguments);
                }
                catch (Exception error)
                {
                    // We throw our own error here so we can keep the JS stack only for custom XPath
                    // functions
                    throw new CustomXPathFunctionException(error, qName.LocalName, qName.NamespaceUri);
                }

                if (result != null && result is AbstractValue)
                    // If this symbol is present, the value has already undergone type conversion.
                    return SequenceFactory.CreateFromValue(result as AbstractValue);

                return AdaptToValues<TNode>.AdaptValueToSequence(executionParameters.DomFacade, result, returnType);
            };
    }


    private static QName SplitFunctionName(string name)
    {
        var parts = name.Split(':');
        if (parts.Length != 2)
            throw new Exception("Do not register custom functions in the default function namespace");

        var prefix = parts[0];
        var localName = parts[1];

        var namespaceUriForPrefix = StaticallyKnownNamespacesExtensions.GetStaticallyKnownNamespaceByPrefix(prefix);
        if (namespaceUriForPrefix == null)
        {
            namespaceUriForPrefix = $"generated_namespace_uri_{prefix}";
            StaticallyKnownNamespacesExtensions.RegisterStaticallyKnownNamespace(prefix, namespaceUriForPrefix);
        }

        return new QName(localName, namespaceUriForPrefix);
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