using System.Xml;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions.Axes;

public class AncestorAxis : AbstractExpression
{
    private readonly AbstractTestExpression _ancestorExpression;
    private readonly bool _inclusive;

    public AncestorAxis(AbstractTestExpression ancestorExpression, bool inclusive) : base(
        new AbstractExpression[] {ancestorExpression}, new OptimizationOptions(false)
    )
    {
        _ancestorExpression = ancestorExpression;
        _inclusive = inclusive;
    }

    private static Iterator<AbstractValue> GenerateAncestors(XmlNode? contextPointer)
    {
        var ancestor = contextPointer;
        return _ =>
        {
            if (ancestor == null) return IteratorResult<AbstractValue>.Done();

            var previousAncestor = ancestor;
            ancestor = ancestor.ParentNode;

            return IteratorResult<AbstractValue>.Ready(new NodeValue(previousAncestor));
        };
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
    {
        var contextItem = ContextNodeUtils.ValidateContextNode(dynamicContext!.ContextItem!);

        return SequenceFactory
            .CreateFromIterator(GenerateAncestors(_inclusive ? contextItem.Value : contextItem.Value.ParentNode)).Filter(
                (item, _, _) => _ancestorExpression.EvaluateToBoolean(dynamicContext, item, executionParameters));
    }
}