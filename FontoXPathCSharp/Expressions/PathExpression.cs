using System.Xml;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions;

public class PathExpression : AbstractExpression
{
    private readonly AbstractExpression[] _stepExpressions;

    public PathExpression(AbstractExpression[] stepExpressions)
    {
        _stepExpressions = stepExpressions;
    }

    public override ISequence Evaluate(XmlNode node, AbstractValue contextItem)
    {
        return _stepExpressions[0].Evaluate(node, contextItem);
    }
}