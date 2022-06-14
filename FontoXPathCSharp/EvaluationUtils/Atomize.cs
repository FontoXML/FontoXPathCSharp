using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Types.Node;
using FontoXPathCSharp.Value;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.EvaluationUtils;

public class Atomize
{
    public static ISequence AtomizeSequence(ISequence sequence, ExecutionParameters parameters)
    {
        var done = false;
        var it = sequence.GetValue();
        Iterator<AbstractValue>? currentOutput = null;

        return SequenceFactory.CreateFromIterator(hint =>
        {
            while (!done)
            {
                if (currentOutput == null)
                {
                    var inputItem = it(IterationHint.None);
                    if (inputItem.IsDone)
                    {
                        done = true;
                        break;
                    }

                    var outputSequence = AtomizeSingleValue(inputItem.Value, parameters);
                    currentOutput = outputSequence.GetValue();
                }

                var itemToOutput = currentOutput(IterationHint.None);
                if (itemToOutput.IsDone)
                {
                    currentOutput = null;
                    continue;
                }

                return itemToOutput;
            }

            return IteratorResult<AbstractValue>.Done();
        });
    }

    private static ISequence AtomizeSingleValue(AbstractValue value, ExecutionParameters executionParameters)
    {
        if (SubtypeUtils.IsSubTypeOfAny(value.GetValueType(), new[]
            {
                ValueType.XsAnyAtomicType, ValueType.XsUntypedAtomic, ValueType.XsBoolean, ValueType.XsDecimal,
                ValueType.XsDouble, ValueType.XsFloat, ValueType.XsInteger, ValueType.XsNumeric, ValueType.XsQName,
                ValueType.XsQName, ValueType.XsString
            }))
            return SequenceFactory.CreateFromValue(value);

        var domfacade = executionParameters.DomFacade;

        if (SubtypeUtils.IsSubtypeOf(value.GetValueType(), ValueType.Node))
        {
            var pointer = value.GetAs<NodePointer>(ValueType.Node);

            if (pointer?.Node.NodeType is NodeTypes.AttributeNode or NodeTypes.TextNode)
                throw new NotImplementedException("Not sure how to do this with the XmlNode replacing domfacade yet");
            // return SequenceFactory.CreateFromIterator(CreateAtomicValue(domfacade[]));
            //TODO: Finish off this if block for the other node things.
        }

        if (SubtypeUtils.IsSubtypeOf(value.GetValueType(), ValueType.Function) &&
            !SubtypeUtils.IsSubtypeOf(value.GetValueType(), ValueType.Array))
            //TODO: Create dedicated function and add proper type to string function.
            throw new Exception($"FOTY0013: Atomization is not supported for {value.GetValueType()}.");

        if (SubtypeUtils.IsSubtypeOf(value.GetValueType(), ValueType.Array))
            throw new NotImplementedException("Implement ArrayValue forst");

        throw new Exception($"Atomizing type {value.GetType()} is not implemented.");
    }

    public static AtomicValue<T> CreateAtomicValue<T>(T value, ValueType type)
    {
        return new AtomicValue<T>(value, type);
    }

    public static AtomicValue<bool> TrueBoolean()
    {
        return new AtomicValue<bool>(true, ValueType.XsBoolean);
    }

    public static AtomicValue<bool> FalseBoolean()
    {
        return new AtomicValue<bool>(false, ValueType.XsBoolean);
    }
}