using FontoXPathCSharp.Functions;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class ArrayValue : FunctionValue<ISequence>
{
    public ArrayValue(List<Func<ISequence>> members) : base(
        new[] { new ParameterType(ValueType.XsInteger, SequenceMultiplicity.ExactlyOne) }, 1,
        null,
        ValueType.Array)
    {
        Members = members;
        _value = (dynamicContext, executionParameters, staticContext, key) =>
            BuiltInFunctionsArraysGet.ArrayGet(
                dynamicContext,
                executionParameters,
                staticContext,
                SequenceFactory.CreateFromValue(this),
                key[0]);
    }

    public List<Func<ISequence>> Members { get; }
}