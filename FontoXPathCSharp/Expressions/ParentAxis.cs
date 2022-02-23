using System.Xml;
using FontoXPathCSharp.Sequences;

namespace FontoXPathCSharp.Expressions;

public class ParentAxis : Expression
{
    private readonly AbstractTestExpression _selector;

    public ParentAxis(AbstractTestExpression selector)
    {
        _selector = selector;
    }

    public override ISequence Evaluate(XmlNode node, Value contextItem)
    {
        var parentNode = node.ParentNode;
        if (parentNode == null)
        {
            return new EmptySequence();
        }

        var isMatch = _selector.EvaluateToBoolean(parentNode, contextItem);
        return isMatch ? new SingletonSequence(contextItem) : new EmptySequence();
    }
}
