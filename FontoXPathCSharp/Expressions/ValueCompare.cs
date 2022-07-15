using FontoXPathCSharp.EvaluationUtils;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions;

public delegate bool ComparatorFunction(AtomicValue lhs, AtomicValue rhs, DynamicContext dynamicContext);

internal delegate (AtomicValue, AtomicValue) ApplyCastingFunction(AtomicValue lhs, AtomicValue rhs);

public enum CompareType
{
    Equal,
    NotEqual,
    Less,
    LessOrEqual,
    Greater,
    GreaterOrEqual
}

public class ValueCompare : AbstractExpression
{
    private static readonly Dictionary<(ValueType, ValueType, CompareType), ComparatorFunction> ComparatorsByTypingKey =
        new();

    private readonly AbstractExpression _firstExpression;
    private readonly CompareType _operator;
    private readonly AbstractExpression _secondExpression;

    public ValueCompare(CompareType operatorType, AbstractExpression firstExpression,
        AbstractExpression secondExpression) : base(
        new[] { firstExpression, secondExpression }, new OptimizationOptions(false))
    {
        _operator = operatorType;
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
        throw new NotImplementedException("HandleNumericEqualOp: comparison for " + first.GetValueType() +
                                          " not supported");
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
        throw new NotImplementedException("HandleNumericNotEqualOp: comparison for " + first.GetValueType() +
                                          " not supported");
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

    public static ComparatorFunction PerformValueCompare(CompareType op, ValueType typeA, ValueType typeB)
    {
        var typingKey = (typeA, typeB, op);
        if (!ComparatorsByTypingKey.ContainsKey(typingKey))
            ComparatorsByTypingKey[typingKey] = GenerateCompareFunction(op, typeA, typeB);

        return ComparatorsByTypingKey[typingKey];
    }

    private static bool AreBothSubtypeOf(ValueType typeA, ValueType typeB, ValueType superType)
    {
        return typeA.IsSubtypeOf(superType) && typeB.IsSubtypeOf(superType);
    }

    private static bool AreBothSubtypeOfAny(ValueType typeA, ValueType typeB, params ValueType[] superTypes)
    {
        return typeA.IsSubTypeOfAny(superTypes) && typeB.IsSubTypeOfAny(superTypes);
    }

    private static ComparatorFunction GenerateCompareFunction(CompareType op, ValueType typeA, ValueType typeB)
    {
        Func<AtomicValue, AtomicValue>? castFunctionForValueA = null;
        Func<AtomicValue, AtomicValue>? castFunctionForValueB = null;

        if (AreBothSubtypeOf(typeA, typeB, ValueType.XsUntypedAtomic))
        {
            typeA = ValueType.XsString;
            typeB = ValueType.XsString;
        }
        else if (typeA.IsSubtypeOf(ValueType.XsUntypedAtomic))
        {
            castFunctionForValueA = val => val.CastToType(typeB);
            typeA = typeB;
        }
        else if (typeB.IsSubtypeOf(ValueType.XsUntypedAtomic))
        {
            castFunctionForValueB = val => val.CastToType(typeA);
            typeB = typeA;
        }

        ApplyCastingFunction applyCastFunctions = (valA, valB) =>
            (castFunctionForValueA != null ? castFunctionForValueA(valA) : valA,
                castFunctionForValueB != null ? castFunctionForValueB(valB) : valB);

        if (AreBothSubtypeOf(typeA, typeB, ValueType.XsQName))
            return HandleQName(op, applyCastFunctions);

        if (AreBothSubtypeOf(typeA, typeB, ValueType.XsBoolean) ||
            AreBothSubtypeOfAny(typeA, typeB, ValueType.XsString, ValueType.Attribute, ValueType.Map) ||
            AreBothSubtypeOfAny(typeA, typeB, ValueType.XsNumeric, ValueType.Attribute, ValueType.Map) ||
            AreBothSubtypeOf(typeA, typeB, ValueType.XsAnyUri) ||
            AreBothSubtypeOf(typeA, typeB, ValueType.XsHexBinary) ||
            AreBothSubtypeOf(typeA, typeB, ValueType.XsBase64Binary) ||
            AreBothSubtypeOfAny(typeA, typeB, ValueType.XsString, ValueType.XsAnyUri))
        {
            var result = HandleNumeric(op, applyCastFunctions);
            if (result != null) return result;
        }

        if (AreBothSubtypeOf(typeA, typeB, ValueType.XsYearMonthDuration))
        {
            var result = HandleYearMonthDuration(op, applyCastFunctions);
            if (result != null) return result;
        }

        if (AreBothSubtypeOf(typeA, typeB, ValueType.XsDayTimeDuration))
        {
            var result = HandleDayTimeDuration(op, applyCastFunctions);
            if (result != null) return result;
        }

        if (AreBothSubtypeOf(typeA, typeB, ValueType.XsDuration))
        {
            var result = HandleDuration(op, applyCastFunctions);
            if (result != null) return result;
        }

        if (AreBothSubtypeOf(typeA, typeB, ValueType.XsDateTime) ||
            AreBothSubtypeOf(typeA, typeB, ValueType.XsDate) ||
            AreBothSubtypeOf(typeA, typeB, ValueType.XsTime))
        {
            var result = HandleDateAndTime(op, applyCastFunctions);
            if (result != null) return result;
        }

        if (AreBothSubtypeOf(typeA, typeB, ValueType.XsGYearMonth) ||
            AreBothSubtypeOf(typeA, typeB, ValueType.XsGYear) ||
            AreBothSubtypeOf(typeA, typeB, ValueType.XsGMonthDay) ||
            AreBothSubtypeOf(typeA, typeB, ValueType.XsGMonth) ||
            AreBothSubtypeOf(typeA, typeB, ValueType.XsGDay))
        {
            var result = HandleDayMonthAndYear(op, applyCastFunctions);
            if (result != null) return result;
        }

        throw new XPathException($"XPTY0004: {op} not available for {typeA.ToString()} and {typeB.ToString()}");
    }

    private static ComparatorFunction? HandleDayMonthAndYear(CompareType op, ApplyCastingFunction applyCastFunctions)
    {
        throw new NotImplementedException("HandleDayMonthAndYear not implemented yet.");
    }

    private static ComparatorFunction? HandleDateAndTime(CompareType op, ApplyCastingFunction applyCastFunctions)
    {
        throw new NotImplementedException("HandleDateAndTime not implemented yet.");
    }

    private static ComparatorFunction? HandleDuration(CompareType op, ApplyCastingFunction applyCastFunctions)
    {
        throw new NotImplementedException("HandleDuration not implemented yet.");
    }

    private static ComparatorFunction? HandleDayTimeDuration(CompareType op, ApplyCastingFunction applyCastFunctions)
    {
        throw new NotImplementedException("HandleDayTimeDuration not implemented yet.");
    }

    private static ComparatorFunction? HandleYearMonthDuration(CompareType op,
        ApplyCastingFunction applyCastFunctions)
    {
        throw new NotImplementedException("HandleYearMonthDuration not implemented yet.");
    }

    private static ComparatorFunction? HandleNumeric(CompareType op,
        ApplyCastingFunction applyCastFunctions)
    {
        return op switch
        {
            CompareType.Equal => (a, b, _) =>
            {
                var res = applyCastFunctions(a, b);
                return (decimal)res.Item1.GetValue() == (decimal)res.Item2.GetValue();
            },
            CompareType.NotEqual => (a, b, _) =>
            {
                var res = applyCastFunctions(a, b);
                return (decimal)res.Item1.GetValue() != (decimal)res.Item2.GetValue();
            },
            CompareType.Less => (a, b, _) =>
            {
                var res = applyCastFunctions(a, b);
                return (decimal)res.Item1.GetValue() < (decimal)res.Item2.GetValue();
            },
            CompareType.LessOrEqual => (a, b, _) =>
            {
                var res = applyCastFunctions(a, b);
                return (decimal)res.Item1.GetValue() <= (decimal)res.Item2.GetValue();
            },
            CompareType.Greater => (a, b, _) =>
            {
                var res = applyCastFunctions(a, b);
                return (decimal)res.Item1.GetValue() > (decimal)res.Item2.GetValue();
            },
            CompareType.GreaterOrEqual => (a, b, _) =>
            {
                var res = applyCastFunctions(a, b);
                return (decimal)res.Item1.GetValue() > (decimal)res.Item2.GetValue();
            },
            _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
        };
    }

    private static ComparatorFunction HandleQName(CompareType op,
        ApplyCastingFunction applyCastFunctions)
    {
        return op switch
        {
            CompareType.Equal => (a, b, _) =>
            {
                var res = applyCastFunctions(a, b);
                var valA = res.Item1.GetAs<QNameValue>().Value;
                var valB = res.Item2.GetAs<QNameValue>().Value;
                return valA.NamespaceUri == valB.NamespaceUri && valA.LocalName == valB.LocalName;
            },
            CompareType.NotEqual => (a, b, _) =>
            {
                var res = applyCastFunctions(a, b);
                var valA = res.Item1.GetAs<QNameValue>().Value;
                var valB = res.Item2.GetAs<QNameValue>().Value;
                return valA.NamespaceUri != valB.NamespaceUri || valA.LocalName != valB.LocalName;
            },
            _ => throw new XPathException("XPTY0004: Only the \"eq\" and \"ne\" comparison is defined for xs:QName")
        };
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
    {
        var firstSequence = _firstExpression.EvaluateMaybeStatically(dynamicContext, executionParameters);
        var secondSequence = _secondExpression.EvaluateMaybeStatically(dynamicContext, executionParameters);

        var firstAtomizedSequence = Atomize.AtomizeSequence(firstSequence, executionParameters);
        var secondAtomizedSequence = Atomize.AtomizeSequence(secondSequence, executionParameters);

        if (firstAtomizedSequence.IsEmpty() || secondAtomizedSequence.IsEmpty())
            return SequenceFactory.CreateEmpty();

        if (!firstAtomizedSequence.IsSingleton() || !secondAtomizedSequence.IsSingleton())
            throw new XPathException("XPTY0004: Sequences to compare are not singleton.");

        var onlyFirstValue = firstAtomizedSequence.First();
        var onlySecondValue = secondAtomizedSequence.First();


        var valueCompare =
            PerformValueCompare(_operator, onlyFirstValue.GetValueType(), onlySecondValue.GetValueType());

        return valueCompare(onlyFirstValue.GetAs<AtomicValue>(), onlySecondValue.GetAs<AtomicValue>(), dynamicContext)
            ? SequenceFactory.SingletonTrueSequence
            : SequenceFactory.SingletonFalseSequence;
    }
}