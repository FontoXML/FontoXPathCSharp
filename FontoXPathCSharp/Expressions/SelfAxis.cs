namespace FontoXPathCSharp.Expressions;

using System.Xml;
using Sequences;
using Value;

public class SelfAxis : Expression
{
    private readonly AbstractTestExpression _selector;

    public SelfAxis(AbstractTestExpression selector)
    {
        _selector = selector;
    }

    public override ISequence Evaluate(XmlNode node, AbstractValue contextItem)
    {
        var isMatch = _selector.EvaluateToBoolean(node, contextItem);
        return isMatch ? new SingletonSequence(contextItem) : new EmptySequence();
    }
}