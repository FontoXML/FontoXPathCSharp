using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Expressions.DataTypes.Builtins;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Types.Node;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
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
        if (SubtypeUtils.IsSubTypeOfAny(value.GetValueType(),
                ValueType.XsAnyAtomicType, ValueType.XsUntypedAtomic, ValueType.XsBoolean, ValueType.XsDecimal,
                ValueType.XsDouble, ValueType.XsFloat, ValueType.XsInteger, ValueType.XsNumeric, ValueType.XsQName,
                ValueType.XsQName, ValueType.XsString
            ))
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

    public static AtomicValue CreateAtomicValue<T>(T value, ValueType type)
    {
        if (!BuiltinDataTypes.Instance.BuiltinDataTypesByType.ContainsKey(type))
            throw new Exception($"Cannot create atomic value from type: {type}");

        return type switch
        {
            ValueType.XsBoolean => new BooleanValue((bool)(object)value!),
            ValueType.XsInt => new IntValue((int)(object)value!),
            ValueType.XsFloat => new FloatValue((float)(object)value!),
            ValueType.XsDouble => new DoubleValue((double)(object)value!),
            ValueType.XsString => new StringValue((string)(object)value!),
            ValueType.XsQName => new QNameValue((QName)(object)value!),
            _ => throw new ArgumentOutOfRangeException($"Atomic Value for {type} is not implemented yet.")
        };
    }


    public static AtomicValue TrueBoolean()
    {
        return CreateAtomicValue(true, ValueType.XsBoolean);
    }

    public static AtomicValue FalseBoolean()
    {
        return CreateAtomicValue(false, ValueType.XsBoolean);
    }
}