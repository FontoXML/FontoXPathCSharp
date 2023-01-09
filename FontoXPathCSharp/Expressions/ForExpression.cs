using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions;

public class ForExpression<TNode> : FlworExpression<TNode> where TNode : notnull
{
    private readonly AbstractExpression<TNode> _clauseExpression;
    private readonly QName? _positionalVariableBinding;
    private readonly QName _rangeVariable;
    private string? _positionalVariableBindingKey;

    private string? _variableBindingKey;

    public ForExpression(
        QName rangeVariable,
        AbstractExpression<TNode> clauseExpression,
        QName positionalVariableBinding,
        AbstractExpression<TNode> returnExpression) : base(
        clauseExpression.Specificity.Add(returnExpression.Specificity),
        new[] { clauseExpression, returnExpression },
        new OptimizationOptions(false),
        returnExpression
    )
    {
        _rangeVariable = rangeVariable;
        _variableBindingKey = null;
        _positionalVariableBinding = positionalVariableBinding;
        _positionalVariableBindingKey = null;
        _clauseExpression = clauseExpression;
    }

    public override ISequence DoFlworExpression(
        DynamicContext dynamicContext,
        Iterator<DynamicContext> dynamicContextIterator,
        ExecutionParameters<TNode> executionParameters,
        Func<Iterator<DynamicContext>, ISequence> createReturnSequence)
    {
        Iterator<AbstractValue>? clauseIterator = null;
        DynamicContext? currentDynamicContext = null;

        var position = 0;
        return createReturnSequence(
            _ =>
            {
                while (true)
                {
                    if (clauseIterator == null)
                    {
                        var temp = dynamicContextIterator(IterationHint.None);
                        if (temp.IsDone) return IteratorResult<DynamicContext>.Done();
                        currentDynamicContext = temp.Value;

                        position = 0;

                        clauseIterator = _clauseExpression.EvaluateMaybeStatically(
                            currentDynamicContext,
                            executionParameters
                        ).GetValue();
                    }

                    var currentClauseValue = clauseIterator(IterationHint.None);
                    if (currentClauseValue.IsDone)
                    {
                        clauseIterator = null;
                        continue;
                    }

                    position++;

                    var variables = new Dictionary<string, Func<ISequence>>();
                    if (_variableBindingKey != null)
                        variables[_variableBindingKey] =
                            () => SequenceFactory.CreateFromValue(currentClauseValue.Value);

                    if (_positionalVariableBindingKey != null)
                        variables[_positionalVariableBindingKey] = () =>
                            SequenceFactory.CreateFromValue(new IntegerValue(position, ValueType.XsInt));
                    return IteratorResult<DynamicContext>.Ready(
                        currentDynamicContext!.ScopeWithVariableBindings(variables));
                }
            });
    }

    public override void PerformStaticEvaluation(StaticContext<TNode> staticContext)
    {
        if (!string.IsNullOrEmpty(_rangeVariable.Prefix))
        {
            _rangeVariable.NamespaceUri = staticContext.ResolveNamespace(_rangeVariable.Prefix);

            if (string.IsNullOrEmpty(_rangeVariable.NamespaceUri))
                throw new XPathException(
                    "XPST0081",
                    $"Could not resolve namespace for prefix {_rangeVariable.Prefix} in a for expression"
                );
        }

        _clauseExpression.PerformStaticEvaluation(staticContext);
        staticContext.IntroduceScope();
        _variableBindingKey = staticContext.RegisterVariable(
            _rangeVariable.NamespaceUri,
            _rangeVariable.LocalName
        );

        if (_positionalVariableBinding != null)
        {
            if (!string.IsNullOrEmpty(_positionalVariableBinding.Prefix))
            {
                _positionalVariableBinding.NamespaceUri = staticContext.ResolveNamespace(
                    _positionalVariableBinding.Prefix
                );

                if (string.IsNullOrEmpty(_positionalVariableBinding.NamespaceUri) &&
                    !string.IsNullOrEmpty(_positionalVariableBinding.Prefix))
                    throw new XPathException(
                        "XPST0081",
                        $"Could not resolve namespace for prefix {_rangeVariable.Prefix} in the positionalVariableBinding in a for expression");
            }

            _positionalVariableBindingKey = staticContext.RegisterVariable(
                _positionalVariableBinding.NamespaceUri,
                _positionalVariableBinding.LocalName
            );
        }

        ReturnExpression.PerformStaticEvaluation(staticContext);
        staticContext.RemoveScope();

        if (_clauseExpression.IsUpdating)
            throw new XPathException(
                "XUST0001",
                "Can not execute an updating expression in a non-updating context.");
        if (ReturnExpression.IsUpdating) IsUpdating = true;
    }
}