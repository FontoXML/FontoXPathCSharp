using System.Xml;
using FontoXPathCSharp.Sequences;

namespace FontoXPathCSharp.Expressions;


public class SelfAxis : Expression
{
    private readonly TestAbstractExpression _selector;

    public SelfAxis(TestAbstractExpression selector)
    {
        _selector = selector;
    }

    public override ISequence Evaluate(XmlNode node, Value contextItem)
    {
        var isMatch = _selector.EvaluateToBoolean(node, contextItem);
        return isMatch ? new SingletonSequence(contextItem) : new EmptySequence();
    }
}
