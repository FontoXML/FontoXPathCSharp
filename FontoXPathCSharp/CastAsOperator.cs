using FontoXPathCSharp.EvaluationUtils;
using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp;

public class CastAsOperator : AbstractExpression
{
    private readonly bool _allowsEmptySequence;
    private readonly AbstractExpression _expression;
    private readonly ValueType _targetType;

    public CastAsOperator(AbstractExpression expression, QName targetType, bool allowsEmptySequence) : base(
        new[] { expression }, new OptimizationOptions(false))
    {
        _targetType = (targetType.Prefix != null
            ? $"{targetType.Prefix}:{targetType.LocalName}"
            : targetType.LocalName).StringToValueType();

        if (_targetType is ValueType.XsAnyAtomicType or ValueType.XsAnySimpleType or ValueType.XsNotation)
            throw new XPathException(
                "XPST0080: Casting to xs:anyAtomicType, xs:anySimpleType or xs:NOTATION is not permitted.");

        if (targetType.NamespaceUri != null)
            throw new NotImplementedException("Not implemented: casting expressions with a namespace URI.");

        _expression = expression;
        _allowsEmptySequence = allowsEmptySequence;
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
    {
        var evaluatedExpression = Atomize.AtomizeSequence(
            _expression.EvaluateMaybeStatically(dynamicContext, executionParameters),
            executionParameters
        );
        if (evaluatedExpression.IsEmpty())
        {
            if (!_allowsEmptySequence)
                throw new XPathException("XPTY0004: Sequence to cast is empty while target type is singleton.");
            return SequenceFactory.CreateEmpty();
        }

        if (evaluatedExpression.IsSingleton())
            return evaluatedExpression.Map((val, _, _) =>
                TypeCasting.CastToType(val.GetAs<AtomicValue>(), _targetType));

        throw new XPathException("XPTY0004: Sequence to cast is not singleton or empty.");
    }
}