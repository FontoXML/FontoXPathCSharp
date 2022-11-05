using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

namespace FontoXPathCSharp.Expressions;

public class VarRef<TNode> : AbstractExpression<TNode> where TNode : notnull
{
    private readonly QName _qualifiedName;
    private Func<DynamicContext, ExecutionParameters<TNode>, ISequence>? _staticallyBoundVariableValue;
    private string? _variableBindingName;

    public VarRef(QName qualifiedName) : base(
        new Specificity(),
        Array.Empty<AbstractExpression<TNode>>(),
        new OptimizationOptions(
            false
        ))
    {
        _qualifiedName = qualifiedName;
        _variableBindingName = null;
        _staticallyBoundVariableValue = null;
    }

    public override ISequence Evaluate(DynamicContext? dynamicContext, ExecutionParameters<TNode>? executionParameters)
    {
        Func<ISequence>? variableBinding = null;
        if (_variableBindingName != null && dynamicContext!.VariableBindings.ContainsKey(_variableBindingName!))
            variableBinding = dynamicContext.VariableBindings[_variableBindingName!];

        // Make dynamic variables take precedence
        if (variableBinding == null)
        {
            return _staticallyBoundVariableValue != null
                ? _staticallyBoundVariableValue(dynamicContext!, executionParameters!)
                : throw new XPathException(
                    "XQDY0054",
                    $"The variable '{_qualifiedName.LocalName}' is declared but not in scope."
                );
        }

        return variableBinding();
    }

    public override void PerformStaticEvaluation(StaticContext<TNode> staticContext)
    {
        if (string.IsNullOrEmpty(_qualifiedName.NamespaceUri) && !string.IsNullOrEmpty(_qualifiedName.Prefix))
            _qualifiedName.NamespaceUri = staticContext.ResolveNamespace(_qualifiedName.Prefix);

        _variableBindingName = staticContext.LookupVariable(
            _qualifiedName.NamespaceUri ?? string.Empty,
            _qualifiedName.LocalName
        );

        if (_variableBindingName == null)
            throw new XPathException(
                "XPST0008", 
                $"The variable '{_qualifiedName.LocalName}' is not in scope."
                );

        var staticallyBoundVariableBinding = staticContext.GetVariableDeclaration(
            _variableBindingName
        );

        if (staticallyBoundVariableBinding != null) _staticallyBoundVariableValue = staticallyBoundVariableBinding;
    }
}