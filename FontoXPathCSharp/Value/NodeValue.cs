using FontoXPathCSharp.DomFacade;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

namespace FontoXPathCSharp.Value;

public class NodeValue<TNode> : AbstractValue
{
    public NodeValue(TNode value, IDomFacade<TNode> domFacade) : base(GetNodeType(value, domFacade))
    {
        Value = value;
    }

    public TNode Value { get; }

    private static ValueType GetNodeType(TNode node, IDomFacade<TNode> domFacade)
    {
        if (domFacade.IsElement(node)) return ValueType.Element;
        if (domFacade.IsAttribute(node)) return ValueType.Attribute;
        if (domFacade.IsComment(node)) return ValueType.Comment;
        if (domFacade.IsDocument(node)) return ValueType.DocumentNode;
        if (domFacade.IsText(node)) return ValueType.Text;
        if (domFacade.IsProcessingInstruction(node)) return ValueType.ProcessingInstruction;
        throw new NotImplementedException(
            $"NodeValue.GetNodeType: \"{node?.ToString() ?? "null"}\" does not map to any existing ValueType");
    }

    public override string ToString()
    {
        return $"<Value>[type: {Type}, value: {Value}]";
    }
}