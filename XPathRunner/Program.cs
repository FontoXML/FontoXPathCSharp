using System.Diagnostics;
using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.Functions;
using FontoXPathCSharp.Parsing;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;

const string query = "catalog";
const string xml = "<p>Test</p>";

Console.WriteLine($"Running: `{query}`\n");

var result = XPathParser.Parse(query, new ParseOptions(false, false)).UnwrapOr((expected, fatal) =>
{
    Console.WriteLine("Parsing error ({0}): expected {1}", fatal, string.Join(", ", expected.Distinct()));
    Environment.Exit(1);
    return new Ast(AstNodeName.All);
});
Console.WriteLine("Parsed query: ");
Console.WriteLine(result);

var xmlDocument = new XmlDocument();
xmlDocument.Load("../../../../XPathTest/assets/QT3TS/catalog.xml");
var document = xmlDocument.FirstChild!;

Console.WriteLine("\nResult:");
var expr = CompileAstToExpression.CompileAst(result, new CompilationOptions(true, false, true, true));
var executionContext =
    new ExecutionSpecificStaticContext(s => null, new Dictionary<string, IExternalValue>(),
        BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(), (_, _) => null);
var staticContext = new StaticContext(executionContext);

// hours_from_duration()

foreach (var function in BuiltInFunctions.Declarations)
{
    FunctionRegistry.RegisterFunction(function.NamespaceUri, function.LocalName, function.ArgumentTypes,
        function.ReturnType, function.CallFunction);

    var functionProperties =
        FunctionRegistry.GetFunctionByArity(function.NamespaceUri, function.LocalName, function.ArgumentTypes.Length);
    staticContext.RegisterFunctionDefinition(functionProperties!);
}

Console.WriteLine(executionContext);
Console.WriteLine(staticContext);

expr.PerformStaticEvaluation(staticContext);
var resultSequence = expr.Evaluate(new DynamicContext(new NodeValue(document), 0), new ExecutionParameters(document));
resultSequence.GetAllValues().ToList().ForEach(Console.WriteLine);


// var qt3tests = new XmlDocument();
// qt3tests.Load("../../../../XPathTest/assets/QT3TS/catalog.xml");
//
// var nodes = qt3tests;
// Console.WriteLine("Selector resulted in: " + Evaluate.EvaluateXPathToBoolean("catalog", qt3tests, null, new Dictionary<string, IExternalValue>(), new Options()));
