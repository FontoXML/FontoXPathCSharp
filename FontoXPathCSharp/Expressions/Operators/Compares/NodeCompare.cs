using FontoXPathCSharp.Sequences;

namespace FontoXPathCSharp.Expressions.Operators.Compares;

public class NodeCompare<TNode> : AbstractExpression<TNode>
{
    //TODO: Implement Node Comparison Expression
    public NodeCompare(Specificity specificity, AbstractExpression<TNode>[] childExpressions, OptimizationOptions optimizationOptions) : base(specificity, childExpressions, optimizationOptions)
    {
        throw new NotImplementedException();
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode> executionParameters)
    {
        throw new NotImplementedException();
    }
}