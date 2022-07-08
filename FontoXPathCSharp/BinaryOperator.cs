using FontoXPathCSharp.EvaluationUtils;
using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp;

internal delegate AbstractValue BinaryOperatorFunction(AbstractValue left, AbstractValue right);

internal class BinaryOperator : AbstractExpression
{
    private static readonly ValueType[] _allTypes =
    {
        ValueType.XsNumeric,
        ValueType.XsYearMonthDuration,
        ValueType.XsDayTimeDuration,
        ValueType.XsDateTime,
        ValueType.XsDate,
        ValueType.XsTime
    };

    private readonly AbstractExpression _firstValueExpr;
    private readonly AstNodeName _operator;
    private readonly AbstractExpression _secondValueExpr;

    private readonly Dictionary<(ValueType, ValueType, AstNodeName), BinaryOperatorFunction> OperatorsByTypingKey =
        new();

    public BinaryOperator(AstNodeName op, AbstractExpression firstValueExpr, AbstractExpression secondValueExpr) : base(
        new[] {firstValueExpr, secondValueExpr}, new OptimizationOptions(true))
    {
        _operator = op;
        _firstValueExpr = firstValueExpr;
        _secondValueExpr = secondValueExpr;
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
    {
        var firstValueSequence = Atomize.AtomizeSequence(
            _firstValueExpr.EvaluateMaybeStatically(dynamicContext, executionParameters),
            executionParameters
        );

        return firstValueSequence.MapAll(firstValues =>
        {
            if (firstValues.Length == 0) // Shortcut, if the first part is empty, we can return empty.
                // As per spec, we do not have to evaluate the second part, though we could.
                return SequenceFactory.CreateEmpty();
            var secondValueSequence = Atomize.AtomizeSequence(
                _secondValueExpr.EvaluateMaybeStatically(dynamicContext, executionParameters),
                executionParameters
            );
            return secondValueSequence.MapAll(secondValues =>
            {
                if (secondValues.Length == 0) return SequenceFactory.CreateEmpty();

                if (firstValues.Length > 1 || secondValues.Length > 1)
                    throw new XPathException(
                        "XPTY0004: the operands of the {_operator} operator should be empty or singleton."
                    );

                var firstValue = firstValues.First();
                var secondValue = secondValues.First();

                // We could infer all the necessary type information to do an early return

                var prefabOperator = GetBinaryPrefabOperator(
                    firstValue.GetValueType(),
                    secondValue.GetValueType(),
                    _operator
                );

                if (!prefabOperator)
                    throw new XPathException(
                        $"XPTY0004: {_operator} not available for types {firstValue.GetValueType()} and {secondValue.GetValueType()}"
                    );

                return SequenceFactory.CreateFromValue(prefabOperator(firstValue, secondValue));
            });
        });
    }

    private BinaryOperatorFunction GetBinaryPrefabOperator(ValueType leftType, ValueType rightType, AstNodeName op)
    {
        var typingKey = (leftType, rightType, op); //$"{leftType}~{rightType}~{op}";

        if (!OperatorsByTypingKey.ContainsKey(typingKey))
            OperatorsByTypingKey.Add(typingKey, GenerateBinaryOperatorFunction(op, leftType, rightType));

        return OperatorsByTypingKey[typingKey];
    }

    private BinaryOperatorFunction GenerateBinaryOperatorFunction(AstNodeName op, ValueType typeA,
        ValueType typeB)
    {
        Func<AbstractValue, AbstractValue> castFunctionForValueA = null;
        Func<AbstractValue, AbstractValue> castFunctionForValueB = null;

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

        // Filter all the possible types to only those which the operands are a subtype of.
        var parentTypesOfA = _allTypes.Where(e => typeA.IsSubtypeOf(e));
        var parentTypesOfB = _allTypes.Where(e => typeB.IsSubtypeOf(e));

        throw new NotImplementedException("Binary operations need to be finished");
    }
}