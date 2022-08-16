using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using FontoXPathCSharp;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;
using XPathTest.Caches;

namespace XPathTest;

public interface NodeUtils<TNode>
{
    protected static readonly Options<TNode> Options = new(_ => "http://www.w3.org/2010/09/qt-fots-catalog");

    public string NodeToString(TNode node);

    public TNode StringToXmlDocument(string xml);

    public TNode? LoadFileToXmlNode(string filename);

    public void LoadModule(TNode testCase, string baseUrl);
    TNode CreateDocument();
}