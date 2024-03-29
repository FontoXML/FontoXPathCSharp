using System;
using System.Xml.Linq;
using XPathTest.Caches;

namespace XPathTest;

public class XObjectUtils : INodeUtils<XObject>
{
    public string NodeToString(XObject node)
    {
        return node.ToString();
    }

    public XObject StringToXmlDocument(string xml)
    {
        return XDocument.Parse(xml);
    }

    public XObject? LoadFileToXmlNode(string filename)
    {
        return XDocumentsByPathCache.Instance.GetResource(TestingUtils.PreprocessFilename(filename));
    }

    public void LoadModule(XObject testCase, string baseUrl)
    {
        Console.WriteLine("Loading Module is not implemented.");
    }

    public XObject CreateDocument()
    {
        return new XDocument();
    }

    public XObject CreateDocumentFragment(XObject document)
    {
        return new XDocument(document);
    }
}