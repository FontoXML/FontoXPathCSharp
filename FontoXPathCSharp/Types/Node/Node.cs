namespace FontoXPathCSharp.Types.Node;

public abstract class Node
{
    public NodeTypes NodeType { get; }
}

public enum NodeTypes
{
    ElementNode,
    AttributeNode,
    TextNode,
    CDataSectionNode,
    ProcessingInstructionNode,
    CommentNode,
    DocumentNode,
    DocumentTypeNode,
    DocumentFragmentNode
}