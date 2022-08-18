using FontoXPathCSharp.DomFacade;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class NodeValue<TNode> : AbstractValue where TNode : notnull
{
    public NodeValue(TNode value, IDomFacade<TNode> domFacade) : base(GetNodeType(value, domFacade))
    {
        NodeType = domFacade.GetNodeType(value);
        Value = value;
    }

    public TNode Value { get; }

    public NodeType NodeType { get; }

    private static ValueType GetNodeType(TNode node, IDomFacade<TNode> domFacade)
    {
        return domFacade.GetNodeType(node) switch
        {
            NodeType.Element => ValueType.Element,
            NodeType.Attribute => ValueType.Attribute,
            NodeType.Text or NodeType.CData => ValueType.Text,
            NodeType.ProcessingInstruction => ValueType.ProcessingInstruction,
            NodeType.Comment => ValueType.Comment,
            NodeType.Document => ValueType.DocumentNode,
            _ => ValueType.Node
        };
    }

    public override string ToString()
    {
        return $"<Value>[type: {Type}, value: {Value}]";
    }
}