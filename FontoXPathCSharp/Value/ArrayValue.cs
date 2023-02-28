using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Expressions.Functions;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class ArrayValue<TNode> : FunctionValue<ISequence, TNode> where TNode : notnull
{
    public ArrayValue(List<Func<ISequence>> members) :
        base(new[]
            {
                new ParameterType(ValueType.XsInteger, SequenceMultiplicity.ExactlyOne)
            },
            1,
            "get",
            BuiltInNamespaceUris.ArrayNamespaceUri.GetUri(),
            new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrMore),
            null!) // can't set value with the this in it.
    {
        Type = ValueType.Array;
        Members = members;
        Value = (_, _, _, key) =>
            BuiltInFunctionsArraysGet<TNode>.ArrayGet(SequenceFactory.CreateFromValue(this), key[0]);
    }

    public List<Func<ISequence>> Members { get; }
}