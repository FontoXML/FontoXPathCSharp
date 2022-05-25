using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.Functions;
using FontoXPathCSharp.Parsing;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;

const string query = "string-length(\"   t    e s    t \")";
const string xml = "<p>Test</p>";

Console.WriteLine($"Running: `{query}`\n");

var result = XPathParser.Parse(query, new ParseOptions(false, true)).UnwrapOr((expected, fatal) =>
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
        "http://www.w3.org/2005/xpath-functions", (_, _) => null);
var staticContext = new StaticContext(executionContext);

// normalize_string()
// hours_from_duration()

foreach (var function in BuiltInFunctions.Declarations)
    staticContext.RegisterFunctionDefinition(new FunctionProperties(function.ArgumentTypes,
        function.ArgumentTypes.Length, function.CallFunction, function.LocalName,
        function.NamespaceUri, function.ReturnType));

expr.PerformStaticEvaluation(staticContext);
var resultSequence = expr.Evaluate(new DynamicContext(new NodeValue(document), 0), new ExecutionParameters(document));
resultSequence.GetAllValues().ToList().ForEach(Console.WriteLine);
