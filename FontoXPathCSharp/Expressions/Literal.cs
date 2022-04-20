using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions;

public class Literal : AbstractExpression
{
    private readonly Func<ISequence> CreateValueSequence;

    public Literal(string value, SequenceType type) : base(Array.Empty<AbstractExpression>(),
        new OptimizationOptions(true))
    {
        switch (type.ValueType)
        {
            case ValueType.XsInteger:
                CreateValueSequence = () => new SingletonSequence(new IntValue(int.Parse(value)));
                break;
            default:
                throw new XPathException("Type '" + type + "' not expected in literal");
        }
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
    {
        return CreateValueSequence();
    }
}