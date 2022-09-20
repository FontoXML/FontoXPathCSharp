using System.Linq;
using System.Xml.Linq;
using FontoXPathCSharp.DomFacade;

namespace XPathTest.Caches;

public class XDocumentsByPathCache : ResourceCache<string, XObject>
{
    private static readonly XObjectUtils Utils = new();
    private static readonly XObjectDomFacade DomFacade = new();

    private static readonly XDocument GlobalDocument = (XDocument)Utils.StringToXmlDocument("<xml/>");
    public static XDocumentsByPathCache Instance { get; } = new();

    protected override XObject? Load(string filename)
    {
        var content = TestFileSystem.ReadFile($"qt3tests/{filename}").Replace("\r\n", "\n");

        if (filename.EndsWith(".out"))
        {
            content = $"<xml>{content}</xml>";

            if (content.EndsWith('\n')) content = content[..^2];

            var parsedContents = DomFacade.GetChildNodes(
                    DomFacade.GetFirstChild(
                        Utils.StringToXmlDocument(content)
                    )!
                )
                .ToList();
            var documentFragment = Utils.CreateDocumentFragment(GlobalDocument) as XDocument;
            parsedContents.ForEach(node => documentFragment.Add(node));
            return documentFragment;
        }

        return Utils.StringToXmlDocument(content);
    }
}
