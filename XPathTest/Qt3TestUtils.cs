using System;
using System.Linq;
using System.Xml;
using XPathTest.Caches;

namespace XPathTest;

public static class Qt3TestUtils
{
    private static readonly XmlDocument GlobalDocument = LoadXmlFromString("<xml/>");

    
    private static XmlDocument LoadXmlFile(string fileName)
    {
        var document = new XmlDocument();
        document.Load(fileName);
        return document;
    }
    
    private static XmlDocument LoadXmlFromString(string contents)
    {
        var document = new XmlDocument();
        document.LoadXml(contents);
        return document;
    }

    private static string PreprocessFilename(string filename)
    {
        while (filename.Contains(".."))
        {
            var parts = filename.Split('/');

            filename = string.Join('/', parts
                .Take(Array.IndexOf(parts, ".."))
                .Concat(parts.Skip(Array.IndexOf(parts, "..") + 1)));
        }

        return filename;
    }
    
    public static XmlNode? LoadFileToXmlNode(string filename)
    {
        filename = PreprocessFilename(filename);

        var cached = XmlDocumentsByPathCache.Instance.GetResource(filename);
        if (cached != null) {
            return cached;
        }
        
        var content = TestFileSystem.ReadFile("QT3TS/${fileName}").Replace("\r\n", "\n");

        if (!filename.EndsWith(".out")) return null;
        if (content.EndsWith('\n'))
        {
            content = content[..^2];
        }
        content = $"<xml>{content}</xml>";

        var parsedContents = LoadXmlFromString(content)
            .FirstChild?
            .ChildNodes
            .Cast<XmlNode>()
            .ToList();
        var documentFragment = GlobalDocument.CreateDocumentFragment();
        parsedContents?.ForEach(node => documentFragment.AppendChild(node));
            
        XmlDocumentsByPathCache.Instance.InsertResource(filename, documentFragment);
            
        return documentFragment;
    }

    public static string? LoadFileToString(string filename)
    {
        return DocumentsByPathCache.Instance.GetResource($"QT3TS/{PreprocessFilename(filename)}");
    }
}