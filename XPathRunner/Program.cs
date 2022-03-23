using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.Parsing;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Value;

const string query = "test(self::p)";
const string xml = "<p>Test</p>";

Console.WriteLine($"Running: `{query}`\n");

var parser = XPathParser.FunctionCall();
var result = parser(query, 0).UnwrapOr((expected, fatal) =>
{
    Console.WriteLine("Parsing error ({0}): {1}", fatal, string.Join(", ", expected));
    Environment.Exit(1);
    return new Ast("ERROR");
});
Console.WriteLine("Parsed query: ");
Console.WriteLine(result);

var xmlDocument = new XmlDocument();
xmlDocument.LoadXml(xml);
var document = xmlDocument.FirstChild!;

Console.WriteLine("\nResult:");
var expr = CompileAstToExpression.CompileAst(result);
var staticContext = new StaticContext();

staticContext.RegisterFunctionDefinition(new FunctionProperties(0, (context, parameters, staticContext, args) =>
{
    Console.WriteLine("Called test function");
    return new EmptySequence();
}, "test", ""));

expr.PerformStaticEvaluation(staticContext);
Console.WriteLine(expr.Evaluate(new DynamicContext(new NodeValue(document), 0), new ExecutionParameters(document)));