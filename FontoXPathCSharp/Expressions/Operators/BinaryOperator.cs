using FontoXPathCSharp.EvaluationUtils;
using FontoXPathCSharp.Expressions.Operators.Arithmetic;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.Operators;

internal delegate AbstractValue BinaryOperatorFunction(AbstractValue left, AbstractValue right);

internal class BinaryOperator<TNode> : AbstractExpression<TNode> where TNode : notnull
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly ValueType[] AllTypes =
    {
        ValueType.XsNumeric,
        ValueType.XsYearMonthDuration,
        ValueType.XsDayTimeDuration,
        ValueType.XsDateTime,
        ValueType.XsDate,
        ValueType.XsTime
    };

    private readonly AbstractExpression<TNode> _firstValueExpr;
    private readonly AstNodeName _operator;

    private readonly Dictionary<(ValueType, ValueType, AstNodeName), BinaryOperatorFunction?> _operatorsByTypingKey =
        new();

    private readonly AbstractExpression<TNode> _secondValueExpr;

    public BinaryOperator(AstNodeName op, AbstractExpression<TNode> firstValueExpr,
        AbstractExpression<TNode> secondValueExpr) : base(
        firstValueExpr.Specificity.Add(secondValueExpr.Specificity),
        new[] { firstValueExpr, secondValueExpr },
        new OptimizationOptions(false))
    {
        _operator = op;
        _firstValueExpr = firstValueExpr;
        _secondValueExpr = secondValueExpr;
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode>? executionParameters)
    {
        var firstValueSequence = Atomize.AtomizeSequence(
            _firstValueExpr.EvaluateMaybeStatically(dynamicContext, executionParameters),
            executionParameters!
        );

        return firstValueSequence.MapAll(firstValues =>
        {
            if (firstValues.Length == 0) // Shortcut, if the first part is empty, we can return empty.
                // As per spec, we do not have to evaluate the second part, though we could.
                return SequenceFactory.CreateEmpty();
            var secondValueSequence = Atomize.AtomizeSequence(
                _secondValueExpr.EvaluateMaybeStatically(dynamicContext, executionParameters),
                executionParameters!
            );
            return secondValueSequence.MapAll(secondValues =>
            {
                if (secondValues.Length == 0) return SequenceFactory.CreateEmpty();

                if (firstValues.Length > 1 || secondValues.Length > 1)
                    throw new XPathException(
                        "XPTY0004", "The operands of the {_operator} operator should be empty or singleton."
                    );

                var firstValue = firstValues.First();
                var secondValue = secondValues.First();

                // We could infer all the necessary type information to do an early return

                var prefabOperator = GetBinaryPrefabOperator(
                    firstValue.GetValueType(),
                    secondValue.GetValueType(),
                    _operator
                );

                if (prefabOperator == null)
                    throw new XPathException(
                        "XPTY0004",
                        $"{_operator} not available for types {firstValue.GetValueType()} and {secondValue.GetValueType()}"
                    );

                return SequenceFactory.CreateFromValue(prefabOperator(firstValue, secondValue));
            });
        });
    }

    private BinaryOperatorFunction? GetBinaryPrefabOperator(ValueType leftType, ValueType rightType, AstNodeName op)
    {
        var typingKey = (leftType, rightType, op); //$"{leftType}~{rightType}~{op}";

        if (!_operatorsByTypingKey.ContainsKey(typingKey))
            _operatorsByTypingKey.Add(typingKey, GenerateBinaryOperatorFunction(op, leftType, rightType));

        return _operatorsByTypingKey[typingKey];
    }

    private static ValueType DetermineReturnType(ValueType typeA, ValueType typeB)
    {
        if (typeA.IsSubtypeOf(ValueType.XsInteger) && typeB.IsSubtypeOf(ValueType.XsInteger))
            return ValueType.XsInteger;

        if (typeA.IsSubtypeOf(ValueType.XsDecimal) && typeB.IsSubtypeOf(ValueType.XsDecimal))
            return ValueType.XsDecimal;

        if (typeA.IsSubtypeOf(ValueType.XsFloat) && typeB.IsSubtypeOf(ValueType.XsFloat)) return ValueType.XsFloat;

        return ValueType.XsDouble;
    }

    // ReSharper disable once InconsistentNaming
    private static (BinaryOperatorFunction, ValueType) IDivOpChecksFunction(
        Func<AbstractValue, AbstractValue, (AtomicValue, AtomicValue)> applyCastFunctions,
        Func<object, object, object> fun)
    {
        return ((a, b) =>
        {
            var (castA, castB) = applyCastFunctions((AtomicValue)a, (AtomicValue)b);
            var valueA = Convert.ToDouble(castA.GetValue());
            var valueB = Convert.ToDouble(castB.GetValue());
            if (valueB == 0) throw new XPathException("FOAR0001", "Divisor of idiv operator cannot be (-)0");

            if (double.IsNaN(valueA) || double.IsNaN(valueB) || !double.IsFinite(valueA))
                throw new XPathException("FOAR0002",
                    "One of the operands of idiv is NaN or the first operand is (-)INF");

            if (double.IsFinite(valueA) && !double.IsFinite(valueB)) return AtomicValue.Create(0, ValueType.XsInteger);

            return AtomicValue.Create(fun(castA.GetValue(), castB.GetValue()), ValueType.XsInteger);
        }, ValueType.XsInteger);
    }

    private BinaryOperatorFunction? GenerateBinaryOperatorFunction(
        AstNodeName op,
        ValueType typeA,
        ValueType typeB)
    {
        Func<AbstractValue, AtomicValue>? castFunctionForValueA = null;
        Func<AbstractValue, AtomicValue>? castFunctionForValueB = null;

        if (typeA.IsSubtypeOf(ValueType.XsUntypedAtomic))
        {
            castFunctionForValueA = value => value.CastToType(ValueType.XsDouble);
            typeA = ValueType.XsDouble;
        }

        if (typeB.IsSubtypeOf(ValueType.XsUntypedAtomic))
        {
            castFunctionForValueB = value => value.CastToType(ValueType.XsDouble);
            typeB = ValueType.XsDouble;
        }

        Func<AbstractValue, AbstractValue, (AtomicValue, AtomicValue)> applyCastFunctions = (valueA, valueB) => (
            castFunctionForValueA != null ? castFunctionForValueA(valueA) : (AtomicValue)valueA,
            castFunctionForValueB != null ? castFunctionForValueB(valueB) : (AtomicValue)valueB
        );

        // Filter all the possible types to only those which the operands are a subtype of.
        var parentTypesOfA = AllTypes.Where(e => typeA.IsSubtypeOf(e));
        var parentTypesOfB = AllTypes.Where(e => typeB.IsSubtypeOf(e));

        var typesOfA = parentTypesOfA as ValueType[] ?? parentTypesOfA.ToArray();
        var typesOfB = parentTypesOfB as ValueType[] ?? parentTypesOfB.ToArray();
        if (typesOfA.Contains(ValueType.XsNumeric) && typesOfB.Contains(ValueType.XsNumeric))
        {
            var fun = BinaryEvaluationFunctionMap.GetOperationForOperands(ValueType.XsNumeric, ValueType.XsNumeric, op);
            if (fun == null)
                throw new Exception(
                    $"No suitable operator function could be found for {(ValueType.XsNumeric, ValueType.XsNumeric, op)}");
            var mapRetType =
                BinaryEvaluationFunctionMap.GetReturnTypeForOperands(ValueType.XsNumeric, ValueType.XsNumeric, op);
            var retType = mapRetType ?? DetermineReturnType(typeA, typeB);
            if (op == AstNodeName.DivOp && retType == ValueType.XsInteger) retType = ValueType.XsDecimal;
            if (op == AstNodeName.IDivOp)
                return IDivOpChecksFunction(applyCastFunctions,
                    (a, b) => Math.Truncate(Convert.ToDecimal(a) / Convert.ToDecimal(b))).Item1;
            return (a, b) =>
            {
                var (castA, castB) = applyCastFunctions(a, b);
                return AtomicValue.Create(fun(castA.GetValue(), castB.GetValue()), retType);
            };
        }

        foreach (var typeOfA in typesOfA)
        foreach (var typeOfB in typesOfB)
        {
            var func = BinaryEvaluationFunctionMap.GetOperationForOperands(typeOfA, typeOfB, op);
            var mapRet = BinaryEvaluationFunctionMap.GetReturnTypeForOperands(typeOfA, typeOfB, op);
            if (func != null && mapRet != null)
            {
                var ret = (ValueType)mapRet;
                return (a, b) =>
                {
                    var (castA, castB) = applyCastFunctions(a, b);
                    return AtomicValue.Create(func(castA.GetValue(), castB.GetValue()), ret);
                };
            }
        }

        return null;
    }
}