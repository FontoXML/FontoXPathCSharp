using System;
using System.IO;
using System.Xml;
using XPathTest.Caches;

namespace XPathTest;

public class XmlNodeUtils : INodeUtils<XmlNode>
{
    public string NodeToString(XmlNode node)
    {
        using var sw = new StringWriter();
        using var xw = new XmlTextWriter(sw);
        xw.Formatting = Formatting.Indented;
        xw.Indentation = 2;

        node.WriteTo(xw);
        return sw.ToString();
    }

    public XmlNode StringToXmlDocument(string xml)
    {
        var doc = new XmlDocument();
        doc.LoadXml(xml);
        return doc;
    }

    public XmlNode? LoadFileToXmlNode(string filename)
    {
        return XmlDocumentsByPathCache.Instance.GetResource(TestingUtils.PreprocessFilename(filename));
    }

    public void LoadModule(XmlNode testCase, string baseUrl)
    {
        Console.WriteLine("Loading Module is not implemented.");
    }

    public XmlNode CreateDocument()
    {
        return new XmlDocument();
    }

    public XmlNode? CreateDocumentFragment(XmlNode document)
    {
        return (document as XmlDocument)?.CreateDocumentFragment();
    }
}