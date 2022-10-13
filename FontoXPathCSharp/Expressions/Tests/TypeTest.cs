using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;

namespace FontoXPathCSharp.Expressions.Tests;

public class TypeTest<TNode> : AbstractTestExpression<TNode> where TNode : notnull
{
    private readonly QName _type;

    public TypeTest(QName type) : base(new Specificity())
    {
        _type = type;
    }

    protected internal override bool EvaluateToBoolean(DynamicContext? dynamicContext, AbstractValue value,
        ExecutionParameters<TNode> executionParameters)
    {
        return value.GetValueType()
            .IsSubtypeOf((_type.Prefix == null ? _type.LocalName : _type.Prefix + ":" + _type.LocalName)
                .StringToValueType());
    }
}