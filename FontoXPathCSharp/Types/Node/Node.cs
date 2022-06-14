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
    CdataSectionNode,
    ProcessingInstructionNode,
    CommentNode,
    DocumentNode,
    DocumentTypeNode,
    DocumentFragmentNode
}