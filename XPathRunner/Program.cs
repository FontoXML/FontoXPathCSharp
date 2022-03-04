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

var expr = CompileAstToExpression.CompileTest(result.FollowPath(new[] {"stepExpr", "nameTest"}));
Console.WriteLine(expr.Evaluate(document, new NodeValue(document)));
