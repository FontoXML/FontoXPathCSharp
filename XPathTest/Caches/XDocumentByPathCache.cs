using System.Linq;
using System.Xml.Linq;

namespace XPathTest.Caches;

public class XDocumentByPathCache : ResourceCache<string, XObject>
{
    private static readonly XDocument GlobalDocument = LoadXmlFromString("<xml/>");
    public static XmlDocumentsByPathCache Instance { get; } = new();


    protected override XObject? Load(string filename)
    {
        var content = TestFileSystem.ReadFile($"QT3TS/{filename}").Replace("\r\n", "\n");

        if (filename.EndsWith(".out"))
        {
            content = $"<xml>{content}</xml>";

            if (content.EndsWith('\n')) content = content[..^2];

            var parsedContents = (LoadXmlFromString(content).FirstNode as XElement)?
                .Nodes()
                .Cast<XObject>()
                .ToList();

            var documentFragment = new XDocument(GlobalDocument);
            parsedContents?.ForEach(node => documentFragment.Add(node));
            return documentFragment;
        }

        return LoadXmlFromString(content);
    }

    private static XDocument LoadXmlFromString(string contents)
    {
        return XDocument.Parse(contents);
    }
}