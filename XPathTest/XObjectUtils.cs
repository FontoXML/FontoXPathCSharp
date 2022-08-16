using System;
using System.Xml.Linq;

namespace XPathTest;

public class XObjectUtils : NodeUtils<XObject>
{
    public string NodeToString(XObject node)
    {
        throw new System.NotImplementedException();
    }

    public XObject StringToXmlDocument(string xml)
    {
        return XDocument.Parse(xml);
    }

    public XObject? LoadFileToXmlNode(string filename)
    {
        throw new System.NotImplementedException();
    }

    public void LoadModule(XObject testCase, string baseUrl)
    {
        Console.WriteLine("Loading Module is not implemented.");
    }

    public XObject CreateDocument()
    {
        return new XDocument();
    }
}