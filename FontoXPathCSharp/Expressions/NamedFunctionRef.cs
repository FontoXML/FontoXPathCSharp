using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions;

public class NamedFunctionRef<TNode> : AbstractExpression<TNode>
{
    private readonly int _arity;
    private readonly QName _functionReference;
    private FunctionProperties<TNode>? _functionProperties;

    public NamedFunctionRef(QName functionReference, int arity) : base(Array.Empty<AbstractExpression<TNode>>(),
        new OptimizationOptions(true))
    {
        _arity = arity;
        _functionReference = functionReference;
        _functionProperties = null;
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode> executionParameters)
    {
        return SequenceFactory.CreateFromValue(new FunctionValue<ISequence, TNode>(
            _functionProperties!.ArgumentTypes,
            _functionProperties!.Arity,
            _functionProperties!.LocalName,
            _functionProperties!.NamespaceUri,
            _functionProperties!.ReturnType,
            _functionProperties!.CallFunction,
            _functionProperties!.IsUpdating));
    }

    public override void PerformStaticEvaluation(StaticContext<TNode> staticContext)
    {
        var namespaceUri = _functionReference.NamespaceUri;
        var localName = _functionReference.LocalName;
        var prefix = _functionReference.Prefix;

        if (namespaceUri == null)
        {
            var functionName = staticContext.ResolveFunctionName(new LexicalQualifiedName(localName, prefix), _arity);

            if (functionName == null)
                throw new XPathException(
                    "XPST0017",$"The function {(string.IsNullOrEmpty(prefix) ? "" : prefix + ":")}{localName} 1with arity {_arity} could not be resolved.");

            namespaceUri = functionName.NamespaceUri;
            localName = functionName.LocalName;
        }

        _functionProperties =
            staticContext.LookupFunction(namespaceUri, localName, _arity);

        if (_functionProperties == null)
            throw new XPathException(
                "XPST0017", $"The function {(string.IsNullOrEmpty(prefix) ? "" : prefix + ":")}{localName} with arity {_arity} is not registered.");


        base.PerformStaticEvaluation(staticContext);
    }
}