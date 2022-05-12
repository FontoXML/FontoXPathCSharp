using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Functions;

public static class BuiltInFunctionsSequences
{
    private static readonly FunctionDefinitionType<ISequence> FnCount = (context, parameters, staticContext, args) =>
    {
        var hasPassed = false;
        return new IteratorBackedSequence(_ =>
        {
            if (hasPassed)
            {
                return IteratorResult<AbstractValue>.Done();
            }

            hasPassed = true;
            return IteratorResult<AbstractValue>.Ready(new IntValue(args[0].GetLength()));
        }, 1);
    };

    public static readonly BuiltinDeclarationType[] Declarations =
    {
        new(new[] {new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrOne)},
            FnCount, "count",
            "http://www.w3.org/2005/xpath-functions",
            new SequenceType(ValueType.XsInteger, SequenceMultiplicity.ExactlyOne))
    };
}