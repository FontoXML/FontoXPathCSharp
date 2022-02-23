using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.Expressions;
using Expression = System.Linq.Expressions.Expression;
using ValueType = FontoXPathCSharp.ValueType;

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

var pathExpr = new SelfAxis(new NameTest(new Name("p", "", "*")));

Console.WriteLine(pathExpr.Evaluate(document, new Value(document, ValueType.NODE)).ToString());