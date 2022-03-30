using FontoXPathCSharp.Types.Node;

namespace FontoXPathCSharp.DomFacade;

public interface IDomFacade
{
    public Attr[] GetAllAttributes(Element node, string? bucket);

    string? GetAttribute(Element node, string? attributeName);

    Node[] GetChildNodes(Node node, string? bucket);

    string GetData(Attr node);

    string GetData(CharacterData node);

    Node? GetFirstChild(Node node, string? bucket);

    Node? GetLastChild(Node node, string? bucket);

    Node? GetNextSibling(Node node, string? bucket);

    Node? GetParentNode(Node node, string? bucket);

    Node? GetPreviousSibling(Node node, string? bucket);
}