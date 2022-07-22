using System.Collections.ObjectModel;
using FontoXPathCSharp.EvaluationUtils;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.Operators;

public class UnaryOperator : AbstractExpression
{
    private readonly IReadOnlyDictionary<ValueType, ValueType> UnaryLookup = new Dictionary<ValueType, ValueType>
    {
        { ValueType.XsInteger, ValueType.XsInteger },
        { ValueType.XsNonPositiveInteger, ValueType.XsInteger },
        { ValueType.XsNegativeInteger, ValueType.XsInteger },
        { ValueType.XsLong, ValueType.XsInteger },
        { ValueType.XsInt, ValueType.XsInteger },
        { ValueType.XsLong, ValueType.XsInteger },
        { ValueType.XsInt, ValueType.XsInteger },
        { ValueType.XsShort, ValueType.XsInteger },
        { ValueType.XsByte, ValueType.XsInteger },
        { ValueType.XsNonNegativeInteger, ValueType.XsInteger },
        { ValueType.XsUnsignedLong, ValueType.XsInteger },
        { ValueType.XsUnsignedInt, ValueType.XsInteger },
        { ValueType.XsUnsignedShort, ValueType.XsInteger },
        { ValueType.XsUnsignedByte, ValueType.XsInteger },
        { ValueType.XsPositiveInteger, ValueType.XsInteger },
        { ValueType.XsDecimal, ValueType.XsDecimal },
        { ValueType.XsFloat, ValueType.XsFloat },
        { ValueType.XsDouble, ValueType.XsDouble }
    };

    private readonly UnaryOperatorKind _kind;
    private readonly AbstractExpression _valueExpr;

    public UnaryOperator(AstNodeName kind, AbstractExpression valueExpr) : base(new[] { valueExpr },
        new OptimizationOptions(false))
    {
        _valueExpr = valueExpr;
        _kind = kind switch {
            AstNodeName.UnaryMinusOp => UnaryOperatorKind.Minus,
            AstNodeName.UnaryPlusOp => UnaryOperatorKind.Plus,
            _ => throw new XPathException($"It's not possible to create a unary operator with {kind}")
        };
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
    {
        return Atomize.AtomizeSequence(
            _valueExpr.EvaluateMaybeStatically(dynamicContext, executionParameters),
            executionParameters
        ).MapAll(atomizedValues =>
        {
            if (atomizedValues.Length == 0) // Return the empty sequence when inputted the empty sequence
                return SequenceFactory.CreateEmpty();

            var value = atomizedValues[0];

            // TODO: No type inferrence yet.
            // if (this.type) {
            //     var finalValue = _kind == UnaryOperatorKind.Plus ? +value.value : -value.value;
            //     if (value.type === ValueType.XSBOOLEAN) finalValue = Number.NaN;
            //     return sequenceFactory.singleton(createAtomicValue(finalValue, this.type.type));
            // }

            if (atomizedValues.Length > 1)
                throw new XPathException(
                    "XPTY0004: The operand to a unary operator must be a sequence with a length less than one");

            if (value.GetValueType().IsSubtypeOf(ValueType.XsUntypedAtomic))
            {
                var castValue = value.CastToType(ValueType.XsDouble).GetAs<DoubleValue>();
                return SequenceFactory.CreateFromValue(
                    AtomicValue.Create(
                        _kind == UnaryOperatorKind.Minus ? -castValue.Value : castValue.Value,
                        ValueType.XsDouble
                    )
                );
            }

            if (value.GetValueType().IsSubtypeOf(ValueType.XsNumeric))
            {
                if (_kind == UnaryOperatorKind.Plus) return SequenceFactory.CreateFromValue(value);

                // Not very pretty, but it is what it is, maybe this can be fixed later.
                if (value.GetValueType().IsSubtypeOf(ValueType.XsDouble))
                    return SequenceFactory.CreateFromValue(
                        AtomicValue.Create(value.GetAs<DoubleValue>().Value * -1,
                            UnaryLookup[value.GetValueType()])
                    );
                if (value.GetValueType().IsSubtypeOf(ValueType.XsFloat))
                    return SequenceFactory.CreateFromValue(
                        AtomicValue.Create(value.GetAs<FloatValue>().Value * -1,
                            UnaryLookup[value.GetValueType()])
                    );

                if (value.GetValueType().IsSubtypeOf(ValueType.XsInteger))
                    return SequenceFactory.CreateFromValue(
                        AtomicValue.Create(value.GetAs<IntValue>().Value * -1, UnaryLookup[value.GetValueType()])
                    );
            }

            return SequenceFactory.CreateFromValue(AtomicValue.Create(double.NaN, ValueType.XsDouble));
        });
    }

    private enum UnaryOperatorKind
    {
        Plus,
        Minus
    }
}