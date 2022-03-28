using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions;

public class NamedFunctionRef : AbstractExpression
{
    private readonly int _arity;
    private readonly QName _functionReference;
    private FunctionProperties? _functionProperties;

    public NamedFunctionRef(QName functionReference, int arity) : base(Array.Empty<AbstractExpression>(),
        new OptimizationOptions(true))
    {
        _arity = arity;
        _functionReference = functionReference;
        _functionProperties = null;
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters? executionParameters)
    {
        var functionProps = _functionProperties!.Value;
        var functionItem = new FunctionValue<ISequence>(functionProps.ArgumentTypes, functionProps.Arity,
            functionProps.CallFunction);
        return new SingletonSequence(functionItem);
    }

    public override void PerformStaticEvaluation(StaticContext staticContext)
    {
        if (_functionReference.NamespaceUri == null)
        {
            // TODO: resolve function name
            throw new NotImplementedException();
        }

        _functionProperties =
            staticContext.LookupFunction(_functionReference.NamespaceUri, _functionReference.LocalName, _arity);

        base.PerformStaticEvaluation(staticContext);
    }
}