using FontoXPathCSharp.Functions;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class ArrayValue<TNode> : FunctionValue<ISequence, TNode>
{
    public ArrayValue(List<Func<ISequence>> members) : base(
        new[] { new ParameterType(ValueType.XsInteger, SequenceMultiplicity.ExactlyOne) }, 1,
        null,
        ValueType.Array)
    {
        Members = members;
        Value = (dynamicContext, executionParameters, staticContext, key) =>
            BuiltInFunctionsArraysGet<TNode>.ArrayGet(
                dynamicContext,
                executionParameters,
                staticContext,
                SequenceFactory.CreateFromValue(this),
                key[0]);
    }

    public List<Func<ISequence>> Members { get; }
}