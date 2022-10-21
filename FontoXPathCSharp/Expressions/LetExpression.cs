using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions;

public class LetExpression<TNode> : FlworExpression<TNode> where TNode : notnull
{
    private readonly AbstractExpression<TNode> _bindingSequence;
    private readonly QName _rangeVariable;
    private string? _variableBinding;

    public LetExpression(QName rangeVariable,
        AbstractExpression<TNode> bindingSequence,
        AbstractExpression<TNode> returnExpression)
        : base(bindingSequence.Specificity.Add(returnExpression.Specificity),
            new[] { bindingSequence, returnExpression },
            new OptimizationOptions(
                false,
                returnExpression.Peer,
                returnExpression.ExpectedResultOrder,
                returnExpression.Subtree
            ), returnExpression
        )
    {
        if (!string.IsNullOrEmpty(rangeVariable.Prefix) || !string.IsNullOrEmpty(rangeVariable.NamespaceUri))
            throw new NotImplementedException("Not implemented: let expressions with namespace usage." +
                                              $"Prefix: {rangeVariable.Prefix}, Namespace: {rangeVariable.NamespaceUri}");

        _rangeVariable = rangeVariable;
        _bindingSequence = bindingSequence;
        _variableBinding = null;
    }

    public override ISequence DoFlworExpression(
        DynamicContext dynamicContext,
        Iterator<DynamicContext> dynamicContextIterator,
        ExecutionParameters<TNode> executionParameters,
        Func<Iterator<DynamicContext>, ISequence> createReturnSequence)
    {
        return createReturnSequence(
            _ =>
            {
                var temp = dynamicContextIterator(IterationHint.None);
                if (temp.IsDone) return IteratorResult<DynamicContext>.Done();

                var currentDynamicContext = temp.Value;
                var scopedContext = currentDynamicContext!.ScopeWithVariableBindings(
                    new Dictionary<string, Func<ISequence>>
                    {
                        {
                            _variableBinding!, ISequence.CreateDoublyIterableSequence(
                                _bindingSequence.EvaluateMaybeStatically(
                                    currentDynamicContext,
                                    executionParameters
                                ))
                        }
                    });
                return IteratorResult<DynamicContext>.Ready(scopedContext);
            }
        );
    }

    public override void PerformStaticEvaluation(StaticContext<TNode> staticContext)
    {
        if (_rangeVariable.Prefix != null)
        {
            _rangeVariable.NamespaceUri = staticContext.ResolveNamespace(_rangeVariable.Prefix);

            if (_rangeVariable.NamespaceUri == null)
                throw new XPathException(
                    "XPST0081",
                    $"Could not resolve namespace for prefix {_rangeVariable.Prefix} using in a for expression");
        }

        _bindingSequence.PerformStaticEvaluation(staticContext);

        staticContext.IntroduceScope();
        _variableBinding = staticContext.RegisterVariable(_rangeVariable.NamespaceUri, _rangeVariable.LocalName);
        ReturnExpression.PerformStaticEvaluation(staticContext);
        staticContext.RemoveScope();

        IsUpdating = ReturnExpression.IsUpdating;

        if (_bindingSequence.IsUpdating)
            throw new XPathException("XUST0001", "Can not execute an updating expression in a non-updating context.");
    }
}