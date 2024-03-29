using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public delegate T FunctionSignature<out T, TNode>(
    DynamicContext? dynamicContext,
    ExecutionParameters<TNode> executionParameters,
    StaticContext<TNode>? staticContext, params ISequence[] sequences) where TNode : notnull;

public delegate ISequence FunctionDefinitionType<TNode>(
    DynamicContext? dynamicContext,
    ExecutionParameters<TNode> executionParameters,
    StaticContext<TNode>? staticContext, params ISequence[] sequences) where TNode : notnull;

public class FunctionValue<TReturn, TNode> : AbstractValue where TReturn : ISequence where TNode : notnull
{
    private readonly string? _namespaceUri;
    public readonly int Arity;
    public readonly bool IsUpdating;

    public FunctionValue(
        ParameterType[] argumentTypes,
        int arity,
        string localName,
        string namespaceUri,
        SequenceType returnType,
        FunctionSignature<TReturn, TNode> value,
        bool isAnonymous = false,
        bool isUpdating = false) : base(ValueType.Function)
    {
        Value = value;
        Arity = arity;
        Name = localName;
        IsUpdating = isUpdating;
        _namespaceUri = namespaceUri;
        IsAnonymous = isAnonymous;
        ReturnType = returnType;
        ArgumentTypes = ExpandParameterTypeToArity(argumentTypes, arity);
    }

    public FunctionSignature<TReturn, TNode> Value { get; set; }

    public ParameterType[] ArgumentTypes { get; }

    public string Name { get; }

    public SequenceType ReturnType { get; }

    public bool IsAnonymous { get; }

    private ParameterType[] ExpandParameterTypeToArity(ParameterType[] argumentTypes, int arity)
    {
        var indexOfRest = Array.FindLastIndex(argumentTypes, a => a.IsEllipsis);

        if (indexOfRest > -1)
        {
            var replacePart = Enumerable.Repeat(argumentTypes[indexOfRest - 1], arity - (argumentTypes.Length - 1));
            return argumentTypes[..indexOfRest].Concat(replacePart).ToArray();
        }

        return argumentTypes;
    }

    public ISequence ApplyArguments(ISequence?[] appliedArguments)
    {
        var fn = Value;

        var argumentSequenceCreators =
            appliedArguments.Select(arg => arg == null ? null : ISequence.CreateDoublyIterableSequence(arg)).ToList();

        FunctionSignature<ISequence, TNode> curriedFunction =
            (dynamicContext, executionParameters, staticContext, sequences) =>
            {
                var newArguments = sequences.ToList();
                var allArguments = argumentSequenceCreators.Select(createArgumentSequence =>
                {
                    // If createArgumentSequence == null, it is a placeholder, so use a provided one
                    if (createArgumentSequence == null)
                    {
                        var firstEntry = newArguments[0];
                        newArguments.RemoveAt(0);
                        return firstEntry;
                    }

                    return createArgumentSequence();
                }).ToArray();
                return fn(dynamicContext, executionParameters, staticContext, allArguments);
            };

        var argumentTypes = appliedArguments.Reduce(
            new List<ParameterType>(),
            (indices, arg, index) =>
            {
                if (arg == null) indices.Add(ArgumentTypes[index]);
                return indices;
            }
        ).ToArray();

        var functionItem = new FunctionValue<ISequence, TNode>(
            argumentTypes,
            argumentTypes.Length,
            "boundFunction",
            _namespaceUri!,
            ReturnType,
            curriedFunction,
            true,
            IsUpdating
        );

        return SequenceFactory.CreateFromValue(functionItem);
    }
}