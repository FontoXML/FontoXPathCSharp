using System.Reflection.Metadata;
using System.Xml.Xsl;
using FontoXPathCSharp.EvaluationUtils;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions;

public enum CompareType
{
    Equal,
    NotEqual
}

public class ValueCompare : AbstractExpression
{
    private readonly CompareType _type;
    private readonly AbstractExpression _firstExpression;
    private readonly AbstractExpression _secondExpression;

    public ValueCompare(CompareType type, AbstractExpression firstExpression,
        AbstractExpression secondExpression) : base(
        new[] {firstExpression, secondExpression}, new OptimizationOptions(false))
    {
        _type = type;
        _firstExpression = firstExpression;
        _secondExpression = secondExpression;
    }

    private static bool HandleNumericEqualOp(AbstractValue first, AbstractValue second)
    {
        if (first.GetValueType() == ValueType.XsInteger && second.GetValueType() == ValueType.XsInteger)
            return first.GetAs<IntValue>().Value == second.GetAs<IntValue>().Value;
        if (first.GetValueType() == ValueType.XsFloat && second.GetValueType() == ValueType.XsFloat)
            return first.GetAs<FloatValue>().Value == second.GetAs<FloatValue>().Value;
        if (first.GetValueType() == ValueType.XsDouble && second.GetValueType() == ValueType.XsDouble)
            return first.GetAs<DoubleValue>().Value == second.GetAs<DoubleValue>().Value;
        if (first.GetValueType() == ValueType.XsString && second.GetValueType() == ValueType.XsString)
            return first.GetAs<StringValue>().Value == second.GetAs<StringValue>().Value;
        if (first.GetValueType() == ValueType.XsBoolean && second.GetValueType() == ValueType.XsBoolean)
            return first.GetAs<BooleanValue>().Value == second.GetAs<BooleanValue>().Value;
        throw new NotImplementedException("HandleNumericEqualOp: comparison for "  + first.GetValueType() + " not supported");
    }

    private static bool HandleNumericNotEqualOp(AbstractValue first, AbstractValue second)
    {
        if (first.GetValueType() == ValueType.XsInteger && second.GetValueType() == ValueType.XsInteger)
            return first.GetAs<IntValue>().Value != second.GetAs<IntValue>().Value;
        if (first.GetValueType() == ValueType.XsFloat && second.GetValueType() == ValueType.XsFloat)
            return first.GetAs<FloatValue>().Value != second.GetAs<FloatValue>().Value;
        if (first.GetValueType() == ValueType.XsDouble && second.GetValueType() == ValueType.XsDouble)
            return first.GetAs<DoubleValue>().Value != second.GetAs<DoubleValue>().Value;
        if (first.GetValueType() == ValueType.XsString && second.GetValueType() == ValueType.XsString)
            return first.GetAs<StringValue>().Value != second.GetAs<StringValue>().Value;
        if (first.GetValueType() == ValueType.XsBoolean && second.GetValueType() == ValueType.XsBoolean)
            return first.GetAs<BooleanValue>().Value != second.GetAs<BooleanValue>().Value;
        throw new NotImplementedException("HandleNumericNotEqualOp: comparison for "  + first.GetValueType() + " not supported");
    }

    private static bool HandleNumericOperator(CompareType type, AbstractValue first, AbstractValue second)
    {
        if (first.GetValueType() != second.GetValueType())
            throw new NotImplementedException(
                "HandleNumericOperator: Different numeric types");


        return type switch
        {
            CompareType.Equal => HandleNumericEqualOp(first, second),
            CompareType.NotEqual => HandleNumericNotEqualOp(first, second),
            _ => throw new NotImplementedException("HandleNumericOperator: " + type)
        };
    }

    public static bool PerformValueCompare(CompareType type, AbstractValue first, AbstractValue second,
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
        {
            throw new NotImplementedException("PerformValueCompare: Handle QName");
            // return HandleQName(type, first, second);
        }

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
        {
            return HandleNumericOperator(type, first, second);
        }

        if (AreBothSubtypeOf(ValueType.XsYearMonthDuration))
        {
            throw new NotImplementedException("YearMonthDuration comparison");
        }

        if (AreBothSubtypeOf(ValueType.XsDayTimeDuration))
        {
            throw new NotImplementedException("DayTimeDuration comparison");
        }

        if (AreBothSubtypeOf(ValueType.XsDuration))
        {
            throw new NotImplementedException("Duration comparison");
        }

        if (AreBothSubtypeOf(ValueType.XsDateTime) ||
            AreBothSubtypeOf(ValueType.XsDate) ||
            AreBothSubtypeOf(ValueType.XsTime))
        {
            throw new NotImplementedException("DateTime, Date, and Time comparison");
        }

        if (AreBothSubtypeOf(ValueType.XsGYearMonth) ||
            AreBothSubtypeOf(ValueType.XsGYear) ||
            AreBothSubtypeOf(ValueType.XsGMonthDay) ||
            AreBothSubtypeOf(ValueType.XsGMonth) ||
            AreBothSubtypeOf(ValueType.XsGDay))
        {
            throw new NotImplementedException("GYearMonth, GYear, GMonthDay, GMonth, and GDay comparison");
        }

        throw new XPathException("XPTY0004: " + type + " not available for " + firstType + " and " + secondType);
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
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