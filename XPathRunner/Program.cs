using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.Value;

const string QUERY = "self::p";
const string DOCUMENT = "<p>Test</p>";

Console.WriteLine($"Running: `{QUERY}`\n");

var parser = XPathParser.PathExpr();
var result = parser(QUERY, 0).Unwrap();
Console.WriteLine("Parsed query: ");
Console.WriteLine(result);

var xmlDocument = new XmlDocument();
xmlDocument.LoadXml(DOCUMENT);
var document = xmlDocument.FirstChild!;

Console.WriteLine("\nResult:");
var expr = CompileAstToExpression.CompileAst(result);
Console.WriteLine(expr.Evaluate(document, new NodeValue(document)));