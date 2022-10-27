using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Expressions.DataTypes;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions;

public class SortedSequenceUtils<TNode> where TNode : notnull
{
    private static bool AreNodesEqual(NodeValue<TNode> node1, NodeValue<TNode> node2)
    {
        return node1 == node2 || node1.Value.Equals(node2.Value);
    }

    private static bool IsSameNodeValue(AbstractValue? a, AbstractValue? b)
    {
        if (a == null || b == null) return false;

        if (!a.GetValueType().IsSubtypeOf(ValueType.Node) || !b.GetValueType().IsSubtypeOf(ValueType.Node))
            return false;

        return AreNodesEqual(a.GetAs<NodeValue<TNode>>(), b.GetAs<NodeValue<TNode>>());
    }

    public static ISequence ConcatSortedSequences(Iterator<ISequence> sequences)
    {
        var currentSequence = sequences(IterationHint.None);
        if (currentSequence.IsDone) return SequenceFactory.CreateEmpty();
        Iterator<AbstractValue>? currentIterator = null;
        AbstractValue? previousValue = null;
        return SequenceFactory.CreateFromIterator(
            hint =>
            {
                if (currentSequence.IsDone) return IteratorResult<AbstractValue>.Done();
                currentIterator ??= currentSequence.Value?.GetValue();

                IteratorResult<AbstractValue> value;
                // Scan to the next value
                do
                {
                    value = currentIterator!(hint);
                    if (value.IsDone)
                    {
                        currentSequence = sequences(IterationHint.None);
                        if (currentSequence.IsDone) return value;
                        currentIterator = currentSequence.Value?.GetValue();
                    }
                } while (value.IsDone || IsSameNodeValue(value.Value, previousValue));

                previousValue = value.Value;
                return value;
            }
        );
    }

    public static ISequence MergeSortedSequences(DomFacade<TNode> domFacade, Iterator<ISequence> sequences)
    {
        var allIterators = new List<MappedIterator>();
        // Because the sequences are sorted locally, but unsorted globally, we first need to sort all the iterators.
        // For that, we need to know all of them
        var loadSequences = () =>
        {
            var val = sequences(IterationHint.None);
            while (!val.IsDone)
            {
                var iterator = val.Value!.GetValue();
                var mappedIterator = new MappedIterator(
                    iterator(IterationHint.None),
                    hint => iterator(hint)
                );
                if (!mappedIterator.Current.IsDone) allIterators.Add(mappedIterator);
                val = sequences(IterationHint.None);
            }
        };
        loadSequences();

        AbstractValue? previousNode = null;

        var allSequencesAreSorted = false;
        return SequenceFactory.CreateFromIterator(
            _ =>
            {
                if (!allSequencesAreSorted)
                {
                    allSequencesAreSorted = true;

                    if (
                            allIterators.All(iterator =>
                                iterator.Current.Value!.GetValueType().IsSubtypeOf(ValueType.Node)
                            )
                        )
                        // Sort the iterators initially. We know these iterators return locally sorted items, but we do not know the inter-ordering of these items.
                        allIterators.Sort((iteratorA, iteratorB) =>
                            DocumentOrderUtils<TNode>.CompareNodePositions(
                                domFacade,
                                iteratorA.Current.Value!.GetAs<NodeValue<TNode>>(),
                                iteratorB.Current.Value!.GetAs<NodeValue<TNode>>()
                            )
                        );
                }

                IteratorResult<AbstractValue> consumedValue;
                do
                {
                    if (allIterators.Count == 0) return IteratorResult<AbstractValue>.Done();

                    var consumedIterator = allIterators[0];
                    allIterators.RemoveAt(0);

                    consumedValue = consumedIterator.Current;
                    consumedIterator.Current = consumedIterator.Next(IterationHint.None);
                    if (!consumedValue.Value!.GetValueType().IsSubtypeOf(ValueType.Node))
                        // Sorting does not matter
                        return consumedValue;

                    if (!consumedIterator.Current.IsDone)
                    {
                        // Make the iterators sorted again
                        var low = 0;
                        var high = allIterators.Count - 1;
                        while (low <= high)
                        {
                            var mid = (int)Math.Floor((low + high) / 2.0);
                            var comparisonResult = DocumentOrderUtils<TNode>.CompareNodePositions(
                                domFacade,
                                consumedIterator.Current.Value!.GetAs<NodeValue<TNode>>(),
                                allIterators[mid].Current.Value!.GetAs<NodeValue<TNode>>()
                            );
                            if (comparisonResult == 0)
                            {
                                // The same, this should be 0
                                low = mid;
                                break;
                            }

                            if (comparisonResult > 0)
                            {
                                // After:
                                low = mid + 1;
                                continue;
                            }

                            high = mid - 1;
                        }

                        allIterators.Insert(low, consumedIterator);
                    }
                } while (IsSameNodeValue(consumedValue.Value, previousNode));

                previousNode = consumedValue.Value;
                return consumedValue;
            }
        );
    }

    private class MappedIterator
    {
        public readonly Iterator<AbstractValue> Next;
        public IteratorResult<AbstractValue> Current;

        public MappedIterator(IteratorResult<AbstractValue> current, Iterator<AbstractValue> next)
        {
            Next = next;
            Current = current;
        }
    }
}