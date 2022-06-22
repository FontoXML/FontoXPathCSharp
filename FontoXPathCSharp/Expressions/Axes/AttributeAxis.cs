using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.Axes;

public class AttributeAxis : AbstractExpression
{
    private readonly AbstractTestExpression _selector;

    public AttributeAxis(AbstractTestExpression selector) : base(new AbstractExpression[] {selector},
        new OptimizationOptions(false))
    {
        _selector = selector;
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
    {
        ContextNodeUtils.ValidateContextNode(dynamicContext!.ContextItem!);
        var contextNode = dynamicContext.ContextItem.GetAs<NodeValue>(ValueType.Node)!.Value();

        throw new NotImplementedException("AttributeAxis Evaluate");
    }
}