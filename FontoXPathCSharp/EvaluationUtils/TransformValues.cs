using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.EvaluationUtils;

public class TransformValues<TNode> where TNode : notnull
{
    public static Iterator<object?> TransformXPathItemToObject(
        AbstractValue value,
        ExecutionParameters<TNode> executionParameters)
    {
        if (value.GetValueType().IsSubtypeOf(ValueType.Map))
            return TransformMapToObject(value.GetAs<MapValue>(), executionParameters);
        if (value.GetValueType().IsSubtypeOf(ValueType.Array))
            return TransformArrayToArray(value.GetAs<ArrayValue<TNode>>(), executionParameters)!;
        if (value.GetValueType().IsSubtypeOf(ValueType.XsQName))
        {
            var qualifiedName = value.GetAs<QNameValue>().GetValue();
            return _ => IteratorResult<object?>.Ready(
                $"Q{{{qualifiedName.NamespaceUri ?? ""}}}{qualifiedName.LocalName}");
        }

        // Make it actual here
        return value.GetValueType() switch
        {
            ValueType.XsDate
                or ValueType.XsTime
                or ValueType.XsDateTime
                or ValueType.XsGYearMonth
                or ValueType.XsGYear
                or ValueType.XsGMonthDay
                or ValueType.XsGMonth
                or ValueType.XsGDay =>
                _ => IteratorResult<object?>.Ready(value.GetAs<DateTimeValue>().Value.ToDateTimeOffset()),
            ValueType.Attribute
                or ValueType.Node
                or ValueType.Element
                or ValueType.DocumentNode
                or ValueType.Text
                or ValueType.ProcessingInstruction
                or ValueType.Comment =>
                _ => IteratorResult<object?>.Ready(value.GetAs<NodeValue<TNode>>().Value),
            _ => _ => IteratorResult<object?>.Ready(value.GetAs<AtomicValue>().GetValue())
        };
    }

    private static Iterator<object?> TransformMapToObject(MapValue map, ExecutionParameters<TNode> executionParameters)
    {
        var mapObj = new Dictionary<string, object?>();
        var i = 0;
        var done = false;
        Iterator<object?>? transformedValueIterator = null;
        return
            _ =>
            {
                if (done) return IteratorResult<object?>.Done();
                while (i < map.KeyValuePairs.Count)
                {
                    // Assume the keys for a map are strings.
                    var key = map.KeyValuePairs[i].Key.GetAs<StringValue>().Value;

                    if (transformedValueIterator == null)
                    {
                        var keyValuePair = map.KeyValuePairs[i];

                        var temp = keyValuePair.Value();
                        if (!temp.IsSingleton() && !temp.IsEmpty())
                            throw new Exception(
                                $"Serialization error: The value of an entry in a map is expected to be a single item or an empty sequence. Use arrays when putting multiple values in a map. The value of the key {keyValuePair.Key.GetAs<StringValue>().Value} holds multiple items");
                        var val = temp.First();

                        if (val == null)
                        {
                            mapObj[key] = null;
                            i++;
                            continue;
                        }

                        transformedValueIterator = TransformXPathItemToObject(
                            val,
                            executionParameters
                        );
                    }

                    var transformedValue = transformedValueIterator(IterationHint.None);
                    transformedValueIterator = null;
                    mapObj[key] = transformedValue.Value;
                    i++;
                }

                done = true;
                return IteratorResult<object?>.Ready(mapObj);
            };
    }

    private static Iterator<object> TransformArrayToArray(ArrayValue<TNode> array,
        ExecutionParameters<TNode> executionParameters)
    {
        var arr = new object[array.Members.Count];
        var i = 0;
        var done = false;
        Iterator<object?>? transformedMemberGenerator = null;
        return
            _ =>
            {
                if (done) return IteratorResult<object>.Done();
                while (i < array.Members.Count)
                {
                    if (transformedMemberGenerator == null)
                    {
                        var temp = array.Members[i]();
                        if (!temp.IsSingleton() && !temp.IsEmpty())
                            throw new Exception(
                                "Serialization error: The value of an entry in an array is expected to be a single item or an empty sequence. Use nested arrays when putting multiple values in an array.");
                        var val = temp.First();

                        if (val == null)
                        {
                            arr[i++] = null;
                            continue;
                        }

                        transformedMemberGenerator = TransformXPathItemToObject(
                            val,
                            executionParameters
                        );
                    }

                    var transformedValue = transformedMemberGenerator(IterationHint.None);
                    transformedMemberGenerator = null;
                    arr[i++] = transformedValue.Value;
                }

                done = true;
                return IteratorResult<object>.Ready(arr);
            };
    }
}