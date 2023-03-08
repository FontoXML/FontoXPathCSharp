using System.Diagnostics;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.EvaluationUtils;
using FontoXPathCSharp.Expressions.DataTypes;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Expressions.Functions;

public static class BuiltInFunctionsNode<TNode> where TNode : notnull
{
    public static readonly FunctionSignature<ISequence, TNode> FnNodeName = (_, executionParameters, _, args) =>
    {
        return ISequence.ZipSingleton(args, pointerValueList =>
        {
            if (pointerValueList.Count == 0) return SequenceFactory.CreateEmpty();

            var pointerValue = pointerValueList.First();

            var domFacade = executionParameters.DomFacade;
            var pointer = pointerValue!.GetAs<NodeValue<TNode>>().Value;
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

    public static readonly FunctionSignature<ISequence, TNode> FnNamespaceUri = (_, executionParameters, _, args) =>
    {
        var sequence = args[0];
        return sequence.Map((node, _, _) =>
            AtomicValue.Create(
                executionParameters.DomFacade.GetNamespaceUri(node.GetAs<NodeValue<TNode>>().Value),
                ValueType.XsAnyUri
            )
        );
    };

    public static readonly FunctionSignature<ISequence, TNode> FnInnermost =
        (_, executionParameters, _, args) =>
        {
            var nodeSequence = args[0];
            return nodeSequence.MapAll(allNodeValues =>
            {
                if (allNodeValues.Length == 0) return SequenceFactory.CreateEmpty();

                var resultNodes = DocumentOrderUtils<TNode>.SortNodeValues(
                    executionParameters.DomFacade,
                    allNodeValues.Cast<NodeValue<TNode>>().ToList()
                ).ReduceRight(new List<NodeValue<TNode>>(), (followingNodes, node, i, allNodes) =>
                    {
                        if (i == allNodes.Count - 1)
                        {
                            followingNodes.Add(node);
                            return followingNodes;
                        }

                        // Because the nodes are sorted, the following node is either a 'following node', or a descendant of this node
                        if (Contains(executionParameters.DomFacade, node.Value, followingNodes[0].Value))
                            // The previous node is an ancestor
                            return followingNodes;

                        followingNodes.Insert(0, node);
                        return followingNodes;
                    }
                ).ToArray();

                return SequenceFactory.CreateFromArray(resultNodes.Cast<AbstractValue>().ToArray());
            });
        };

    public static readonly FunctionSignature<ISequence, TNode> FnOutermost = (_, executionParameters, _, args) =>
    {
        var nodeSequence = args[0];
        return nodeSequence.MapAll(allNodeValues =>
        {
            if (allNodeValues.Length == 0) return SequenceFactory.CreateEmpty();

            var resultNodes = DocumentOrderUtils<TNode>.SortNodeValues(
                    executionParameters.DomFacade,
                    allNodeValues.Cast<NodeValue<TNode>>().ToList())
                .Reduce(new List<NodeValue<TNode>>(), (previousNodes, node, i) =>
                {
                    if (i == 0)
                    {
                        previousNodes.Add(node);
                        return previousNodes;
                    }

                    // Because the nodes are sorted, the previous node is either a 'previous node', or an ancestor of this node
                    if (
                            Contains(
                                executionParameters.DomFacade,
                                previousNodes[previousNodes.Count - 1].Value,
                                node.Value
                            )
                        )
                        // The previous node is an ancestor
                        return previousNodes;

                    previousNodes.Add(node);
                    return previousNodes;
                })
                .Cast<AbstractValue>()
                .ToArray();

            return SequenceFactory.CreateFromArray(resultNodes);
        }, IterationHint.SkipDescendants);
    };

    public static readonly FunctionSignature<ISequence, TNode> FnHasChildren = (_, executionParameters, _, args) =>
    {
        var nodeSequence = args[0];
        return ISequence.ZipSingleton(new[] { nodeSequence }, sequenceValue =>
        {
            var pointerValue = sequenceValue.FirstOrDefault();
            var pointer = pointerValue != null ? pointerValue.GetAs<NodeValue<TNode>>().Value : default;

            if (pointer != null && executionParameters.DomFacade.GetFirstChild(pointer) != null)
                return SequenceFactory.SingletonTrueSequence;
            return SequenceFactory.SingletonFalseSequence;
        });
    };

    public static readonly FunctionSignature<ISequence, TNode> FnPath = (_, executionParameters, _, args) =>
    {
        var nodeSequence = args[0];

        return ISequence.ZipSingleton(new[] { nodeSequence }, sequenceValue =>
        {
            var pointerValue = sequenceValue[0];

            if (pointerValue == null) return SequenceFactory.CreateEmpty();

            var pointer = pointerValue.GetAs<NodeValue<TNode>>().Value;
            var domFacade = executionParameters.DomFacade;

            var result = "";

            int GetChildIndex(TNode child)
            {
                var i = 0;
                var otherChild = child;
                while (otherChild != null)
                {
                    if (AreSameNode(child, otherChild, domFacade)) i++;

                    otherChild = domFacade.GetPreviousSibling(otherChild);
                }

                return i;
            }

            TNode? ancestor;
            for (
                ancestor = pointer;
                executionParameters.DomFacade.GetParentNode(ancestor!) != null;
                ancestor = executionParameters.DomFacade.GetParentNode(ancestor)
            )

            {
                Debug.Assert(ancestor != null, nameof(ancestor) + " != null");
                switch (domFacade.GetNodeType(ancestor))
                {
                    case NodeType.Element:
                    {
                        result = $"/Q{{{domFacade.GetNamespaceUri(ancestor)}}}" +
                                 $"{domFacade.GetLocalName(ancestor)}[{GetChildIndex(ancestor)}]" +
                                 $"{result}";
                        break;
                    }
                    case NodeType.Attribute:
                    {
                        var nsu = domFacade.GetNamespaceUri(ancestor);
                        var attributeNameSpace = nsu != ""
                            ? $"Q{{{domFacade.GetNamespaceUri(ancestor)}}}"
                            : "";
                        result = $"/@{attributeNameSpace}{domFacade.GetLocalName(ancestor)}{result}";
                        break;
                    }
                    case NodeType.Text:
                    {
                        result = $"/text()[{GetChildIndex(ancestor)}]{result}";
                        break;
                    }
                    case NodeType.ProcessingInstruction:
                    {
                        result = $"/processing-instruction({domFacade.GetTarget(ancestor)})" +
                                 $"[{GetChildIndex(ancestor)}]{result}";
                        break;
                    }
                    case NodeType.Comment:
                    {
                        result = $"/comment()[{GetChildIndex(ancestor)}]{result}";
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (domFacade.GetNodeType(ancestor!) == NodeType.Document)
                return SequenceFactory.CreateFromValue(AtomicValue.Create(!string.IsNullOrEmpty(result) ? result : "/",
                    ValueType.XsString));
            result = "Q{http://www.w3.org/2005/xpath-functions}root()" + result;
            return SequenceFactory.CreateFromValue(AtomicValue.Create(result, ValueType.XsString));
        });
    };

    public static readonly FunctionSignature<ISequence, TNode> FnLocalName = (_, executionParameters, _, args) =>
    {
        var sequence = args[0];
        var domFacade = executionParameters.DomFacade;

        if (sequence.IsEmpty()) return SequenceFactory.CreateFromValue(AtomicValue.Create("", ValueType.XsString));

        return sequence.Map((nodeValue, _, _) =>
        {
            var node = nodeValue.GetAs<NodeValue<TNode>>().Value;
            return AtomicValue.Create(domFacade.GetNodeType(node) == NodeType.ProcessingInstruction
                ? domFacade.GetTarget(node)
                : domFacade.GetLocalName(node), ValueType.XsString);
        });
    };

    public static readonly FunctionSignature<ISequence, TNode> FnRoot =
        (_, executionParameters, _, args) =>
        {
            var nodeSequence = args[0];
            return nodeSequence.Map((node, _, _) =>
            {
                if (!node.GetValueType().IsSubtypeOf(ValueType.Node))
                    throw new XPathException(
                        "XPTY0004",
                        "Argument passed to fn:root() should be of the type node()"
                    );

                var ancestor = default(TNode);
                var parent = node.GetAs<NodeValue<TNode>>().Value;
                while (parent != null)
                {
                    ancestor = parent;
                    parent = executionParameters.DomFacade.GetParentNode(ancestor);
                }

                return new NodeValue<TNode>(ancestor!, executionParameters.DomFacade);
            });
        };

    public static readonly FunctionSignature<ISequence, TNode> FnData = (_, executionParameters, _, args) =>
    {
        var sequence = args[0];
        return Atomize.AtomizeSequence(sequence, executionParameters);
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
        )
    };

    private static bool AreSameNode(TNode nodeA, TNode nodeB, DomFacade<TNode> domFacade)
    {
        if (domFacade.GetNodeType(nodeA) != domFacade.GetNodeType(nodeB)) return false;
        if (domFacade.GetNodeType(nodeB) == NodeType.Element)
            return domFacade.GetLocalName(nodeB) == domFacade.GetLocalName(nodeA) &&
                   domFacade.GetNamespaceUri(nodeB) == domFacade.GetNamespaceUri(nodeA);
        if (domFacade.GetNodeType(nodeB) == NodeType.ProcessingInstruction)
            return domFacade.GetTarget(nodeB) == domFacade.GetTarget(nodeA);

        return true;
    }

    private static bool Contains(IDomFacade<TNode> domFacade, TNode ancestor, TNode? descendant)
    {
        if (domFacade.GetNodeType(ancestor) == NodeType.Attribute) return ancestor.Equals(descendant);
        while (descendant != null)
        {
            if (ancestor.Equals(descendant)) return true;
            if (domFacade.GetNodeType(descendant) == NodeType.Document) return false;
            descendant = domFacade.GetParentNode(descendant);
        }

        return false;
    }
}