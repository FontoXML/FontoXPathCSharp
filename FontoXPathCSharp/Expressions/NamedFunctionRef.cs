using System.Xml;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions;

public class NamedFunctionRef : AbstractExpression
{
    private readonly QName _functionReference;
    private readonly int _arity;

    public NamedFunctionRef(QName functionReference, int arity)
    {
        _functionReference = functionReference;
        _arity = arity;
    }

    public override ISequence Evaluate(DynamicContext dynamicContext, ExecutionParameters executionParameters)
    {
        Console.WriteLine(_functionReference.LocalName);
        throw new NotImplementedException();
    }
}