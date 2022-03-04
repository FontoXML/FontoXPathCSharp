using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Value;

var parser = XPathParser.PathExpr();

var result = parser("self::p", 0).Unwrap();
Console.WriteLine(result);

var xmlDocument = new XmlDocument();
xmlDocument.LoadXml("<p>Test</p>");
var document = xmlDocument.FirstChild!;

var pathExpr = new SelfAxis(new NameTest(new QName("p", "", "*")));

Console.WriteLine(pathExpr.Evaluate(document, new NodeValue(document)).ToString());