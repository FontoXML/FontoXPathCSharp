using System.Xml;
using FontoXPathCSharp.EvaluationUtils;
using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Functions;

public class BuiltInFunctionsSequencesDeepEqual
{
    public static Iterator<BooleanValue> SequenceDeepEqual(
        DynamicContext dynamicContext,
        ExecutionParameters executionParameters,
        StaticContext staticContext,
        ISequence sequence1,
        ISequence sequence2)
    {
        var domFacade = executionParameters.DomFacade;
        var it1 = sequence1.GetValue();
        var it2 = sequence2.GetValue();
        IteratorResult<AbstractValue>? item1 = null;
        IteratorResult<AbstractValue>? item2 = null;
        Iterator<BooleanValue>? comparisonGenerator = null;
        var done = false;
        var textValues1 = new List<AbstractValue>();
        var textValues2 = new List<AbstractValue>();

        return _ =>
        {
            while (!done)
            {
                if (item1 == null) item1 = it1(IterationHint.None);
                item1 = TakeConsecutiveTextValues(item1, textValues1, it1, domFacade);

                if (item2 == null) item2 = it2(IterationHint.None);
                item2 = TakeConsecutiveTextValues(item2, textValues2, it2, domFacade);

                if (textValues1.Count > 0 || textValues2.Count > 0)
                {
                    var textComparisonResult = CompareNormalizedTextNodes(
                        dynamicContext,
                        executionParameters,
                        staticContext,
                        textValues1,
                        textValues2
                    );
                    textValues1.Clear();
                    textValues2.Clear();

                    if (textComparisonResult.Value != null && !textComparisonResult.Value.GetAs<BooleanValue>().Value)
                    {
                        done = true;
                        return textComparisonResult;
                    }

                    // We compare the textNodes so far, we should continue as normal.
                    continue;
                }

                if (item1.IsDone || item2.IsDone)
                {
                    done = true;
                    return IteratorResult<BooleanValue>.Ready(new BooleanValue(item1.IsDone == item2.IsDone));
                }

                if (comparisonGenerator == null)
                    comparisonGenerator = ItemDeepEqual(
                        dynamicContext,
                        executionParameters,
                        staticContext,
                        item1.Value,
                        item2.Value
                    );

                var comparisonResult = comparisonGenerator(IterationHint.None);
                comparisonGenerator = null;
                if (comparisonResult.Value != null && !comparisonResult.Value.GetAs<BooleanValue>().Value)
                {
                    done = true;
                    return comparisonResult;
                }

                // Compare next one
                item1 = null;
                item2 = null;
            }

            return IteratorResult<BooleanValue>.Done();
        };
    }

    private static Iterator<BooleanValue> ItemDeepEqual(
        DynamicContext dynamicContext,
        ExecutionParameters executionParameters,
        StaticContext staticContext,
        AbstractValue item1,
        AbstractValue item2)
    {
        if (item1.GetValueType().IsSubtypeOf(ValueType.XsAnyAtomicType) &&
            item2.GetValueType().IsSubtypeOf(ValueType.XsAnyAtomicType))
            return IteratorUtils.SingleValueIterator(
                AnyAtomicTypeDeepEqual(dynamicContext, executionParameters, staticContext, item1.GetAs<AtomicValue>(),
                    item2.GetAs<AtomicValue>())
            );

        // Maps
        if (item1.GetValueType().IsSubtypeOf(ValueType.Map) &&
            item2.GetValueType().IsSubtypeOf(ValueType.Map))
            return MapTypeDeepEqual(
                dynamicContext,
                executionParameters,
                staticContext,
                item1.GetAs<MapValue>(),
                item2.GetAs<MapValue>()
            );

        // Arrays
        if (item1.GetValueType().IsSubtypeOf(ValueType.Array) &&
            item2.GetValueType().IsSubtypeOf(ValueType.Array))
            return ArrayTypeDeepEqual(
                dynamicContext,
                executionParameters,
                staticContext,
                item1.GetAs<ArrayValue>(),
                item2.GetAs<ArrayValue>()
            );

        // Nodes
        if (item1.GetValueType().IsSubtypeOf(ValueType.Node) &&
            item2.GetValueType().IsSubtypeOf(ValueType.Node))
        {
            // Document nodes
            if (
                item1.GetValueType().IsSubtypeOf(ValueType.DocumentNode) &&
                item2.GetValueType().IsSubtypeOf(ValueType.DocumentNode)
            )
                return NodeDeepEqual(dynamicContext, executionParameters, staticContext, item1, item2);

            // Element nodes, cannot be compared due to missing schema information
            if (
                item1.GetValueType().IsSubtypeOf(ValueType.Element) &&
                item2.GetValueType().IsSubtypeOf(ValueType.Element)
            )
                return ElementNodeDeepEqual(
                    dynamicContext,
                    executionParameters,
                    staticContext,
                    item1,
                    item2
                );

            // Attribute nodes
            if (
                item1.GetValueType().IsSubtypeOf(ValueType.Attribute) &&
                item2.GetValueType().IsSubtypeOf(ValueType.Attribute)
            )
                return AtomicTypeNodeDeepEqual(
                    dynamicContext,
                    executionParameters,
                    staticContext,
                    item1,
                    item2
                );

            // Processing instruction node
            if (
                item1.GetValueType().IsSubtypeOf(ValueType.ProcessingInstruction) &&
                item2.GetValueType().IsSubtypeOf(ValueType.ProcessingInstruction)
            )
                return AtomicTypeNodeDeepEqual(
                    dynamicContext,
                    executionParameters,
                    staticContext,
                    item1,
                    item2
                );

            // Comment nodes
            if (
                item1.GetValueType().IsSubtypeOf(ValueType.Comment) &&
                item2.GetValueType().IsSubtypeOf(ValueType.Comment)
            )
                return AtomicTypeNodeDeepEqual(
                    dynamicContext,
                    executionParameters,
                    staticContext,
                    item1,
                    item2
                );

            // TextNodes
        }

        return IteratorUtils.SingleValueIterator(new BooleanValue(false));
    }

    private static Iterator<BooleanValue> MapTypeDeepEqual(DynamicContext dynamicContext,
        ExecutionParameters executionParameters, StaticContext staticContext, MapValue item1, MapValue item2)
    {
        if (item1.KeyValuePairs.Count != item2.KeyValuePairs.Count)
            return IteratorUtils.SingleValueIterator(new BooleanValue(false));

        return AsyncGenerateEvery(item1.KeyValuePairs, (mapEntry1, _, _) =>
        {
            var mapEntry2 = item2.KeyValuePairs.Find(entry =>
                AnyAtomicTypeDeepEqual(
                    dynamicContext,
                    executionParameters,
                    staticContext,
                    entry.Key.GetAs<AtomicValue>(),
                    mapEntry1.Key.GetAs<AtomicValue>()
                ).Value
            );

            if (mapEntry2.Equals(default(KeyValuePair<AbstractValue, Func<ISequence>>)))
                return IteratorUtils.SingleValueIterator(new BooleanValue(false));

            return SequenceDeepEqual(
                dynamicContext,
                executionParameters,
                staticContext,
                mapEntry1.Value(),
                mapEntry2.Value()
            );
        });
    }

    private static Iterator<BooleanValue> AsyncGenerateEvery<T>(
        List<T> items,
        Func<T, int, List<T>, Iterator<BooleanValue>> callback)
    {
        var i = 0;
        var l = items.Count;
        var done = false;
        Iterator<BooleanValue>? filterGenerator = null;
        return _ =>
        {
            if (!done)
            {
                while (i < l)
                {
                    if (filterGenerator == null) filterGenerator = callback(items[i], i, items);

                    var filterResult = filterGenerator(IterationHint.None);
                    filterGenerator = null;
                    if (filterResult.Value != null && filterResult.Value.Value)
                    {
                        i++;
                        continue;
                    }

                    return IteratorResult<BooleanValue>.Ready(new BooleanValue(false));
                }

                done = true;
                return IteratorResult<BooleanValue>.Ready(new BooleanValue(true));
            }

            return IteratorResult<BooleanValue>.Done();
        };
    }

    private static BooleanValue AnyAtomicTypeDeepEqual(DynamicContext dynamicContext,
        ExecutionParameters executionParameters, StaticContext staticContext, AtomicValue item1, AtomicValue item2)
    {
        if (item1.GetValueType().IsSubTypeOfAny(ValueType.XsDecimal, ValueType.XsFloat) &&
            item2.GetValueType().IsSubTypeOfAny(ValueType.XsDecimal, ValueType.XsFloat))
        {
            var temp1 = TypeCasting.CastToType(item1, ValueType.XsFloat).GetAs<FloatValue>();
            var temp2 = TypeCasting.CastToType(item2, ValueType.XsFloat).GetAs<FloatValue>();
            return new BooleanValue(temp1.Equals(temp2));
        }

        if (item1.GetValueType().IsSubTypeOfAny(ValueType.XsDecimal, ValueType.XsFloat, ValueType.XsDouble) &&
            item2.GetValueType().IsSubTypeOfAny(ValueType.XsDecimal, ValueType.XsFloat, ValueType.XsDouble))
        {
            var temp1 = TypeCasting.CastToType(item1, ValueType.XsDouble).GetAs<DoubleValue>();
            var temp2 = TypeCasting.CastToType(item2, ValueType.XsDouble).GetAs<DoubleValue>();
            return new BooleanValue(temp1.Equals(temp2));
        }

        if (item1.GetValueType().IsSubtypeOf(ValueType.XsQName) && item2.GetValueType().IsSubtypeOf(ValueType.XsQName))
        {
            var temp1 = item1.GetAs<QNameValue>();
            var temp2 = item2.GetAs<QNameValue>();

            return new BooleanValue(temp1.Value.NamespaceUri == temp2.Value.NamespaceUri &&
                                    temp1.Value.LocalName == temp2.Value.LocalName);
        }

        if (item1.GetValueType().IsSubTypeOfAny(ValueType.XsDateTime, ValueType.XsDate, ValueType.XsTime,
                ValueType.XsGYearMonth, ValueType.XsGYear, ValueType.XsGMonthDay, ValueType.XsGMonth,
                ValueType.XsGDay) &&
            item2.GetValueType().IsSubTypeOfAny(ValueType.XsDateTime, ValueType.XsDate, ValueType.XsTime,
                ValueType.XsGYearMonth, ValueType.XsGYear, ValueType.XsGMonthDay, ValueType.XsGMonth, ValueType.XsGDay)
           )
            throw new NotImplementedException("Comparison between dates/times not implemented yet");

        return new BooleanValue(item1.Equals(item2));
    }

    private static Iterator<BooleanValue> ArrayTypeDeepEqual(DynamicContext dynamicContext,
        ExecutionParameters executionParameters, StaticContext staticContext, ArrayValue item1, ArrayValue item2)
    {
        if (item1.Members.Count != item2.Members.Count)
            return IteratorUtils.SingleValueIterator(new BooleanValue(false));

        return AsyncGenerateEvery(item1.Members, (arrayEntry1, index, _) =>
        {
            var arrayEntry2 = item2.Members[index];
            return SequenceDeepEqual(
                dynamicContext,
                executionParameters,
                staticContext,
                arrayEntry1(),
                arrayEntry2()
            );
        });
    }

    private static Iterator<BooleanValue> AtomicTypeNodeDeepEqual(DynamicContext dynamicContext,
        ExecutionParameters executionParameters, StaticContext staticContext, AbstractValue item1, AbstractValue item2)
    {
        var namesAreEqualResultGenerator = SequenceDeepEqual(
            dynamicContext,
            executionParameters,
            staticContext,
            BuiltInFunctionsNode.FnNodeName(
                dynamicContext,
                executionParameters,
                staticContext,
                SequenceFactory.CreateFromValue(item1)
            ),
            BuiltInFunctionsNode.FnNodeName(
                dynamicContext,
                executionParameters,
                staticContext,
                SequenceFactory.CreateFromValue(item2)
            )
        );
        var done = false;
        return _ =>
        {
            if (done) return IteratorResult<BooleanValue>.Done();

            var namesAreEqualResult = namesAreEqualResultGenerator(IterationHint.None);
            if (!namesAreEqualResult.IsDone)
                if (namesAreEqualResult.Value != null && namesAreEqualResult.Value.Value == false)
                {
                    done = true;
                    return namesAreEqualResult;
                }

            // Assume here that a node always atomizes to a singlevalue. This will not work
            // anymore when schema support will be imlemented.
            return IteratorResult<BooleanValue>.Ready(
                AnyAtomicTypeDeepEqual(
                    dynamicContext,
                    executionParameters,
                    staticContext,
                    Atomize.AtomizeSingleValue(item1, executionParameters).First()!.GetAs<AtomicValue>(),
                    Atomize.AtomizeSingleValue(item2, executionParameters).First()!.GetAs<AtomicValue>()
                )
            );
        };
    }

    private static Iterator<BooleanValue> ElementNodeDeepEqual(DynamicContext dynamicContext,
        ExecutionParameters executionParameters, StaticContext staticContext, AbstractValue item1, AbstractValue item2)
    {
        throw new NotImplementedException("ElementNodeDeepEqual not implemented yet");
    }

    private static Iterator<BooleanValue> NodeDeepEqual(DynamicContext dynamicContext,
        ExecutionParameters executionParameters, StaticContext staticContext, AbstractValue item1, AbstractValue item2)
    {
        throw new NotImplementedException("NodeDeepEqual not implemented yet");
    }

    private static IteratorResult<BooleanValue> CompareNormalizedTextNodes(DynamicContext dynamicContext,
        ExecutionParameters executionParameters, StaticContext staticContext, List<AbstractValue> textValues1,
        List<AbstractValue> textValues2)
    {
        var atomicValues = new[] { textValues1, textValues2 }.Select(textValues =>
        {
            var value = textValues.Aggregate("", (wholeValue, textValue) =>
                wholeValue + Atomize.AtomizeSingleValue(textValue, executionParameters).First().GetAs<StringValue>()
                    .Value);

            return Atomize.CreateAtomicValue(value, ValueType.XsString);
        }).ToArray();

        return IteratorResult<BooleanValue>.Ready(
            AnyAtomicTypeDeepEqual(dynamicContext, executionParameters,
                staticContext, atomicValues[0], atomicValues[1])
        );
    }

    private static IteratorResult<AbstractValue> TakeConsecutiveTextValues(IteratorResult<AbstractValue> item,
        List<AbstractValue> textValues, Iterator<AbstractValue> iterator, XmlNode domFacade)
    {
        throw new NotImplementedException("TakeConsecutiveTextValues not implemented yet");
    }
}