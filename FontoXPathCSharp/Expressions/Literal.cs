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
        switch (type.ValueType)
        {
            case ValueType.XsInteger:
                _createValueSequence = () => new SingletonSequence(new IntValue(int.Parse(value)));
                break;
            case ValueType.XsString:
                _createValueSequence = () => new SingletonSequence(new StringValue(value));
                break;
            default:
                throw new XPathException("Type '" + type + "' not expected in literal");
        }
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
    {
        return _createValueSequence();
    }
}