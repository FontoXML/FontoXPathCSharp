using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class ArrayValue<T> : FunctionValue<T>
{
    public ArrayValue(Func<ISequence>[] members, FunctionDefinitionType<T> definitionType) : base(
        new[] { new ParameterType(ValueType.XsInteger, SequenceMultiplicity.ExactlyOne) }, 1, definitionType,
        ValueType.Array)
    {
        Members = Members;
    }

    private Func<ISequence>? Members { get; }
}