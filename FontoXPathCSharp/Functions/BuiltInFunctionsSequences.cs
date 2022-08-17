using FontoXPathCSharp.EvaluationUtils;
using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Functions;

public static class BuiltInFunctionsSequences<TNode>
{
    private static readonly FunctionSignature<ISequence, TNode> FnCount = (_, _, _, args) =>
    {
        var hasPassed = false;
        return SequenceFactory.CreateFromIterator(_ =>
        {
            if (hasPassed) return IteratorResult<AbstractValue>.Done();

            hasPassed = true;
            return IteratorResult<AbstractValue>.Ready(new IntValue(args[0].GetLength()));
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

    private static readonly FunctionSignature<ISequence, TNode> FnAvg = (_, _, _, args) =>
    {
        var sequence = args[0];
        if (sequence.IsEmpty()) return sequence;
        

        // TODO: throw FORG0006 if the items contain both yearMonthDurations and dayTimeDurations
        var items = CastUntypedItemsToDouble(sequence.GetAllValues());
        items = CommonTypeUtils.ConvertItemsToCommonType(items);
        if (items == null) {
            throw new XPathException("FORG0006" ,"Incompatible types to be converted to a common type");
        }

        if (!items.All((item) => item.GetValueType().IsSubtypeOf(ValueType.XsNumeric))) {
            throw new XPathException("FORG0006" ,"Items passed to fn:avg are not all numeric.");
        }

        var resultValue = items.Aggregate(0.0, (sum, item) => 
            sum + Convert.ToDouble(((AtomicValue)item).GetValue())) / items.Length;

        if (items.All(item => (
                item.GetValueType().IsSubtypeOf(ValueType.XsInteger) ||
                item.GetValueType().IsSubtypeOf(ValueType.XsDouble)
            ))
        ) {
            return SequenceFactory.CreateFromValue(AtomicValue.Create(resultValue, ValueType.XsDouble));
        }

        if (items.All((item) => {
                return item.GetValueType().IsSubtypeOf(ValueType.XsDecimal);
            })
        ) {
            return SequenceFactory.CreateFromValue(AtomicValue.Create(resultValue, ValueType.XsDecimal));
        }

        return SequenceFactory.CreateFromValue(AtomicValue.Create(resultValue, ValueType.XsInteger));
        
    };

    private static AbstractValue[] CastItemsForMinMax(AbstractValue[] items)
    {
        // Values of type xs:untypedAtomic in $arg are cast to xs:double.
        items = CastUntypedItemsToDouble(items);

        if (items.Any(item => double.IsNaN(item.GetAs<DoubleValue>().Value) ||
                              float.IsNaN(item.GetAs<FloatValue>().Value)))
        {
            return new[] { AtomicValue.Create(double.NaN, ValueType.XsDouble) };
        }

        var convertResult = CommonTypeUtils.ConvertItemsToCommonType(items);

        if (convertResult == null)
        {
            throw new XPathException("FORG0006", "Incompatible types to be converted to a common type");
        }

        return convertResult!;
    }

    private static AbstractValue[] CastUntypedItemsToDouble(AbstractValue[] items)
    {
        return items.Select(item =>
            item.GetValueType().IsSubtypeOf(ValueType.XsUntypedAtomic)
                ? item.CastToType(ValueType.XsDouble)
                : item).ToArray();
    }


    private static readonly FunctionSignature<ISequence, TNode> FnZeroOrOne = (_, _, _, args) =>
    {
        var arg = args[0];
        if (!arg.IsEmpty() && !arg.IsSingleton())
        {
            arg.GetAllValues().ToList().ForEach(Console.WriteLine);
            throw new XPathException("FORG0003", "The argument passed to fn:zero-or-one contained more than one item.");
        }

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
                dynamicContext,
                executionParameters,
                staticContext,
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


    public static readonly BuiltinDeclarationType<TNode>[] Declarations =
    {
        new(new[] { new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrOne) },
            FnCount, "count",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsInteger, SequenceMultiplicity.ExactlyOne)),
        new(new[] { new ParameterType(ValueType.XsAnyAtomicType, SequenceMultiplicity.ZeroOrMore) },
            FnAvg, "avg",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsAnyAtomicType, SequenceMultiplicity.ZeroOrOne)),
        new(new[] { new ParameterType(ValueType.XsAnyAtomicType, SequenceMultiplicity.ZeroOrMore) },
            FnMax, "max",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsAnyAtomicType, SequenceMultiplicity.ZeroOrOne)),
        new(new[] { new ParameterType(ValueType.XsAnyAtomicType, SequenceMultiplicity.ZeroOrMore) },
            FnMin, "min",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsAnyAtomicType, SequenceMultiplicity.ZeroOrOne)),
        new(new[] { new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrMore) },
            FnZeroOrOne, "zero-or-one",
            BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.Item, SequenceMultiplicity.ZeroOrOne)),
        new(new[] { new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrMore) },
            FnEmpty, "empty", BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsBoolean, SequenceMultiplicity.ExactlyOne)),
        new(new[] { new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrMore) },
            FnExists, "exists", BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsBoolean, SequenceMultiplicity.ExactlyOne)),
        new(new[]
            {
                new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrMore),
                new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrMore)
            },
            FnDeepEqual, "deep-equal", BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsBoolean, SequenceMultiplicity.ExactlyOne)),
        new(new[]
            {
                new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrMore),
                new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrMore),
                new ParameterType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)
            },
            (_, _, _, _) => { throw new Exception("FOCH0002: No collations are supported"); },
            "deep-equal", BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsBoolean, SequenceMultiplicity.ExactlyOne))
    };
}