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

    public override ISequence Evaluate(XmlNode documentNode, AbstractValue contextItem)
    {
        return _stepExpressions.Aggregate(new ArrayBackedSequence(new[] {contextItem}),
            (contextItems, step) =>
            {
                return new ArrayBackedSequence(contextItems
                    .SelectMany(c => (IEnumerable<AbstractValue>) step.Evaluate(documentNode, c)).ToArray());
            });
    }
}