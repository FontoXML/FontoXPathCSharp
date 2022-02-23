using System.Xml;
using FontoXPathCSharp.Sequences;

namespace FontoXPathCSharp.Expressions;

public abstract class Expression
{
    public abstract ISequence Evaluate(XmlNode node, Value contextItem);
}