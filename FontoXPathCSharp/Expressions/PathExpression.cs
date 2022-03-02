namespace FontoXPathCSharp.Expressions;

using System.Diagnostics;
using System.Xml;
using Sequences;
using Value;

public class PathExpression : Expression
{
    private readonly Expression[] _stepExpressions;

    public PathExpression(Expression[] stepExpressions)
    {
        _stepExpressions = stepExpressions;
    }

    public override ISequence Evaluate(XmlNode node, AbstractValue contextItem)
    {
        Debug.Assert(_stepExpressions.Length == 1);
        return new ArrayBackedSequence(Array.Empty<AbstractValue>());
    }
}