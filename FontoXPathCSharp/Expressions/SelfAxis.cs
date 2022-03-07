namespace FontoXPathCSharp.Expressions;

using System.Xml;
using Sequences;
using Value;

public class SelfAxis : AbstractExpression
{
    private readonly AbstractTestAbstractExpression _selector;

    public SelfAxis(AbstractTestAbstractExpression selector)
    {
        _selector = selector;
    }

    public override ISequence Evaluate(XmlNode node, AbstractValue contextItem)
    {
        var isMatch = _selector.EvaluateToBoolean(node, contextItem);
        return isMatch ? new SingletonSequence(contextItem) : new EmptySequence();
    }
}