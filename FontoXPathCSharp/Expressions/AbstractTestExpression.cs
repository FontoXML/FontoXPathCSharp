namespace FontoXPathCSharp.Expressions;

using System.Xml;
using Sequences;
using Value;

public abstract class AbstractTestExpression : Expression
{
    public override ISequence Evaluate(XmlNode node, AbstractValue contextItem)
    {
        return new SingletonSequence(new BooleanValue(EvaluateToBoolean(node, contextItem)));
    }

    protected internal abstract bool EvaluateToBoolean(XmlNode node, AbstractValue contextItem);
}