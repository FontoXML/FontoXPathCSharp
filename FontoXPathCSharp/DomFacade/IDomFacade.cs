namespace FontoXPathCSharp.DomFacade;

public interface IDomFacade<TNode>
{
    IEnumerable<TNode> GetAllAttributes(TNode node, string? bucket = null);

    string? GetAttribute(TNode node, string? attributeName);

    IEnumerable<TNode> GetChildNodes(TNode node, string? bucket = null);

    string GetData(TNode node);

    TNode? GetFirstChild(TNode node, string? bucket = null);

    TNode? GetLastChild(TNode node, string? bucket = null);

    TNode? GetNextSibling(TNode node, string? bucket = null);

    TNode? GetParentNode(TNode node, string? bucket = null);

    TNode? GetPreviousSibling(TNode node, string? bucket = null);

    string GetLocalName(TNode node);

    string GetNamespaceUri(TNode node);

    string? GetPrefix(TNode nodeValue);

    bool IsElement(TNode node);

    bool IsAttribute(TNode node);

    bool IsText(TNode node);
    bool IsProcessingInstruction(TNode node);
    bool IsComment(TNode node);
    bool IsDocument(TNode node);
    bool IsDocumentFragment(TNode node);
    bool IsCharacterData(TNode node);
    NodeType GetNodeType(TNode node);
    string? LookupNamespaceUri(TNode node, string? prefix);
}

public enum NodeType
{
    Element,
    Attribute,
    Text,
    CData,
    ProcessingInstruction,
    Comment,
    Document,
    DocumentType,
    DocumentFragment
}