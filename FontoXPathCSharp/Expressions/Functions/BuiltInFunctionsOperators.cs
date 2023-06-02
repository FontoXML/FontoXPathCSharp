using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.Functions;

public static class BuiltInFunctionsOperators<TNode> where TNode : notnull
{
    private static readonly FunctionSignature<ISequence, TNode> FnOpTo = (_, _, _, sequences) =>
    {
        var fromSequence = sequences[0];
        var toSequence = sequences[1];
        // shortcut the non-trivial case of both values being known
        // RangeExpr is inclusive: 1 to 3 will make (1,2,3)
        var from = fromSequence.First();
        var to = toSequence.First();
        if (from == null || to == null) return SequenceFactory.CreateEmpty();

        var fromValue = Convert.ToInt32(from.GetAs<UntypedAtomicValue>().Value);
        var toValue = Convert.ToInt32(to.GetAs<UntypedAtomicValue>().Value);
        if (fromValue > toValue) return SequenceFactory.CreateEmpty();

        // By providing a length, we do not have to hold an end condition into account
        return SequenceFactory.CreateFromIterator(
            _ => IteratorResult<AbstractValue>.Ready(AtomicValue.Create(fromValue++, ValueType.XsInteger)),
            toValue - fromValue + 1
        );
    };

    public static readonly BuiltinDeclarationType<TNode>[] Declarations =
    {
        new(new[]
            {
                new ParameterType(ValueType.XsInteger, SequenceMultiplicity.ZeroOrOne),
                new ParameterType(ValueType.XsInteger, SequenceMultiplicity.ZeroOrOne)
            },
            FnOpTo, "to", "http://fontoxpath/operators",
            new SequenceType(ValueType.XsInteger, SequenceMultiplicity.ZeroOrMore)
        )
    };
}