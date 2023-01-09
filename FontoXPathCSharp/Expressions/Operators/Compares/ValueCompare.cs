using FontoXPathCSharp.EvaluationUtils;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.Operators.Compares;

public enum CompareType
{
    Equal,
    NotEqual,
    LessThan,
    LessEquals,
    GreaterThan,
    GreaterEquals
}

public class ValueCompare<TNode> : AbstractExpression<TNode> where TNode : notnull
{
    private readonly AbstractExpression<TNode> _firstExpression;
    private readonly CompareType _operator;
    private readonly AbstractExpression<TNode> _secondExpression;

    public ValueCompare(CompareType kind, AbstractExpression<TNode> firstExpression,
        AbstractExpression<TNode> secondExpression) : base(
        firstExpression.Specificity.Add(secondExpression.Specificity),
        new[] { firstExpression, secondExpression },
        new OptimizationOptions(false))
    {
        _operator = kind;
        _firstExpression = firstExpression;
        _secondExpression = secondExpression;
    }

    private static bool Compare<T>(CompareType compareType, T a, T b) where T : IComparable
    {
        return compareType switch
        {
            CompareType.Equal => a.CompareTo(b) == 0,
            CompareType.NotEqual => a.CompareTo(b) != 0,
            CompareType.LessThan => a.CompareTo(b) < 0,
            CompareType.LessEquals => a.CompareTo(b) <= 0,
            CompareType.GreaterThan => a.CompareTo(b) > 0,
            CompareType.GreaterEquals => a.CompareTo(b) >= 0,
            _ => throw new ArgumentOutOfRangeException(nameof(compareType), compareType, null)
        };
    }

    private static bool HandleNumericOperator(CompareType type, AbstractValue a, AbstractValue b)
    {
        if (a.GetValueType() == b.GetValueType())
            return a.GetValueType() switch
            {
                ValueType.XsBoolean => Compare(type, a.GetAs<BooleanValue>().Value, b.GetAs<BooleanValue>().Value),
                ValueType.XsInteger
                    or ValueType.XsPositiveInteger
                    or ValueType.XsNegativeInteger
                    or ValueType.XsNonPositiveInteger
                    or ValueType.XsNonNegativeInteger
                    or ValueType.XsByte
                    or ValueType.XsUnsignedByte
                    or ValueType.XsShort
                    or ValueType.XsUnsignedShort
                    or ValueType.XsInt
                    or ValueType.XsUnsignedInt
                    or ValueType.XsLong
                    or ValueType.XsUnsignedLong =>
                    Compare(type, a.GetAs<IntegerValue>().Value, b.GetAs<IntegerValue>().Value),
                ValueType.XsFloat => Compare(type, a.GetAs<FloatValue>().Value, b.GetAs<FloatValue>().Value),
                ValueType.XsDouble => Compare(type, a.GetAs<DoubleValue>().Value, b.GetAs<DoubleValue>().Value),
                ValueType.XsString or ValueType.XsUntypedAtomic => Compare(type, 
                        a.GetAs<UntypedAtomicValue>().GetValue().ToString(), 
                        b.GetAs<UntypedAtomicValue>().GetValue().ToString()),
                ValueType.XsDecimal => Compare(type, a.GetAs<DecimalValue>().Value, b.GetAs<DecimalValue>().Value),
                _ => throw new ArgumentOutOfRangeException(
                    $"Comparison between operands of type {a.GetValueType()} not implemented yet.")
            };

        // Avoids conversions
        if (a.GetValueType().IsSubtypeOf(ValueType.XsInteger) && b.GetValueType().IsSubtypeOf(ValueType.XsInteger))
            return Compare(type, a.GetAs<IntegerValue>().Value, b.GetAs<IntegerValue>().Value);

        // Last resort, see if both are numeric, if so convert to most generic numeric type and compare.
        if (a.GetValueType().IsSubtypeOf(ValueType.XsNumeric) && b.GetValueType().IsSubtypeOf(ValueType.XsNumeric))
            return Compare(type,
                Convert.ToDecimal(a.GetAs<AtomicValue>().GetValue()),
                Convert.ToDecimal(b.GetAs<AtomicValue>().GetValue())
            );


        throw new NotImplementedException(
            $"HandleNumericOperator: Different numeric types: {a.GetValueType()} and {b.GetValueType()}");
    }

    private static bool HandleDuration(CompareType type, AbstractValue first, AbstractValue second)
    {
        return Compare(type, first.GetAs<DurationValue>().Value, first.GetAs<DurationValue>().Value);
    }

    public static bool PerformValueCompare(
        CompareType type,
        AbstractValue first,
        AbstractValue second,
        DynamicContext dynamicContext)
    {
        var firstType = first.GetValueType();
        var secondType = second.GetValueType();

        if (firstType.IsSubtypeOf(ValueType.XsUntypedAtomic) &&
            secondType.IsSubtypeOf(ValueType.XsUntypedAtomic))
        {
            firstType = ValueType.XsString;
            secondType = ValueType.XsString;
        }
        else if (firstType.IsSubtypeOf(ValueType.XsUntypedAtomic))
        {
            first = first.CastToType(secondType);
            firstType = secondType;
        }
        else if (secondType.IsSubtypeOf(ValueType.XsUntypedAtomic))
        {
            second = second.CastToType(firstType);
            secondType = firstType;
        }

        if (firstType.IsSubtypeOf(ValueType.XsQName) && secondType.IsSubtypeOf(ValueType.XsQName))
            throw new NotImplementedException("PerformValueCompare: Handle QName");
        // return HandleQName(type, first, second);

        bool AreBothSubtypeOf(params ValueType[] types)
        {
            return types.Any(x => firstType.IsSubtypeOf(x)) &&
                   types.Any(x => secondType.IsSubtypeOf(x));
        }

        if (
            AreBothSubtypeOf(ValueType.XsBoolean) ||
            AreBothSubtypeOf(ValueType.XsString, ValueType.Attribute, ValueType.Map) ||
            AreBothSubtypeOf(ValueType.XsNumeric, ValueType.Attribute, ValueType.Map) ||
            AreBothSubtypeOf(ValueType.XsAnyUri) ||
            AreBothSubtypeOf(ValueType.XsHexBinary) ||
            AreBothSubtypeOf(ValueType.XsBase64Binary) ||
            AreBothSubtypeOf(ValueType.XsString, ValueType.XsAnyUri)
        )
            return HandleNumericOperator(type, first, second);

        if (AreBothSubtypeOf(ValueType.XsYearMonthDuration) ||
            AreBothSubtypeOf(ValueType.XsDayTimeDuration) ||
            AreBothSubtypeOf(ValueType.XsDuration))
            return HandleDuration(type, first, second);

        if (AreBothSubtypeOf(ValueType.XsDateTime) ||
            AreBothSubtypeOf(ValueType.XsDate) ||
            AreBothSubtypeOf(ValueType.XsTime))
            return HandleDateTime(type, first, second);

        if (AreBothSubtypeOf(ValueType.XsGYearMonth) ||
            AreBothSubtypeOf(ValueType.XsGYear) ||
            AreBothSubtypeOf(ValueType.XsGMonthDay) ||
            AreBothSubtypeOf(ValueType.XsGMonth) ||
            AreBothSubtypeOf(ValueType.XsGDay))
            return HandleDayMonthAndYear(type, first, second);

        throw new XPathException("XPTY0004", type + " not available for " + firstType + " and " + secondType);
    }

    private static bool HandleDayMonthAndYear(CompareType type, AbstractValue first, AbstractValue second)
    {
        return type switch
        {
            CompareType.Equal or CompareType.NotEqual => Compare(
                type,
                first.GetAs<DateTimeValue>().Value,
                second.GetAs<DateTimeValue>().Value
            ),
            _ => throw new XPathException(
                "XPTY0004",
                $"{type} not available for {first.GetType()} and {first.GetType()}"
            )
        };
    }

    private static bool HandleDateTime(CompareType type, AbstractValue first, AbstractValue second)
    {
        return Compare(type, first.GetAs<DateTimeValue>().Value, second.GetAs<DateTimeValue>().Value);
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode>? executionParameters)
    {
        var firstSequence = _firstExpression.EvaluateMaybeStatically(dynamicContext, executionParameters);
        var secondSequence = _secondExpression.EvaluateMaybeStatically(dynamicContext, executionParameters);

        var firstAtomizedSequence = Atomize.AtomizeSequence(firstSequence, executionParameters!);
        var secondAtomizedSequence = Atomize.AtomizeSequence(secondSequence, executionParameters!);

        if (firstAtomizedSequence.IsEmpty() || secondAtomizedSequence.IsEmpty())
            return SequenceFactory.CreateEmpty();

        var onlyFirstValue = firstAtomizedSequence.First()!;
        var onlySecondValue = secondAtomizedSequence.First()!;

        return SequenceFactory.CreateFromValue(
            new BooleanValue(PerformValueCompare(_operator, onlyFirstValue, onlySecondValue, dynamicContext!)));
    }
}