using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Functions;

public static class BuiltInFunctionsSequences
{
    private static readonly FunctionDefinitionType<ISequence> FnCount = (_, _, _, args) =>
    {
        var hasPassed = false;
        return SequenceFactory.CreateFromIterator(_ =>
        {
            if (hasPassed) return IteratorResult<AbstractValue>.Done();

            hasPassed = true;
            return IteratorResult<AbstractValue>.Ready(new IntValue(args[0].GetLength()));
        }, 1);
    };

    private static readonly FunctionDefinitionType<ISequence> FnZeroOrOne = (_, _, _, args) =>
    {
        var arg = args[0];
        if (!arg.IsEmpty() && !arg.IsSingleton())
        {
            arg.GetAllValues().ToList().ForEach(Console.WriteLine);
            throw new XPathException("FORG0003: The argument passed to fn:zero-or-one contained more than one item.");
        }

        return arg;
    };

    public static readonly BuiltinDeclarationType[] Declarations =
    {
        new(new[] { new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrOne) },
            FnCount, "count",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsInteger, SequenceMultiplicity.ExactlyOne)),
        new(new[] { new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrMore) },
            FnZeroOrOne, "zero-or-one",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.Item, SequenceMultiplicity.ZeroOrOne))
    };
}