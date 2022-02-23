using System.Xml;
using FontoXPathCSharp.Sequences;

namespace FontoXPathCSharp.Expressions;

public abstract class TestAbstractExpression : Expression
{
    public override ISequence Evaluate(XmlNode node, Value contextItem)
    {
        return new SingletonSequence(new Value(EvaluateToBoolean(node, contextItem), ValueType.XSBOOLEAN));
    }

    protected internal abstract bool EvaluateToBoolean(XmlNode node, Value contextItem);
}
