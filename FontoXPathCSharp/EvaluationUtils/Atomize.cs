using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.EvaluationUtils;

public static class Atomize
{
    public static ISequence AtomizeSequence<TNode>(ISequence sequence, ExecutionParameters<TNode> parameters)
        where TNode : notnull
    {
        var done = false;
        var it = sequence.GetValue();
        Iterator<AbstractValue>? currentOutput = null;

        return SequenceFactory.CreateFromIterator(_ =>
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

                    var outputSequence = AtomizeSingleValue(inputItem.Value!, parameters);
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

    public static ISequence AtomizeSingleValue<TNode>(AbstractValue value,
        ExecutionParameters<TNode> executionParameters) where TNode : notnull
    {
        if (value.GetValueType().IsSubtypeOfAny(ValueType.XsAnyAtomicType, ValueType.XsUntypedAtomic,
                ValueType.XsBoolean, ValueType.XsDecimal,
                ValueType.XsDouble, ValueType.XsFloat, ValueType.XsInteger, ValueType.XsNumeric, ValueType.XsQName,
                ValueType.XsQName, ValueType.XsString
            ))
            return SequenceFactory.CreateFromValue(value);

        var domfacade = executionParameters.DomFacade;

        if (value.GetValueType().IsSubtypeOf(ValueType.Node))
        {
            var pointer = value.GetAs<NodeValue<TNode>>().Value;

            if (domfacade.IsAttribute(pointer) || domfacade.IsText(pointer))
                return SequenceFactory.CreateFromValue(AtomicValue.Create(domfacade.GetData(pointer),
                    ValueType.XsString));

            if (domfacade.IsComment(pointer) || domfacade.IsProcessingInstruction(pointer))
                return SequenceFactory.CreateFromValue(AtomicValue.Create(domfacade.GetData(pointer),
                    ValueType.XsString));

            var allTexts = new List<string>();

            Action<TNode>? getTextNodes = null;
            getTextNodes = node =>
            {
                if (domfacade.IsComment(node) || domfacade.IsProcessingInstruction(node))
                    return;

                if (domfacade.IsText(node))
                {
                    allTexts.Add(domfacade.GetData(node));
                    return;
                }

                if (domfacade.IsElement(node) || domfacade.IsDocument(node))
                {
                    var children = domfacade.GetChildNodes(node).ToList();
                    children.ForEach(child => getTextNodes?.Invoke(child));
                }
            };
            getTextNodes(pointer);

            return SequenceFactory.CreateFromValue(AtomicValue.Create(string.Join("", allTexts),
                ValueType.XsUntypedAtomic));
        }

        if (value.GetValueType().IsSubtypeOf(ValueType.Function) &&
            !value.GetValueType().IsSubtypeOf(ValueType.Array))
            //TODO: Create dedicated function and add proper type to string function.
            throw new XPathException("FOTY0013", $"Atomization is not supported for {value.GetValueType()}.");

        if (value.GetValueType().IsSubtypeOf(ValueType.Array))
            throw new NotImplementedException("Implement ArrayValue forst");

        throw new Exception($"Atomizing type {value.GetType()} is not implemented.");
    }

    // TODO: Move all this stuff to the AtomicValue file.
}