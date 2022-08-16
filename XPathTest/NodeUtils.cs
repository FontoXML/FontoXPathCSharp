using FontoXPathCSharp.Types;

namespace XPathTest;

public interface NodeUtils<TNode>
{
    public string NodeToString(TNode node);

    public TNode StringToXmlDocument(string xml);

    public TNode? LoadFileToXmlNode(string filename);

    public void LoadModule(TNode testCase, string baseUrl);
    TNode CreateDocument();

    TNode? CreateDocumentFragment(TNode document);
}