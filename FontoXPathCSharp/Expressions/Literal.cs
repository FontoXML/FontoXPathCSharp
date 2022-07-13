using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions;

public class Literal : AbstractExpression
{
    private readonly Func<ISequence> _createValueSequence;

    public Literal(string value, SequenceType type) : base(Array.Empty<AbstractExpression>(),
        new OptimizationOptions(true))
    {
        _createValueSequence = type.ValueType switch
        {
            ValueType.XsInteger or ValueType.XsDecimal => () => SequenceFactory.CreateFromValue(new IntValue(int.Parse(value))),
            ValueType.XsString => () => SequenceFactory.CreateFromValue(new StringValue(value)),
            ValueType.XsDouble => () => SequenceFactory.CreateFromValue(new DoubleValue(Decimal.Parse(value))),
            _ => throw new XPathException("Type '" + type + "' not expected in literal")
        };
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
    {
        return _createValueSequence();
    }
}