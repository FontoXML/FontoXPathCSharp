using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions;

public class NamedFunctionRef : AbstractExpression
{
    private readonly int _arity;
    private readonly QName _functionReference;

    public NamedFunctionRef(QName functionReference, int arity) : base(Array.Empty<AbstractExpression>(),
        new OptimizationOptions(true))
    {
        _functionReference = functionReference;
        _arity = arity;
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
    {
        Console.WriteLine(_functionReference.LocalName);
        throw new NotImplementedException();
    }
}