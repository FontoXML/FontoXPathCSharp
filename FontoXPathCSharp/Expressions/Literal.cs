using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions;

public class Literal<TNode> : AbstractExpression<TNode> where TNode : notnull
{
    private readonly Func<ISequence> _createValueSequence;

    public Literal(string value, SequenceType type) : base(
        new Specificity(),
        Array.Empty<AbstractExpression<TNode>>(),
        new OptimizationOptions(
            true,
            resultOrder: ResultOrdering.Sorted)
    )
    {
        _createValueSequence = type.ValueType switch
        {
            ValueType.XsInteger => () =>
                SequenceFactory.CreateFromValue(IntegerValue.CreateIntegerValue(value, type.ValueType)),
            ValueType.XsString => () => SequenceFactory.CreateFromValue(StringValue.CreateStringValue(value, ValueType.XsString)),
            ValueType.XsDouble => () => SequenceFactory.CreateFromValue(DoubleValue.CreateDoubleValue(value)),
            ValueType.XsDecimal => () => SequenceFactory.CreateFromValue(DecimalValue.CreateDecimalValue(value)),
            _ => throw new ArgumentOutOfRangeException($"Type '{type}' not expected in literal")
        };
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode>? executionParameters)
    {
        return _createValueSequence();
    }
}