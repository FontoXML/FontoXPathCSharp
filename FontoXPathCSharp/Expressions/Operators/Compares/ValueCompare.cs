using FontoXPathCSharp.EvaluationUtils;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions;

public enum CompareType
{
    Equal,
    NotEqual,
    LessThan,
    LessEquals,
    GreaterThan,
    GreaterEquals
}

public class ValueCompare<TNode> : AbstractExpression<TNode>
{
    private readonly AbstractExpression<TNode> _firstExpression;
    private readonly AbstractExpression<TNode> _secondExpression;
    private readonly CompareType _type;

    public ValueCompare(CompareType type, AbstractExpression<TNode> firstExpression,
        AbstractExpression<TNode> secondExpression) : base(
        new[] { firstExpression, secondExpression }, new OptimizationOptions(false))
    {
        _type = type;
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
        if (a.GetValueType() != b.GetValueType())
            throw new NotImplementedException(
                "HandleNumericOperator: Different numeric types");

        return a.GetValueType() switch
        {
            ValueType.XsBoolean => Compare(type, a.GetAs<BooleanValue>().Value, b.GetAs<BooleanValue>().Value),
            ValueType.XsInteger or ValueType.XsInt => Compare(type, a.GetAs<IntValue>().Value,
                b.GetAs<IntValue>().Value),
            ValueType.XsFloat => Compare(type, a.GetAs<FloatValue>().Value, b.GetAs<FloatValue>().Value),
            ValueType.XsDouble => Compare(type, a.GetAs<DoubleValue>().Value, b.GetAs<DoubleValue>().Value),
            ValueType.XsString => Compare(type, a.GetAs<StringValue>().Value, b.GetAs<StringValue>().Value),
            _ => throw new ArgumentOutOfRangeException(
                $"Comparison between operands of type {a.GetValueType()} not implemented yet.")
        };
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
            return types.Select(x => firstType.IsSubtypeOf(x)).Any() &&
                   types.Select(x => secondType.IsSubtypeOf(x)).Any();
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

        if (AreBothSubtypeOf(ValueType.XsYearMonthDuration))
            throw new NotImplementedException("YearMonthDuration comparison");

        if (AreBothSubtypeOf(ValueType.XsDayTimeDuration))
            throw new NotImplementedException("DayTimeDuration comparison");

        if (AreBothSubtypeOf(ValueType.XsDuration)) throw new NotImplementedException("Duration comparison");

        if (AreBothSubtypeOf(ValueType.XsDateTime) ||
            AreBothSubtypeOf(ValueType.XsDate) ||
            AreBothSubtypeOf(ValueType.XsTime))
            throw new NotImplementedException("DateTime, Date, and Time comparison");

        if (AreBothSubtypeOf(ValueType.XsGYearMonth) ||
            AreBothSubtypeOf(ValueType.XsGYear) ||
            AreBothSubtypeOf(ValueType.XsGMonthDay) ||
            AreBothSubtypeOf(ValueType.XsGMonth) ||
            AreBothSubtypeOf(ValueType.XsGDay))
            throw new NotImplementedException("GYearMonth, GYear, GMonthDay, GMonth, and GDay comparison");

        throw new XPathException("XPTY0004: " + type + " not available for " + firstType + " and " + secondType);
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode> executionParameters)
    {
        var firstSequence = _firstExpression.EvaluateMaybeStatically(dynamicContext, executionParameters);
        var secondSequence = _secondExpression.EvaluateMaybeStatically(dynamicContext, executionParameters);

        var firstAtomizedSequence = Atomize.AtomizeSequence(firstSequence, executionParameters);
        var secondAtomizedSequence = Atomize.AtomizeSequence(secondSequence, executionParameters);

        if (firstAtomizedSequence.IsEmpty() || secondAtomizedSequence.IsEmpty())
            return SequenceFactory.CreateEmpty();

        var onlyFirstValue = firstAtomizedSequence.First();
        var onlySecondValue = secondAtomizedSequence.First();

        return SequenceFactory.CreateFromValue(
            new BooleanValue(PerformValueCompare(_type, onlyFirstValue, onlySecondValue, dynamicContext)));
    }
}