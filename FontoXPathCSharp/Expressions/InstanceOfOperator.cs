using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value.Types;

namespace FontoXPathCSharp.Expressions;

public class InstanceOfOperator : AbstractExpression
{
    private AbstractExpression _expression;
    private SequenceMultiplicity _multiplicity;
    private AbstractExpression _typeTest;

    public InstanceOfOperator(AbstractExpression expression, AbstractExpression typeTest, string multiplicity) : base(
        new[] { expression }, new OptimizationOptions(canBeStaticallyEvaluated: false))
    {
        _expression = expression;
        _typeTest = typeTest;
        _multiplicity = multiplicity switch
        {
            "" => SequenceMultiplicity.ExactlyOne,
            "?" => SequenceMultiplicity.ZeroOrOne,
            "*" => SequenceMultiplicity.ZeroOrMore,
            "+" => SequenceMultiplicity.OneOrMore,
            _ => throw new XPathException(
                $"InstanceOfOperator, somehow {multiplicity} got passed in as a multiplicity.")
        };
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
    {
        var evaluatedExpression = _expression.EvaluateMaybeStatically(dynamicContext, executionParameters);

        if (evaluatedExpression.IsEmpty())
        {
            return _multiplicity is SequenceMultiplicity.ZeroOrOne or SequenceMultiplicity.ZeroOrOne
                ? SequenceFactory.SingletonTrueSequence
                : SequenceFactory.SingletonFalseSequence;
        }

        if (evaluatedExpression.IsSingleton() ||
            _multiplicity is SequenceMultiplicity.OneOrMore or SequenceMultiplicity.ZeroOrMore)
        {
            return evaluatedExpression.Every(value =>
            {
                var contextItem = SequenceFactory.CreateFromValue(value);
                var scopedContext = dynamicContext?.ScopeWithFocus(0, value, contextItem);
                return _typeTest.EvaluateMaybeStatically(
                    scopedContext,
                    executionParameters
                );
            });
        }

        return SequenceFactory.SingletonFalseSequence;
    }
}