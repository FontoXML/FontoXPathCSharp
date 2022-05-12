using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.Functions;
using FontoXPathCSharp.Parsing;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;

const string query = "count(self::p)";
const string xml = "<p>Test</p>";

Console.WriteLine($"Running: `{query}`\n");

var result = XPathParser.QueryBody(query, 0).UnwrapOr((expected, fatal) =>
{
    Console.WriteLine("Parsing error ({0}): {1}", fatal, string.Join(", ", expected));
    Environment.Exit(1);
    return new Ast(AstNodeName.All);
});
Console.WriteLine("Parsed query: ");
Console.WriteLine(result);

var xmlDocument = new XmlDocument();
xmlDocument.LoadXml(xml);
var document = xmlDocument.FirstChild!;

Console.WriteLine("\nResult:");
var expr = CompileAstToExpression.CompileAst(result);
var executionContext =
    new ExecutionSpecificStaticContext(s => s, new Dictionary<string, IExternalValue>(),
        "http://www.w3.org/2005/xpath-functions", (name, i) => null);
var staticContext = new StaticContext(executionContext);

// zero_or_one()
// x("string")
// normalize_string()
// hours_from_duration()

foreach (var function in BuiltInFunctions.Declarations)
    staticContext.RegisterFunctionDefinition(new FunctionProperties(function.ArgumentTypes,
        function.ArgumentTypes.Length, function.CallFunction, function.LocalName,
        function.NamespaceUri, function.ReturnType));

expr.PerformStaticEvaluation(staticContext);
Console.WriteLine(expr.Evaluate(new DynamicContext(new NodeValue(document), 0), new ExecutionParameters(document)));
