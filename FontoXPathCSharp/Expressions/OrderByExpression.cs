using FontoXPathCSharp.EvaluationUtils;
using FontoXPathCSharp.Expressions.Operators.Compares;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions;

public record OrderSpecs<TNode>(AbstractExpression<TNode> Expression, bool IsAscending, bool IsEmptyLeast)
    where TNode : notnull;

public class OrderByExpression<TNode> : FlworExpression<TNode> where TNode : notnull
{
    private readonly OrderSpecs<TNode>[] _orderSpecs;

    public OrderByExpression(OrderSpecs<TNode>[] orderSpecs, AbstractExpression<TNode> returnExpression) : base(
        new Specificity(),
        new[] { returnExpression }.Concat(orderSpecs.Select(spec => spec.Expression)).ToArray(),
        new OptimizationOptions(false),
        returnExpression)
    {
        _orderSpecs = orderSpecs;
    }

    public override ISequence DoFlworExpression(
        DynamicContext dynamicContext,
        Iterator<DynamicContext> dynamicContextIterator,
        ExecutionParameters<TNode> executionParameters,
        Func<Iterator<DynamicContext>, ISequence> createReturnSequence)
    {
        // More than one order spec is not supported for now.
        if (_orderSpecs.Length > 1)
            throw new NotImplementedException("More than one order spec is not supported for the order by clause.");

        var dynamicContexts = new List<DynamicContext?>();

        var hasValues = false;
        List<AbstractValue?> values;
        List<int> indices;

        Iterator<AbstractValue>? returnValueIterator = null;

        var orderSpec = _orderSpecs[0];

        return SequenceFactory.CreateFromIterator(_ =>
            {
                if (!hasValues)
                {
                    var iteratorResult = dynamicContextIterator(IterationHint.None);
                    while (!iteratorResult.IsDone)
                    {
                        dynamicContexts.Add(iteratorResult.Value);
                        iteratorResult = dynamicContextIterator(IterationHint.None);
                    }

                    // Evaluate order specs. Limited to only one order spec for now.
                    var evaluatedOrderSpecs = dynamicContexts.Select(
                        dynamicContextForEvaluation =>
                            orderSpec.Expression.Evaluate(
                                dynamicContextForEvaluation,
                                executionParameters
                            )
                    ).ToArray();

                    // Atomize
                    // Atomization is applied to the result of the expression in each order spec.
                    // If the result of atomization is neither a single atomic value nor an empty sequence
                    // a type error is raised [err:XPTY0004].
                    var atomizedSequences = evaluatedOrderSpecs.Select(
                        evaluatedOrderSpec => Atomize.AtomizeSequence(evaluatedOrderSpec, executionParameters)
                    ).ToArray();
                    if (atomizedSequences.Any(val => !val.IsEmpty() && !val.IsSingleton()))
                        throw new XPathException(
                            "XPTY0004",
                            "Order by only accepts empty or singleton sequences"
                        );

                    // Switch to values instead of sequences as we now know we're dealing with singletons only.
                    values = atomizedSequences.Select(seq => seq.FirstOrDefault()).ToList();

                    // Casting values
                    // If the value of an order spec has the dynamic type xs:untypedAtomic, it is cast to the type xs:string.
                    values = values.Select(value =>
                    {
                        if (value == null) return value;

                        if (ValueType.XsUntypedAtomic.IsSubtypeOf(value.GetValueType()))
                            return value.CastToType(ValueType.XsString);

                        return value;
                    }).ToList();

                    // If the resulting sequence contains values that are instances of more than one primitive type, then:
                    // 1. If each value is an instance of one of the types xs:string or xs:anyURI, then all values are cast to type xs:string.
                    // 2. If each value is an instance of one of the types xs:decimal or xs:float, then all values are cast to type xs:float.
                    // 3. If each value is an instance of one of the types xs:decimal, xs:float, or xs:double, then all values are cast to type xs:double.
                    // 4. Otherwise, a type error is raised [err:XPTY0004].
                    var firstPrimitiveType = GetFirstPrimitiveType(values);

                    if (firstPrimitiveType != null)
                    {
                        values = CommonTypeUtils.ConvertItemsToCommonType(values)!.ToList();

                        if (values == null)
                            throw new XPathException(
                                "XPTY0004",
                                "Could not cast values to a common primitive type."
                            );
                    }

                    // Sort values
                    var numberOfValues = values.Count;
                    indices = values.Select((_, index) => index).ToList();
                    for (var startIndex = 0; startIndex < numberOfValues; startIndex++)
                    {
                        if (startIndex + 1 == numberOfValues) continue;

                        for (var i = startIndex; i >= 0; i--)
                        {
                            var firstItemIndex = i;
                            var secondItemIndex = i + 1;

                            if (secondItemIndex == numberOfValues) continue;

                            var w = values[indices[firstItemIndex]];
                            var v = values[indices[secondItemIndex]];

                            if (v == null && w == null) continue;

                            if (orderSpec.IsEmptyLeast)
                            {
                                // W is empty and thus is already in the right spot
                                if (w == null) continue;

                                if (v == null)
                                {
                                    // V is an empty sequence, thus swap indices in index array
                                    (indices[firstItemIndex], indices[secondItemIndex]) = (indices[secondItemIndex],
                                        indices[firstItemIndex]);

                                    continue;
                                }

                                if (float.IsNaN(v.GetAs<FloatValue>().Value) &&
                                    !float.IsNaN(w.GetAs<FloatValue>().Value))
                                {
                                    // V is NaN, thus swap indices in index array
                                    (indices[firstItemIndex], indices[secondItemIndex]) = (indices[secondItemIndex],
                                        indices[firstItemIndex]);
                                    continue;
                                }
                            }
                            else
                            {
                                // V is already empty and thus is in the right spot
                                if (v == null) continue;

                                if (w == null)
                                {
                                    (indices[firstItemIndex], indices[secondItemIndex]) = (indices[secondItemIndex],
                                        indices[firstItemIndex]);
                                    continue;
                                }

                                if (float.IsNaN(w.GetAs<FloatValue>().Value) &&
                                    !float.IsNaN(v.GetAs<FloatValue>().Value))
                                {
                                    (indices[firstItemIndex], indices[secondItemIndex]) = (indices[secondItemIndex],
                                        indices[firstItemIndex]);
                                    continue;
                                }
                            }

                            if (ValueCompare<TNode>.PerformValueCompare(CompareType.GreaterThan, w, v, dynamicContext))
                                (indices[firstItemIndex], indices[secondItemIndex]) =
                                    (indices[secondItemIndex], indices[firstItemIndex]);
                            // Else done
                        }
                    }

                    // For the purpose of determining their relative position in the ordering sequence, the greater-than relationship between two order spec values W and V is defined as follows:

                    // When the order spec specifies empty least, the following rules are applied in order:
                    // 1. If V is an empty sequence and W is not an empty sequence, then W greater-than V is true.
                    // 2. If V is NaN and W is neither NaN nor an empty sequence, then W greater-than V is true.
                    // 3. If a specific collation C is specified, and V and W are both of type xs:string or are convertible to xs:string by subtype substitution and/or type promotion, then:
                    // 4. If fn:compare(V, W, C) is less than zero, then W greater-than V is true; otherwise W greater-than V is false.
                    // 5. If none of the above rules apply, then:
                    // 6. If W gt V is true, then W greater-than V is true; otherwise W greater-than V is false.

                    // When the  order spec specifies empty greatest, the following rules are applied in order:
                    // 1. If W is an empty sequence and V is not an empty sequence, then W greater-than V is true.
                    // 2. If W is NaN and V is neither NaN nor an empty sequence, then W greater-than V is true.
                    // 3. If a specific collation C is specified, and V and W are both of type xs:string or are convertible to xs:string by subtype substitution and/or type promotion, then:
                    // 4. If fn:compare(V, W, C) is less than zero, then W greater-than V is true; otherwise W greater-than V is false.
                    // 5. If none of the above rules apply, then:
                    // 6. If W gt V is true, then W greater-than V is true; otherwise W greater-than V is false.

                    // When the order spec specifies neither empty least nor empty greatest, the default order for empty sequences in the static context determines whether the rules for empty least or empty greatest are used.

                    var currentIndex = orderSpec.IsAscending ? 0 : values.Count - 1;

                    // We have an order here.
                    // Order the dynamic contexts by the same order and pass them in an iterator to the createReturnSequence
                    var returnValue = createReturnSequence(
                        _ =>
                        {
                            if (orderSpec.IsAscending)
                            {
                                if (currentIndex >= values.Count) return IteratorResult<DynamicContext>.Done();

                                return IteratorResult<DynamicContext>.Ready(dynamicContexts[indices[currentIndex++]]!);
                            }

                            if (currentIndex < 0) return IteratorResult<DynamicContext>.Done();

                            return IteratorResult<DynamicContext>.Ready(dynamicContexts[indices[currentIndex--]]!);
                        });

                    returnValueIterator = returnValue.GetValue();

                    hasValues = true;
                }

                return returnValueIterator(IterationHint.None);
            }
        );
    }

    private static ValueType? GetFirstPrimitiveType(IEnumerable<AbstractValue?> values)
    {
        var firstActualValue = values.FirstOrDefault(value => value != null);

        return firstActualValue != null
            ? TypeHelpers.GetPrimitiveTypeName(firstActualValue.GetValueType())
            : null;
    }
}