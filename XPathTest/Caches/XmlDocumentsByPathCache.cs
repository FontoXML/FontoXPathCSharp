using System.IO;
using System.Linq;
using System.Xml;
using FontoXPathCSharp.DomFacade;

namespace XPathTest.Caches;

public class XmlDocumentsByPathCache : ResourceCache<string, XmlNode>
{
    private static readonly XmlNodeUtils Utils = new();
    private static readonly XmlNodeDomFacade DomFacade = new();

    private static readonly XmlDocument GlobalDocument = (XmlDocument)Utils.StringToXmlDocument("<xml/>");
    public static XmlDocumentsByPathCache Instance { get; } = new();


    protected override XmlNode? Load(string filename)
    {
        string content;
        try
        {
            // Add support for testsets, for example to access ForExpr-013.out, which needs to enter the prod folder.
            content = TestFileSystem.ReadFile($"qt3tests/{filename}").Replace("\r\n", "\n");
        }
        catch (DirectoryNotFoundException)
        {
            throw new DirectoryNotFoundException($"Could not find a part of the path ending in: 'qt3tests/{filename}'");
        }


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
            var documentFragment = Utils.CreateDocumentFragment(GlobalDocument);
            parsedContents.ForEach(node => documentFragment?.AppendChild(node));
            return documentFragment;
        }

        return Utils.StringToXmlDocument(content);
    }
}