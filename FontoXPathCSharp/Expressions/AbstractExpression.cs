namespace FontoXPathCSharp.Expressions;

using System.Xml;
using Sequences;
using Value;

public abstract class AbstractExpression
{
    public abstract ISequence Evaluate(XmlNode node, AbstractValue contextItem);
}