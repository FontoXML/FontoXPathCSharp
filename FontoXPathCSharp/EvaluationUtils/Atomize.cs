using System.Xml;
using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Expressions.DataTypes.Builtins;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Types.Node;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.EvaluationUtils;

public static class Atomize
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
        if (value.GetValueType().IsSubTypeOfAny(ValueType.XsAnyAtomicType, ValueType.XsUntypedAtomic,
                ValueType.XsBoolean, ValueType.XsDecimal,
                ValueType.XsDouble, ValueType.XsFloat, ValueType.XsInteger, ValueType.XsNumeric, ValueType.XsQName,
                ValueType.XsQName, ValueType.XsString
            ))
            return SequenceFactory.CreateFromValue(value);

        var domfacade = executionParameters.DomFacade;

        if (value.GetValueType().IsSubtypeOf(ValueType.Node))
        {
            var pointer = value.GetAs<NodeValue>();

            if (pointer.GetValueType().IsSubTypeOfAny(ValueType.Attribute, ValueType.Text))
            {
                return SequenceFactory.CreateFromValue(new StringValue(pointer.Value.InnerText));
            }
            // throw new NotImplementedException("Not sure how to do this with the XmlNode replacing domfacade yet");
            // return SequenceFactory.CreateFromIterator(CreateAtomicValue(domfacade[]));

            if (pointer.GetValueType().IsSubTypeOfAny(ValueType.Comment, ValueType.ProcessingInstruction))
            {
                return SequenceFactory.CreateFromValue(new StringValue(pointer.Value.InnerText));
            }

            var allTexts = new List<string>();

            Action<XmlNode>? getTextNodes = null;
            getTextNodes = node =>
            {
                var aNode = new NodeValue(node);
                if (pointer.GetValueType().IsSubTypeOfAny(ValueType.Comment, ValueType.ProcessingInstruction)) return;

                if (aNode.GetValueType().IsSubTypeOfAny(ValueType.Text))
                {
                    allTexts.Add(node.InnerText);
                }

                if (aNode.GetValueType().IsSubTypeOfAny(ValueType.Element, ValueType.DocumentNode))
                {
                    var children = aNode.Value.ChildNodes;
                    foreach (XmlNode child in children)
                    {
                        getTextNodes?.Invoke(child);
                    }
                }
            };

            return SequenceFactory.CreateFromValue(CreateAtomicValue(string.Join("", allTexts), ValueType.XsUntypedAtomic));

            // (function getTextNodes(aNode: ConcreteNode | TinyNode) {
            //     if (
            //         pointer.node.nodeType === NODE_TYPES.COMMENT_NODE ||
            //                                   pointer.node.nodeType === NODE_TYPES.PROCESSING_INSTRUCTION_NODE
            //     ) {
            //         return;
            //     }
            //     const aNodeType = aNode['nodeType'];
            //
            //     if (aNodeType === NODE_TYPES.TEXT_NODE || aNodeType === NODE_TYPES.CDATA_SECTION_NODE) {
            //         allTexts.push(
            //             domFacade.getData(aNode as ConcreteCharacterDataNode | TinyCharacterDataNode)
            //         );
            //         return;
            //     }
            //     if (aNodeType === NODE_TYPES.ELEMENT_NODE || aNodeType === NODE_TYPES.DOCUMENT_NODE) {
            //         const children = domFacade.getChildNodes(
            //             aNode as ConcreteParentNode | TinyParentNode
            //         );
            //         children.forEach((child) => {
            //             getTextNodes(child as ConcreteParentNode | TinyParentNode);
            //         });
            //     }
            // })(pointer.node);

        }

        if (value.GetValueType().IsSubtypeOf(ValueType.Function) &&
            !value.GetValueType().IsSubtypeOf(ValueType.Array))
            //TODO: Create dedicated function and add proper type to string function.
            throw new Exception($"FOTY0013: Atomization is not supported for {value.GetValueType()}.");

        if (value.GetValueType().IsSubtypeOf(ValueType.Array))
            throw new NotImplementedException("Implement ArrayValue forst");

        throw new Exception($"Atomizing type {value.GetType()} is not implemented.");
    }

    // TODO: Move all this stuff to the AtomicValue file.
    public static AtomicValue CreateAtomicValue<T>(T value, ValueType type)
    {
        if (!BuiltinDataTypes.Instance.BuiltinDataTypesByType.ContainsKey(type))
            throw new Exception($"Cannot create atomic value from type: {type}");

        return type switch
        {
            ValueType.XsBoolean => new BooleanValue((bool)(object)value!),
            ValueType.XsInt => new IntValue((int)(object)value!),
            ValueType.XsFloat => new FloatValue((decimal)(object)value!),
            ValueType.XsDouble => new DoubleValue((decimal)(object)value!),
            ValueType.XsString => new StringValue((string)(object)value!),
            ValueType.XsQName => new QNameValue((QName)(object)value!),
            ValueType.XsUntypedAtomic => new UntypedAtomicValue((object)value!),
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