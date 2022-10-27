using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.Tests;

public class PiTest<TNode> : AbstractTestExpression<TNode> where TNode : notnull
{
    private readonly string _target;

    public PiTest(string target) : base(new Specificity(
        new Dictionary<SpecificityKind, int> { { SpecificityKind.NodeName, 1 } }))
    {
        _target = target;
    }

    protected internal override bool EvaluateToBoolean(
        DynamicContext? dynamicContext,
        AbstractValue node,
        ExecutionParameters<TNode>? executionParameters)
    {
        // Assume singleton
        var isMatchingProcessingInstruction =
            node.GetValueType().IsSubtypeOf(ValueType.ProcessingInstruction) &&
            executionParameters!.DomFacade.GetTarget(node.GetAs<NodeValue<TNode>>().Value) == _target;
        return isMatchingProcessingInstruction;
    }

    public override string GetBucket()
    {
        return "type-7";
    }
}