namespace FontoXPathCSharp.DomFacade;

public class DomFacade<TNode> : IDomFacade<TNode>
{
    private readonly IDomFacade<TNode> _domFacade;

    public DomFacade(IDomFacade<TNode> domFacade)
    {
        _domFacade = domFacade;
        OrderOfDetachedNodes = new List<TNode>();
    }

    public List<TNode> OrderOfDetachedNodes { get; }

    public IEnumerable<TNode> GetAllAttributes(TNode node, string? bucket = null)
    {
        return _domFacade.GetAllAttributes(node, bucket);
    }

    public string? GetAttribute(TNode node, string? attributeName)
    {
        return _domFacade.GetAttribute(node, attributeName);
    }

    public IEnumerable<TNode> GetChildNodes(TNode node, string? bucket = null)
    {
        return _domFacade.GetChildNodes(node, bucket);
    }

    public string GetData(TNode node)
    {
        return _domFacade.GetData(node);
    }

    public TNode? GetFirstChild(TNode node, string? bucket = null)
    {
        return _domFacade.GetFirstChild(node, bucket);
    }

    public TNode? GetLastChild(TNode node, string? bucket = null)
    {
        return _domFacade.GetLastChild(node, bucket);
    }

    public TNode? GetNextSibling(TNode node, string? bucket = null)
    {
        return _domFacade.GetNextSibling(node, bucket);
    }

    public TNode? GetParentNode(TNode node, string? bucket = null)
    {
        return _domFacade.GetParentNode(node, bucket);
    }

    public TNode? GetPreviousSibling(TNode node, string? bucket = null)
    {
        return _domFacade.GetPreviousSibling(node, bucket);
    }

    public TNode? GetDocument(TNode node)
    {
        return _domFacade.GetDocument(node);
    }

    public string GetLocalName(TNode node)
    {
        return _domFacade.GetLocalName(node);
    }

    public string GetNamespaceUri(TNode node)
    {
        return _domFacade.GetNamespaceUri(node);
    }

    public string? GetPrefix(TNode nodeValue)
    {
        return _domFacade.GetPrefix(nodeValue);
    }

    public bool IsElement(TNode node)
    {
        return _domFacade.IsElement(node);
    }

    public bool IsAttribute(TNode node)
    {
        return _domFacade.IsAttribute(node);
    }

    public bool IsText(TNode node)
    {
        return _domFacade.IsText(node);
    }

    public bool IsProcessingInstruction(TNode node)
    {
        return _domFacade.IsProcessingInstruction(node);
    }

    public bool IsComment(TNode node)
    {
        return _domFacade.IsComment(node);
    }

    public bool IsDocument(TNode node)
    {
        return _domFacade.IsDocument(node);
    }

    public bool IsDocumentFragment(TNode node)
    {
        return _domFacade.IsDocumentFragment(node);
    }

    public bool IsCharacterData(TNode node)
    {
        return _domFacade.IsCharacterData(node);
    }

    public NodeType GetNodeType(TNode node)
    {
        return _domFacade.GetNodeType(node);
    }

    public TNode? GetDocumentElement(TNode node)
    {
        return _domFacade.GetDocumentElement(node);
    }

    public string GetNodeName(TNode node)
    {
        return _domFacade.GetNodeName(node);
    }

    public string? GetTarget(TNode node)
    {
        return _domFacade.GetTarget(node);
    }
}