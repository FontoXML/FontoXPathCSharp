using System.Xml;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions;

public class FunctionCall :  AbstractExpression
{
    private readonly AbstractExpression _functionReferenceExpression;

    public FunctionCall(AbstractExpression functionReferenceExpression)
    {
        _functionReferenceExpression = functionReferenceExpression;
    }

    public override ISequence Evaluate(XmlNode documentNode, AbstractValue contextItem)
    {
        _functionReferenceExpression.Evaluate(documentNode, contextItem);
        throw new NotImplementedException();
    }
}