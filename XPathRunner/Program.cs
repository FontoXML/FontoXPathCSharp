using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Value;

static void PrintAst(IEnumerable<object> ast, int indent = 0)
{
    foreach (var a in ast)
        if (a is object[] objects)
            PrintAst(objects, indent + 1);
        else
            Console.WriteLine(new string('\t', indent) + a);
}

var parser = XPathParser.PathExpr();

var result = parser("self::p", 0).Unwrap();
PrintAst(result);

var xmlDocument = new XmlDocument();
xmlDocument.LoadXml("<p>Test</p>");
var document = xmlDocument.FirstChild!;

var pathExpr = new SelfAxis(new NameTest(new QName("p", "", "*")));

Console.WriteLine(pathExpr.Evaluate(document, new NodeValue(document)).ToString());