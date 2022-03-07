using System.Xml;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions;

public class ParentAxis : AbstractExpression
{
    private readonly AbstractTestAbstractExpression _selector;

    public ParentAxis(AbstractTestAbstractExpression selector)
    {
        _selector = selector;
    }

    public override ISequence Evaluate(XmlNode documentNode, AbstractValue contextItem)
    {
        var parentNode = documentNode.ParentNode;
        if (parentNode == null)
        {
            return new EmptySequence();
        }

        var isMatch = _selector.EvaluateToBoolean(parentNode, contextItem);
        return isMatch ? new SingletonSequence(contextItem) : new EmptySequence();
    }
}