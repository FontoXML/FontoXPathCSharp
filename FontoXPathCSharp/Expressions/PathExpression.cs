using System.Diagnostics;
using System.Xml;
using FontoXPathCSharp.Sequences;

namespace FontoXPathCSharp.Expressions;

public class PathExpression : Expression
{
    private readonly Expression[] _stepExpressions;

    public PathExpression(Expression[] stepExpressions)
    {
        _stepExpressions = stepExpressions;
    }

    public override ISequence Evaluate(XmlNode node, Value contextItem)
    {
        Debug.Assert(_stepExpressions.Length == 1);
        return new ArrayBackedSequence(Array.Empty<Value>());
    }
}