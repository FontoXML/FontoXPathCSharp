using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value.Types;

namespace FontoXPathCSharp.Expressions;

public class InstanceOfOperator<TNode> : AbstractExpression<TNode>
{
    private readonly AbstractExpression<TNode> _expression;
    private readonly SequenceMultiplicity _multiplicity;
    private readonly AbstractExpression<TNode> _typeTest;

    public InstanceOfOperator(AbstractExpression<TNode> expression, AbstractExpression<TNode> typeTest,
        string multiplicity) : base(
        expression.Specificity,
        new[] { expression },
        new OptimizationOptions(false))
    {
        _expression = expression;
        _typeTest = typeTest;
        _multiplicity = multiplicity switch
        {
            "" => SequenceMultiplicity.ExactlyOne,
            "?" => SequenceMultiplicity.ZeroOrOne,
            "*" => SequenceMultiplicity.ZeroOrMore,
            "+" => SequenceMultiplicity.OneOrMore,
            _ => throw new ArgumentOutOfRangeException(
                $"InstanceOfOperator, somehow {multiplicity} got passed in as a multiplicity.")
        };
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode> executionParameters)
    {
        var evaluatedExpression = _expression.EvaluateMaybeStatically(dynamicContext, executionParameters);

        if (evaluatedExpression.IsEmpty())
            return _multiplicity is SequenceMultiplicity.ZeroOrOne or SequenceMultiplicity.ZeroOrOne
                ? SequenceFactory.SingletonTrueSequence
                : SequenceFactory.SingletonFalseSequence;

        if (evaluatedExpression.IsSingleton() ||
            _multiplicity is SequenceMultiplicity.OneOrMore or SequenceMultiplicity.ZeroOrMore)
            return evaluatedExpression.Every(value =>
            {
                var contextItem = SequenceFactory.CreateFromValue(value);
                var scopedContext = dynamicContext?.ScopeWithFocus(0, value, contextItem);
                return _typeTest.EvaluateMaybeStatically(
                    scopedContext,
                    executionParameters
                );
            });

        return SequenceFactory.SingletonFalseSequence;
    }
}