using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions;

public class LetExpression<TNode> : FlworExpression<TNode>
{
    public readonly AbstractExpression<TNode> BindingSequence;
    public readonly QName RangeVariable;
    public string? VariableBinding {get ; protected set; }

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
        if (rangeVariable.Prefix != null || rangeVariable.NamespaceUri != null)
            throw new NotImplementedException("Not implemented: let expressions with namespace usage.");

        RangeVariable = rangeVariable;
        BindingSequence = bindingSequence;
        VariableBinding = null;
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
                            VariableBinding!, ISequence.CreateDoublyIterableSequence(
                                BindingSequence.EvaluateMaybeStatically(
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
        if (RangeVariable.Prefix != null)
        {
            RangeVariable.NamespaceUri = staticContext.ResolveNamespace(RangeVariable.Prefix);

            if (RangeVariable.NamespaceUri == null)
                throw new XPathException(
                    "XPST0081",
                    $"Could not resolve namespace for prefix {RangeVariable.Prefix} using in a for expression");
        }

        BindingSequence.PerformStaticEvaluation(staticContext);

        staticContext.IntroduceScope();
        VariableBinding = staticContext.RegisterVariable(RangeVariable.NamespaceUri, RangeVariable.LocalName);
        ReturnExpression.PerformStaticEvaluation(staticContext);
        staticContext.RemoveScope();

        IsUpdating = ReturnExpression.IsUpdating;

        if (BindingSequence.IsUpdating) throw new XPathException("XUST0001","Can not execute an updating expression in a non-updating context.");
    }
}