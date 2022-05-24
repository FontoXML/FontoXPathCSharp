using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Types;
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
        var functionProps = _functionProperties!;
        var functionItem = new FunctionValue<ISequence>(functionProps.ArgumentTypes, functionProps.Arity,
            functionProps.CallFunction);
        return SequenceFactory.CreateFromValue(functionItem);
    }

    public override void PerformStaticEvaluation(StaticContext staticContext)
    {
        var namespaceUri = _functionReference.NamespaceUri;
        var localName = _functionReference.LocalName;
        var prefix = _functionReference.Prefix;

        if (namespaceUri == null)
        {
            var functionName = staticContext.ResolveFunctionName(new LexicalQualifiedName(localName, prefix), _arity);

            if (functionName == null)
                throw new XPathException("XPST0017: The function " + (prefix == null ? "" : prefix + ":") + localName +
                                         " with arity " + _arity + " could not be resolved.");

            namespaceUri = functionName.NamespaceUri;
            localName = functionName.LocalName;
        }

        _functionProperties =
            staticContext.LookupFunction(namespaceUri, localName, _arity, false);

        if (_functionProperties == null)
            throw new XPathException("XPST0017: The function " + (prefix == null ? "" : prefix + ":") + localName +
                                     " with arity " + _arity + " is not registered.");

        base.PerformStaticEvaluation(staticContext);
    }
}