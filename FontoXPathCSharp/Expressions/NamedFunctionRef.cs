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
        if (_functionReference.LocalName == "test")
        {
            return new SingletonSequence(new FunctionValue<ISequence>(0, (context, parameters, staticContext, args) =>
            {
                Console.WriteLine("Called test function");
                return new EmptySequence();
            }));
        }

        throw new NotImplementedException();
    }
}