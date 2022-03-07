namespace FontoXPathCSharp.Expressions;

using System.Xml;
using Sequences;
using Value;

public abstract class AbstractTestAbstractExpression : AbstractExpression
{
    public override ISequence Evaluate(XmlNode documentNode, AbstractValue contextItem)
    {
        return new SingletonSequence(new BooleanValue(EvaluateToBoolean(documentNode, contextItem)));
    }

    protected internal abstract bool EvaluateToBoolean(XmlNode node, AbstractValue contextItem);
}