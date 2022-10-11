using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Functions;

public static class BuiltInFunctionsNode<TNode>
{
    public static readonly FunctionSignature<ISequence, TNode> FnNodeName = (_, executionParameters, _, args) =>
    {
        return ISequence.ZipSingleton(args, pointerValueList =>
        {
            if (pointerValueList.Count == 0) return SequenceFactory.CreateEmpty();

            var pointerValue = pointerValueList.First();

            var domFacade = executionParameters.DomFacade;
            var pointer = pointerValue.GetAs<NodeValue<TNode>>().Value;
            return domFacade.GetNodeType(pointer) switch
            {
                NodeType.Element =>
                    // element or attribute
                    SequenceFactory.CreateFromValue(AtomicValue.Create(
                        new QName(
                            domFacade.GetPrefix(pointer) ?? string.Empty,
                            domFacade.GetNamespaceUri(pointer),
                            domFacade.GetLocalName(pointer)
                        ),
                        ValueType.XsQName)),
                NodeType.Attribute =>
                    // element or attribute
                    SequenceFactory.CreateFromValue(AtomicValue.Create(
                        new QName(
                            domFacade.GetPrefix(pointer) ?? string.Empty,
                            domFacade.GetNamespaceUri(pointer),
                            domFacade.GetLocalName(pointer)
                        ),
                        ValueType.XsQName)),
                NodeType.ProcessingInstruction =>
                    // A processing instruction's target is its nodename (https://www.w3.org/TR/xpath-functions-31/#func-node-name)
                    SequenceFactory.CreateFromValue(AtomicValue.Create(
                        new QName(
                            string.Empty,
                            string.Empty,
                            domFacade.GetTarget(pointer)
                        ),
                        ValueType.XsQName)),
                _ => SequenceFactory.CreateEmpty()
            };
        });
    };

    public static readonly FunctionSignature<ISequence, TNode> FnName =
        (dynamicContext, executionParameters, staticContext, args) =>
        {
            var sequence = args[0];
            return sequence.IsEmpty()
                ? SequenceFactory.CreateEmpty()
                : BuiltInFunctionsString<TNode>.FnString(dynamicContext,
                    executionParameters,
                    staticContext,
                    FnNodeName(dynamicContext, executionParameters, staticContext, sequence));
        };

    public static readonly FunctionSignature<ISequence, TNode> FnNamespaceUri =
        (dynamicContext, executionParameters, staticContext, args) =>
        {
            return default;
        };
    
    public static readonly FunctionSignature<ISequence, TNode> FnInnermost =
        (dynamicContext, executionParameters, staticContext, args) =>
        {
            return default;
        };
    
    public static readonly FunctionSignature<ISequence, TNode> FnOutermost =
        (dynamicContext, executionParameters, staticContext, args) =>
        {
            return default;
        };
    
    public static readonly FunctionSignature<ISequence, TNode> FnHasChildren =
        (dynamicContext, executionParameters, staticContext, args) =>
        {
            return default;
        };
    
    public static readonly FunctionSignature<ISequence, TNode> FnPath =
        (dynamicContext, executionParameters, staticContext, args) =>
        {
            return default;
        };
    
    public static readonly FunctionSignature<ISequence, TNode> FnLocalName =
        (dynamicContext, executionParameters, staticContext, args) =>
        {
            return default;
        };
    
    public static readonly FunctionSignature<ISequence, TNode> FnRoot =
        (dynamicContext, executionParameters, staticContext, args) =>
        {
            return default;
        };
    
    public static readonly FunctionSignature<ISequence, TNode> FnData =
        (dynamicContext, executionParameters, staticContext, args) =>
        {
            return default;
        };
    
    public static readonly BuiltinDeclarationType<TNode>[] Declarations =
    {
        new(new[]
            {
                new ParameterType(ValueType.Node, SequenceMultiplicity.ZeroOrOne)
            },
            FnName,
            "name",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne)
        ),

        new(Array.Empty<ParameterType>(),
            BuiltInFunctions<TNode>.ContextItemAsFirstArgument(FnName),
            "name",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne)
        ),
        
        new(new[]
            {
                new ParameterType(ValueType.Node, SequenceMultiplicity.ExactlyOne)
            },
            FnNamespaceUri,
            "namespace-uri",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsAnyUri, SequenceMultiplicity.ExactlyOne)
        ),
        
        new(Array.Empty<ParameterType>(),
            BuiltInFunctions<TNode>.ContextItemAsFirstArgument(FnNamespaceUri),
            "namespace-uri",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsAnyUri, SequenceMultiplicity.ExactlyOne)
        ),
        
        new(new[]
            {
                new ParameterType(ValueType.Node, SequenceMultiplicity.ZeroOrMore)
            },
            FnInnermost,
            "innermost",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.Node, SequenceMultiplicity.ZeroOrMore)
        ),
        
        new(new[]
            {
                new ParameterType(ValueType.Node, SequenceMultiplicity.ZeroOrMore)
            },
            FnOutermost,
            "outermost",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.Node, SequenceMultiplicity.ZeroOrMore)
        ),
        
        new(new[]
            {
                new ParameterType(ValueType.Node, SequenceMultiplicity.ZeroOrOne)
            },
            FnHasChildren,
            "has-children",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsBoolean, SequenceMultiplicity.ExactlyOne)
        ),
        
        new(Array.Empty<ParameterType>(),
            BuiltInFunctions<TNode>.ContextItemAsFirstArgument(FnHasChildren),
            "has-children",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsBoolean, SequenceMultiplicity.ExactlyOne)
        ),
        
        new(new[]
            {
                new ParameterType(ValueType.Node, SequenceMultiplicity.ZeroOrOne)
            },
            FnPath,
            "path",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne)
        ),
        
        new(Array.Empty<ParameterType>(),
            BuiltInFunctions<TNode>.ContextItemAsFirstArgument(FnPath),
            "path",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ZeroOrOne)
        ),


        new(new[]
            {
                new ParameterType(ValueType.Node, SequenceMultiplicity.ZeroOrOne)
            },
            FnNodeName,
            "node-name",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsQName, SequenceMultiplicity.ZeroOrOne)
        ),

        new(Array.Empty<ParameterType>(),
            BuiltInFunctions<TNode>.ContextItemAsFirstArgument(FnNodeName),
            "node-name",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsQName, SequenceMultiplicity.ZeroOrOne)
        ),
        
        new(new[]
            {
                new ParameterType(ValueType.Node, SequenceMultiplicity.ZeroOrOne)
            },
            FnLocalName,
            "local-name",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)
        ),

        new(Array.Empty<ParameterType>(),
            BuiltInFunctions<TNode>.ContextItemAsFirstArgument(FnLocalName),
            "local-name",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsString, SequenceMultiplicity.ExactlyOne)
        ),
        
        new(new[]
            {
                new ParameterType(ValueType.Node, SequenceMultiplicity.ZeroOrOne)
            },
            FnRoot,
            "root",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.Node, SequenceMultiplicity.ZeroOrOne)
        ),

        new(Array.Empty<ParameterType>(),
            BuiltInFunctions<TNode>.ContextItemAsFirstArgument(FnRoot),
            "root",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.Node, SequenceMultiplicity.ZeroOrOne)
        ),
        
        new(new[]
            {
                new ParameterType(ValueType.Item, SequenceMultiplicity.ZeroOrOne)
            },
            FnData,
            "data",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsAnyAtomicType, SequenceMultiplicity.ZeroOrMore)
        ),

        new(Array.Empty<ParameterType>(),
            BuiltInFunctions<TNode>.ContextItemAsFirstArgument(FnData),
            "data",
            BuiltInUri.FunctionsNamespaceUri.GetBuiltinNamespaceUri(),
            new SequenceType(ValueType.XsAnyAtomicType, SequenceMultiplicity.ZeroOrMore)
        ),
    };
}