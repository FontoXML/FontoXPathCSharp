using System.Linq;
using System.Xml;

namespace XPathTest.Caches;

public class XmlDocumentsByPathCache : ResourceCache<string, XmlNode>
{
    private static readonly XmlDocument GlobalDocument = LoadXmlFromString("<xml/>");
    public static XmlDocumentsByPathCache Instance { get; } = new();


    protected override XmlNode? Load(string filename)
    {
        var content = TestFileSystem.ReadFile($"qt3tests/{filename}").Replace("\r\n", "\n");

        if (filename.EndsWith(".out"))
        {
            content = $"<xml>{content}</xml>";

            if (content.EndsWith('\n')) content = content[..^2];

            var parsedContents = LoadXmlFromString(content)
                .FirstChild?
                .ChildNodes
                .Cast<XmlNode>()
                .ToList();
            var documentFragment = GlobalDocument.CreateDocumentFragment();
            parsedContents?.ForEach(node => documentFragment.AppendChild(node));
            return documentFragment;
        }

        ;

        return LoadXmlFromString(content);
    }

    private static XmlDocument LoadXmlFromString(string contents)
    {
        var document = new XmlDocument();
        document.LoadXml(contents);
        return document;
    }
}