using FontoXPathCSharp.EvaluationUtils;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.Functions;

public static class BuiltInFunctionsSequences<TNode> where TNode : notnull
{
    private static readonly FunctionSignature<ISequence, TNode> FnCount = (_, _, _, args) =>
    {
        var hasPassed = false;
        return SequenceFactory.CreateFromIterator(_ =>
        {
            if (hasPassed) return IteratorResult<AbstractValue>.Done();

            hasPassed = true;
            return IteratorResult<AbstractValue>.Ready(new IntegerValue(args[0].GetLength(), ValueType.XsInt));
        }, 1);
    };

    private static readonly FunctionSignature<ISequence, TNode> FnMax = (_, _, _, args) =>
    {
        var sequence = args[0];
        if (sequence.IsEmpty()) return sequence;

        var items = CastItemsForMinMax(sequence.GetAllValues());

        // Use first element in array as initial value
        return SequenceFactory.CreateFromValue(
            items.Aggregate((max, item) =>
                Convert.ToDecimal(((AtomicValue)max).GetValue()) < Convert.ToDecimal(((AtomicValue)item).GetValue())
                    ? item
                    : max)
        );
    };

    private static readonly FunctionSignature<ISequence, TNode> FnMin = (_, _, _, args) =>
    {
        var sequence = args[0];
        if (sequence.IsEmpty()) return sequence;

        var items = CastItemsForMinMax(sequence.GetAllValues());

        // Use first element in array as initial value
        return SequenceFactory.CreateFromValue(
            items.Aggregate((max, item) =>
                Convert.ToDecimal(((AtomicValue)max).GetValue()) > Convert.ToDecimal(((AtomicValue)item).GetValue())
                    ? item
                    : max)
        );
    };

    private static readonly FunctionSignature<ISequence, TNode> FnSum = (_, _, _, args) =>
    {
        var sequence = args[0];
        var zero = args.Length > 1
            ? args[1]
            : SequenceFactory.CreateFromValue(AtomicValue.Create(0, ValueType.XsInteger));

        // TODO: throw FORG0006 if the items contain both yearMonthDurations and dayTimeDurations
        if (sequence.IsEmpty()) return zero;

        var items = CastUntypedItemsToDouble(sequence.GetAllValues());
        items = CommonTypeUtils.ConvertItemsToCommonType(items)!;
        if (items == null)
            throw new XPathException("FORG0006", "Incompatible types to be converted to a common type");

        if (items.Any(item => !item.GetValueType().IsSubtypeOf(ValueType.XsNumeric)))
            throw new XPathException("FORG0006", "Items passed to fn:sum are not all numeric.");

        if (items.All(item => item.GetValueType().IsSubtypeOf(ValueType.XsInteger)))
            return SequenceFactory.CreateFromValue(
                AtomicValue.Create(
                    items.Select(v => v.GetAs<IntegerValue>().Value).Sum(),
                    ValueType.XsInteger)
            );

        if (items.All(item => item.GetValueType().IsSubtypeOf(ValueType.XsDouble)))
            return SequenceFactory.CreateFromValue(
                AtomicValue.Create(
                    items.Select(v => v.GetAs<DoubleValue>().Value).Sum(),
                    ValueType.XsDouble)
            );

        if (items.All(item => item.GetValueType().IsSubtypeOf(ValueType.XsDecimal)))
            return SequenceFactory.CreateFromValue(
                AtomicValue.Create(
                    items.Select(v => v.GetAs<DecimalValue>().Value).Sum(),
                    ValueType.XsDecimal)
            );

        var floatResult = items.Select(v => v.GetAs<FloatValue>().Value).Sum();
        return SequenceFactory.CreateFromValue(AtomicValue.Create(floatResult, ValueType.XsFloat));
    };

    private static readonly FunctionSignature<ISequence, TNode> FnAvg = (_, _, _, args) =>
    {
        var sequence = args[0];
        if (sequence.IsEmpty()) return sequence;


        // TODO: throw FORG0006 if the items contain both yearMonthDurations and dayTimeDurations
        var items = CastUntypedItemsToDouble(sequence.GetAllValues());
        items = CommonTypeUtils.ConvertItemsToCommonType(items)!;
        if (items == null) throw new XPathException("FORG0006", "Incompatible types to be converted to a common type");

        if (!items.All(item => item.GetValueType().IsSubtypeOf(ValueType.XsNumeric)))
            throw new XPathException("FORG0006", "Items passed to fn:avg are not all numeric.");

        var resultValue = items.Aggregate(0.0, (sum, item) =>
            sum + Convert.ToDouble(((AtomicValue)item).GetValue())) / items.Length;

        if (items.All(item => item.GetValueType().IsSubtypeOf(ValueType.XsInteger)
                              || item.GetValueType().IsSubtypeOf(ValueType.XsDouble)))
            return SequenceFactory.CreateFromValue(AtomicValue.Create(resultValue, ValueType.XsDouble));

        if (items.All(item => item.GetValueType().IsSubtypeOf(ValueType.XsDecimal)))
            return SequenceFactory.CreateFromValue(AtomicValue.Create(resultValue, ValueType.XsDecimal));

        return SequenceFactory.CreateFromValue(AtomicValue.Create(resultValue, ValueType.XsInteger));
    };


    private static readonly FunctionSignature<ISequence, TNode> FnZeroOrOne = (_, _, _, args) =>
    {
        var arg = args[0];
        if (!arg.IsEmpty() && !arg.IsSingleton())
            throw new XPathException("FORG0003", "The argument passed to fn:zero-or-one contained more than one item.");

        return arg;
    };

    private static readonly FunctionSignature<ISequence, TNode> FnExists = (_, _, _, args) =>
        SequenceFactory.CreateFromValue(new BooleanValue(!args[0].IsEmpty()));

    private static readonly FunctionSignature<ISequence, TNode> FnEmpty = (_, _, _, args) =>
        SequenceFactory.CreateFromValue(new BooleanValue(args[0].IsEmpty()));

    private static readonly FunctionSignature<ISequence, TNode> FnDeepEqual =
        (dynamicContext, executionParameters, staticContext, args) =>
        {
            var hasPassed = false;
            var deepEqualityIterator = BuiltInFunctionsSequencesDeepEqual<TNode>.SequenceDeepEqual(
                dynamicContext!,
                executionParameters,
                staticContext!,
                args[0],
                args[1]
            );

            return SequenceFactory.CreateFromIterator(
                _ =>
                {
                    if (hasPassed) return IteratorResult<BooleanValue>.Done();

                    var result = deepEqualityIterator(IterationHint.None);
                    if (result.IsDone) return result;

                    hasPassed = true;
                    return IteratorResult<BooleanValue>.Ready(result.Value!);
                }
            );
        };

    private static readonly FunctionSignature<ISequence, TNode> FnFilter =
        (dynamicContext, executionParameters, staticContext, args) =>
        {
            var sequence = args[0];

            if (sequence.IsEmpty()) return sequence;

            var callbackSequence = args[1];

            var callbackFn = callbackSequence.First() as FunctionValue<ISequence, TNode>;
            var callbackArgumentTypes = callbackFn!.ArgumentTypes;
            if (callbackArgumentTypes.Length != 1)
                throw new XPathException("XPTY0004", "Signature of function passed to fn:filter is incompatible.");

            return sequence.Filter((item,_,_) =>
            {
                // Transform argument
                var transformedArgument = ArgumentHelper<TNode>.PerformFunctionConversion(
                    callbackArgumentTypes[0],
                    SequenceFactory.CreateFromValue(item),
                    executionParameters,
                    "fn:filter",
                    false
                );
                var functionCallResult = callbackFn.Value(
                    dynamicContext,
                    executionParameters,
                    staticContext,
                    transformedArgument
                );
                if (!functionCallResult.IsSingleton() ||
                    !functionCallResult.First()!.GetValueType().IsSubtypeOf(ValueType.XsBoolean)
                   )
                    throw new XPathException("XPTY0004", "Signature of function passed to fn:filter is incompatible.");
                return functionCallResult.First().GetAs<BooleanValue>().Value;
            });
        };


    private static readonly FunctionSignature<ISequence, TNode> FnForEach =
        (dynamicContext, executionParameters, staticContext, args) =>
        {
            var sequence = args[0];
            if (sequence.IsEmpty()) return sequence;

            var callbackSequence = args[1];
            var callbackFn = callbackSequence.First() as FunctionValue<ISequence, TNode>;
            var callbackArgumentTypes = callbackFn!.ArgumentTypes;
            if (callbackArgumentTypes.Length != 1)
                throw new XPathException("XPTY0004", "Signature of function passed to fn:for-each is incompatible.");

            var outerIterator = sequence.GetValue();
            Iterator<AbstractValue>? innerIterator = null;
            return SequenceFactory.CreateFromIterator(
                hint =>
                {
                    while (true)
                    {
                        if (innerIterator == null)
                        {
                            var item = outerIterator(IterationHint.None);

                            if (item.IsDone) return item;

                            var transformedArgument = ArgumentHelper<TNode>.PerformFunctionConversion(
                                callbackArgumentTypes[0],
                                SequenceFactory.CreateFromValue(item.Value),
                                executionParameters,
                                "fn:for-each",
                                false
                            );
                            var nextSequence = callbackFn.Value(
                                dynamicContext,
                                executionParameters,
                                staticContext,
                                transformedArgument
                            );

                            innerIterator = nextSequence.GetValue();
                        }

                        var entry = innerIterator(hint);
                        if (!entry.IsDone) return entry;
                        innerIterator = null;
                    }
                });
        };

    private static readonly FunctionSignature<ISequence, TNode> FnFoldLeft =
        (dynamicContext, executionParameters, staticContext, args) =>
        {
            var sequence = args[0];
            if (sequence.IsEmpty()) return sequence;

            var seed = args[1];
            var callbackSequence = args[2];

            var callbackFn = callbackSequence.First() as FunctionValue<ISequence, TNode>;
            var callbackArgumentTypes = callbackFn!.ArgumentTypes;

            if (callbackArgumentTypes.Length != 2)
                throw new XPathException("XPTY0004", "Signature of function passed to fn:fold-left is incompatible.");

            return sequence.MapAll(values =>
                values.Reduce(seed, (previous, current, _) =>
                {
                    var previousArg = ArgumentHelper<TNode>.PerformFunctionConversion(
                        callbackArgumentTypes[0],
                        previous,
                        executionParameters,
                        "fn:fold-left",
                        false
                    );
                    var currentArg = ArgumentHelper<TNode>.PerformFunctionConversion(
                        callbackArgumentTypes[1],
                        SequenceFactory.CreateFromValue(current),
                        executionParameters,
                        "fn:fold-left",
                        false
                    );
                    return callbackFn.Value(
                        dynamicContext,
                        executionParameters,
                        staticContext,
                        previousArg,
                        currentArg
                    );
                }));
        };

    private static readonly FunctionSignature<ISequence, TNode> FnFoldRight =
        (dynamicContext, executionParameters, staticContext, args) =>
        {
            var sequence = args[0];
            if (sequence.IsEmpty()) return sequence;

            var seed = args[1];
            var callbackSequence = args[2];

            var callbackFn = callbackSequence.First() as FunctionValue<ISequence, TNode>;
            var callbackArgumentTypes = callbackFn!.ArgumentTypes;

            if (callbackArgumentTypes.Length != 2)
                throw new XPathException("XPTY0004", "Signature of function passed to fn:fold-right is incompatible.");

            return sequence.MapAll(values =>
                values.ReduceRight(seed, (previous, current, _) =>
                {
                    var previousArg = ArgumentHelper<TNode>.PerformFunctionConversion(
                        callbackArgumentTypes[0],
                        previous,
                        executionParameters,
                        "fn:fold-left",
                        false
                    );
                    var currentArg = ArgumentHelper<TNode>.PerformFunctionConversion(
                        callbackArgumentTypes[1],
                        SequenceFactory.CreateFromValue(current),
                        executionParameters,
                        "fn:fold-left",
                        false
                    );
                    return callbackFn.Value(
                        dynamicContext,
                        executionParameters,
                        staticContext,
                        previousArg,
                        currentArg
                    );
                }));
        };

    public static readonly BuiltinDeclarationType<TNode>[] Declarations =
    {
        new(new[]
            {
                new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrOne)
            },
            FnCount,
            "count",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsInteger, SequenceMultiplicity.ExactlyOne)
        ),

        new(new[]
            {
                new ParameterType(ValueType.XsAnyAtomicType, SequenceMultiplicity.ZeroOrMore)
            },
            FnAvg,
            "avg",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsAnyAtomicType, SequenceMultiplicity.ZeroOrOne)
        ),

        new(new[]
            {
                new ParameterType(ValueType.XsAnyAtomicType, SequenceMultiplicity.ZeroOrMore)
            },
            FnMax,
            "max",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsAnyAtomicType, SequenceMultiplicity.ZeroOrOne)
        ),

        new(new[]
            {
                new ParameterType(ValueType.XsAnyAtomicType, SequenceMultiplicity.ZeroOrMore)
            },
            FnMin,
            "min",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsAnyAtomicType, SequenceMultiplicity.ZeroOrOne)
        ),

        new(new[]
            {
                new ParameterType(ValueType.XsAnyAtomicType, SequenceMultiplicity.ZeroOrMore)
            },
            (dynamicContext, executionParameters, staticContext, sequences) =>
                FnSum(
                    dynamicContext,
                    executionParameters,
                    staticContext,
                    sequences.Concat(new[]
                        { SequenceFactory.CreateFromValue(AtomicValue.Create(0, ValueType.XsInteger)) }).ToArray()),
            "sum",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsAnyAtomicType, SequenceMultiplicity.ExactlyOne)
        ),

        new(new[]
            {
                new ParameterType(ValueType.XsAnyAtomicType, SequenceMultiplicity.ZeroOrMore),
                new ParameterType(ValueType.XsAnyAtomicType, SequenceMultiplicity.ZeroOrOne)
            },
            FnSum,
            "sum",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsAnyAtomicType, SequenceMultiplicity.ZeroOrOne)
        ),

        new(new[]
            {
                new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrMore)
            },
            FnZeroOrOne,
            "zero-or-one",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.Item, SequenceMultiplicity.ZeroOrOne)
        ),

        new(new[]
            {
                new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrMore)
            },
            FnEmpty,
            "empty",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsBoolean, SequenceMultiplicity.ExactlyOne)
        ),

        new(new[]
            {
                new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrMore)
            },
            FnExists,
            "exists",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsBoolean, SequenceMultiplicity.ExactlyOne)
        ),

        new(new[]
            {
                new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrMore),
                new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrMore)
            },
            FnDeepEqual,
            "deep-equal",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsBoolean, SequenceMultiplicity.ExactlyOne)
        ),

        new(new[]
            {
                new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrMore),
                new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrMore),
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)
            },
            (_, _, _, _) => throw new XPathException("FOCH0002", "No collations are supported"),
            "deep-equal",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsBoolean, SequenceMultiplicity.ExactlyOne)
        ),

        new(new[]
            {
                new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrMore),
                new ParameterType(ValueType.Function, SequenceMultiplicity.ExactlyOne)
            },
            FnFilter,
            "filter",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.Item, SequenceMultiplicity.ZeroOrMore)
        ),

        new(new[]
            {
                new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrMore),
                new ParameterType(ValueType.Function, SequenceMultiplicity.ExactlyOne)
            },
            FnForEach,
            "for-each",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.Item, SequenceMultiplicity.ZeroOrMore)
        ),

        new(new[]
            {
                new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrMore),
                new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrMore),
                new ParameterType(ValueType.Function, SequenceMultiplicity.ExactlyOne)
            },
            FnFoldLeft,
            "fold-left",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.Item, SequenceMultiplicity.ZeroOrMore)
        ),

        new(new[]
            {
                new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrMore),
                new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrMore),
                new ParameterType(ValueType.Function, SequenceMultiplicity.ExactlyOne)
            },
            FnFoldRight,
            "fold-right",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.Item, SequenceMultiplicity.ZeroOrMore)
        )
    };

    private static AbstractValue[] CastItemsForMinMax(AbstractValue[] items)
    {
        // Values of type xs:untypedAtomic in $arg are cast to xs:double.
        items = CastUntypedItemsToDouble(items);

        if (items.Any(item =>
                (item.GetValueType() == ValueType.XsDouble && double.IsNaN(item.GetAs<DoubleValue>().Value))
                || (item.GetValueType() == ValueType.XsFloat && float.IsNaN(item.GetAs<FloatValue>().Value))))
            return new AbstractValue[] { AtomicValue.Create(double.NaN, ValueType.XsDouble) };

        var convertResult = CommonTypeUtils.ConvertItemsToCommonType(items);

        if (convertResult == null)
            throw new XPathException("FORG0006", "Incompatible types to be converted to a common type");

        return convertResult!;
    }

    private static AbstractValue[] CastUntypedItemsToDouble(AbstractValue[] items)
    {
        return items.Select(item =>
            item.GetValueType().IsSubtypeOf(ValueType.XsUntypedAtomic)
                ? item.CastToType(ValueType.XsDouble)!
                : item).ToArray();
    }
}