using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.Value;

const string query = "self::p";
const string xml = "<p>Test</p>";

Console.WriteLine($"Running: `{query}`\n");

var parser = XPathParser.PathExpr();
var result = parser(query, 0).Unwrap();
Console.WriteLine("Parsed query: ");
Console.WriteLine(result);

var xmlDocument = new XmlDocument();
xmlDocument.LoadXml(xml);
var document = xmlDocument.FirstChild!;

Console.WriteLine("\nResult:");
var expr = CompileAstToExpression.CompileAst(result);
Console.WriteLine(expr.Evaluate(document, new NodeValue(document)));