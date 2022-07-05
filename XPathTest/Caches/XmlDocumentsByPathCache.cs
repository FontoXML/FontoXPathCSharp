using System.Xml;

namespace XPathTest.Caches;

public class XmlDocumentsByPathCache : ResourceCache<string, XmlNode>
{
    public static XmlDocumentsByPathCache Instance { get; } = new();

    protected override XmlNode Load(string key)
    {
        var xmlFile = new XmlDocument();
        xmlFile.Load(key);
        return xmlFile;
    }
}