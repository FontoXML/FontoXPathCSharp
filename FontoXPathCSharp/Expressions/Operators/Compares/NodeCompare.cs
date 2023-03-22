using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Expressions.DataTypes;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.Operators.Compares;

public enum NodeCompareType
{
    IsOp,
    NodeBeforeOp,
    NodeAfterOp
}

public class NodeCompare<TNode> : AbstractExpression<TNode> where TNode : notnull
{
    private readonly AbstractExpression<TNode> _firstExpression;
    private readonly NodeCompareType _operator;
    private readonly AbstractExpression<TNode> _secondExpression;

    public NodeCompare(
        NodeCompareType nodeOperator,
        AbstractExpression<TNode> firstExpression,
        AbstractExpression<TNode> secondExpression) : base(
        firstExpression.Specificity.Add(secondExpression.Specificity),
        new[] { firstExpression, secondExpression },
        new OptimizationOptions(false))
    {
        _firstExpression = firstExpression;
        _secondExpression = secondExpression;
        _operator = nodeOperator;
    }

    public override ISequence Evaluate(
        DynamicContext? dynamicContext,
        ExecutionParameters<TNode>? executionParameters)
    {
        var firstSequence = _firstExpression.EvaluateMaybeStatically(
            dynamicContext,
            executionParameters
        );
        var secondSequence = _secondExpression.EvaluateMaybeStatically(
            dynamicContext,
            executionParameters
        );

        if (firstSequence.IsEmpty() || secondSequence.IsEmpty()) return SequenceFactory.CreateEmpty();
        if (!firstSequence.IsSingleton() || !secondSequence.IsSingleton())
            throw new XPathException("XPTY0004", "Sequences to compare are not singleton");

        var first = firstSequence.First();
        var second = secondSequence.First();
        var compareFunction = GetNodeCompareFunction(
            _operator,
            executionParameters.DomFacade,
            first.GetValueType(),
            second.GetValueType()
        );
        return compareFunction(firstSequence, secondSequence, dynamicContext)
            ? SequenceFactory.SingletonTrueSequence
            : SequenceFactory.SingletonFalseSequence;
    }

    private static Func<ISequence, ISequence, DynamicContext?, bool>? GetNodeCompareFunction(
        NodeCompareType nodeCompare,
        DomFacade<TNode>? domFacade,
        ValueType first,
        ValueType second)
    {
        if (!first.IsSubtypeOf(ValueType.Node) || !second.IsSubtypeOf(ValueType.Node))
            throw new XPathException("XPTY0004", "Sequences to compare are not nodes");

        return nodeCompare switch
        {
            NodeCompareType.IsOp => GetIsOpHandler(first, second),
            NodeCompareType.NodeBeforeOp => domFacade == null
                ? null
                : (firstSequenceParam, secondSequenceParam, _) => DocumentOrderUtils<TNode>.CompareNodePositions(
                    domFacade,
                    firstSequenceParam.First()!.GetAs<NodeValue<TNode>>(),
                    secondSequenceParam.First()!.GetAs<NodeValue<TNode>>()
                ) < 0,
            NodeCompareType.NodeAfterOp => domFacade == null
                ? null
                : (firstSequenceParam, secondSequenceParam, _) => DocumentOrderUtils<TNode>.CompareNodePositions(
                    domFacade,
                    firstSequenceParam.First()!.GetAs<NodeValue<TNode>>(),
                    secondSequenceParam.First()!.GetAs<NodeValue<TNode>>()
                ) > 0
        };
    }

    private static Func<ISequence, ISequence, DynamicContext?, bool> GetIsOpHandler(ValueType first, ValueType second)
    {
        if (first == second &&
            first is ValueType.Attribute
                or ValueType.Node
                or ValueType.Element
                or ValueType.DocumentNode
                or ValueType.Text
                or ValueType.ProcessingInstruction
                or ValueType.Comment
           )
            return (
                firstSequenceParam,
                secondSequenceParam,
                _
            ) => SortedSequenceUtils<TNode>.AreNodesEqual(
                firstSequenceParam.First()!.GetAs<NodeValue<TNode>>(),
                secondSequenceParam.First()!.GetAs<NodeValue<TNode>>()
            );
        return (_, _, _) => false;
    }
}