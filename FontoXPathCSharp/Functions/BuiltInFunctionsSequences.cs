using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Functions;

public static class BuiltInFunctionsSequences<TNode>
{
    private static readonly FunctionSignature<ISequence, TNode> FnCount = (_, _, _, args) =>
    {
        var hasPassed = false;
        return SequenceFactory.CreateFromIterator(_ =>
        {
            if (hasPassed) return IteratorResult<AbstractValue>.Done();

            hasPassed = true;
            return IteratorResult<AbstractValue>.Ready(new IntValue(args[0].GetLength()));
        }, 1);
    };

    private static readonly FunctionSignature<ISequence, TNode> FnZeroOrOne = (_, _, _, args) =>
    {
        var arg = args[0];
        if (!arg.IsEmpty() && !arg.IsSingleton())
        {
            arg.GetAllValues().ToList().ForEach(Console.WriteLine);
            throw new XPathException("FORG0003", "The argument passed to fn:zero-or-one contained more than one item.");
        }

        return arg;
    };

    private static readonly FunctionSignature<ISequence, TNode> FnExists = (_, _, _, args) =>
        SequenceFactory.CreateFromValue(new BooleanValue(!args[0].IsEmpty()));

    private static readonly FunctionSignature<ISequence, TNode> FnEmpty = (_, _, _, args) =>
        SequenceFactory.CreateFromValue(new BooleanValue(args[0].IsEmpty()));

    private static readonly FunctionSignature<ISequence, TNode> FnDeepEqual =
        (dynamicContext, executionParameters, staticContext, args) =>
        {
            var hasPassed = false;
            var deepEqualityIterator = BuiltInFunctionsSequencesDeepEqual<TNode>.SequenceDeepEqual(
                dynamicContext,
                executionParameters,
                staticContext,
                args[0],
                args[1]
            );

            return SequenceFactory.CreateFromIterator(
                _ =>
                {
                    if (hasPassed) return IteratorResult<BooleanValue>.Done();

                    var result = deepEqualityIterator(IterationHint.None);
                    if (result.IsDone) return result;

                    hasPassed = true;
                    return IteratorResult<BooleanValue>.Ready(result.Value!);
                }
            );
        };


    public static readonly BuiltinDeclarationType<TNode>[] Declarations =
    {
        new(new[] { new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrOne) },
            FnCount, "count",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsInteger, SequenceMultiplicity.ExactlyOne)),
        new(new[] { new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrMore) },
            FnZeroOrOne, "zero-or-one",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.Item, SequenceMultiplicity.ZeroOrOne)),
        new(new[] { new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrMore) },
            FnEmpty, "empty", BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsBoolean, SequenceMultiplicity.ExactlyOne)),
        new(new[] { new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrMore) },
            FnExists, "exists", BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsBoolean, SequenceMultiplicity.ExactlyOne)),
        new(new[]
            {
                new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrMore),
                new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrMore)
            },
            FnDeepEqual, "deep-equal", BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsBoolean, SequenceMultiplicity.ExactlyOne)),
        new(new[]
            {
                new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrMore),
                new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrMore),
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)
            },
            (_, _, _, _) => { throw new Exception("FOCH0002: No collations are supported"); },
            "deep-equal", BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsBoolean, SequenceMultiplicity.ExactlyOne))
    };
}