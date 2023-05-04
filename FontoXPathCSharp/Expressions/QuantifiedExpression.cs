using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions;

public enum QuantifierType
{
    Some,
    Every
}

public record InClause<TNode>(QName Name, AbstractExpression<TNode> SourceExpr) where TNode : notnull;

public class QuantifiedExpression<TNode> : AbstractExpression<TNode> where TNode : notnull
{
    private readonly AbstractExpression<TNode>[] _inClauseExpressions;
    private readonly QName[] _inClauseNames;
    private string[] _inClauseVariableNames;
    private readonly QuantifierType _quantifier;
    private readonly AbstractExpression<TNode> _satisfiesExpr;

    public QuantifiedExpression(
        string quantifier,
        InClause<TNode>[] inClauses,
        AbstractExpression<TNode> satisfiesExpr) : base(
        inClauses.Reduce(
            satisfiesExpr.Specificity,
            (summedSpecificity, inClause, _) => summedSpecificity.Add(inClause.SourceExpr.Specificity)),
        inClauses.Select(inClause => inClause.SourceExpr).Append(satisfiesExpr).ToArray(),
        new OptimizationOptions(false))
    {
        _quantifier = quantifier switch
        {
            "some" => QuantifierType.Some,
            "every" => QuantifierType.Every,
            _ => throw new ArgumentOutOfRangeException(nameof(quantifier), quantifier, null)
        };
        _inClauseExpressions = inClauses.Select(inClause => inClause.SourceExpr).ToArray();
        _inClauseNames = inClauses.Select(inClause => inClause.Name).ToArray();
        _satisfiesExpr = satisfiesExpr;
        _inClauseVariableNames = null;
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode>? executionParameters)
    {
        var scopingContext = dynamicContext;
        var evaluatedInClauses = _inClauseVariableNames.Select((variableBinding, i) =>
        {
            var allValuesInInClause = _inClauseExpressions[i]
                .EvaluateMaybeStatically(scopingContext, executionParameters)
                .GetAllValues();

            scopingContext = dynamicContext.ScopeWithVariableBindings(new Dictionary<string, Func<ISequence>>()
            {
                { "variableBinding", () => SequenceFactory.CreateFromArray(allValuesInInClause) }
            });

            return allValuesInInClause;
        }).ToArray();


        // If any item of evaluatedInClauses is empty stop
        if (evaluatedInClauses.Any(items => items.Length == 0))
        {
            return _quantifier == QuantifierType.Every
                ? SequenceFactory.SingletonTrueSequence
                : SequenceFactory.SingletonFalseSequence;
        }

        var indices = new int[evaluatedInClauses.Length];
        indices[0] = -1;

        var hasOverflowed = true;
        while (hasOverflowed)
        {
            hasOverflowed = false;
            var l = indices.Length;
            for (var i = 0; i < l; ++i)
            {
                var valueArray = evaluatedInClauses[i];
                if (++indices[i] > valueArray.Length - 1)
                {
                    indices[i] = 0;
                    continue;
                }

                var variables = new Dictionary<string, Func<ISequence>>();

                for (var y = 0; y < l; y++)
                {
                    var value = evaluatedInClauses[y][indices[y]];
                    variables[_inClauseVariableNames[y]] = () =>
                        SequenceFactory.CreateFromValue(value);
                }

                var context = dynamicContext.ScopeWithVariableBindings(variables);
                var result = _satisfiesExpr.EvaluateMaybeStatically(
                    context,
                    executionParameters
                );

                if (result.GetEffectiveBooleanValue() && _quantifier == QuantifierType.Some)
                {
                    return SequenceFactory.SingletonTrueSequence;
                }

                if (!result.GetEffectiveBooleanValue() && _quantifier == QuantifierType.Every)
                {
                    return SequenceFactory.SingletonFalseSequence;
                }

                hasOverflowed = true;
                break;
            }
        }

        // An every quantifier is true if all items match, a some is false if none of the items match
        return _quantifier == QuantifierType.Every
            ? SequenceFactory.SingletonTrueSequence
            : SequenceFactory.SingletonFalseSequence;
    }

    public override void PerformStaticEvaluation(StaticContext<TNode> staticContext)
    {
        var l = _inClauseNames.Length;
        _inClauseVariableNames = new string[l];
        for (var i = 0; i < l; ++i)
        {
            var expr = _inClauseExpressions[i];
            expr.PerformStaticEvaluation(staticContext);

            // The existance of this variable should be known for the next expression
            staticContext.IntroduceScope();
            var inClauseName = _inClauseNames[i];
            var inClauseNameNamespaceURI = inClauseName.Prefix != null
                ? staticContext.ResolveNamespace(inClauseName.Prefix)
                : null;
            var varBindingName = staticContext.RegisterVariable(
                inClauseNameNamespaceURI,
                inClauseName.LocalName
            );
            _inClauseVariableNames[i] = varBindingName;
        }

        _satisfiesExpr.PerformStaticEvaluation(staticContext);
        for (var i = 0; i < l; ++i)
        {
            staticContext.RemoveScope();
        }
    }
}