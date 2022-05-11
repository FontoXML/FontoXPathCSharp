using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.Functions;
using FontoXPathCSharp.Parsing;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;
using FontoXPathCSharp.Value.Types;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

const string query = "string-length(\"test\")";
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
    new ExecutionSpecificStaticContext(s => s, new Dictionary<string, IExternalValue>(), "", (name, i) => null);
var staticContext = new StaticContext(executionContext);

// node-name()
// string-length()
// count()
// zero_or_one()
// x("string")
// normalize_string()
// hours_from_duration()
staticContext.RegisterFunctionDefinition(new FunctionProperties(
    new[]
    {
        new ParameterType(ValueType.XsInteger, SequenceMultiplicity.ExactlyOne),
        new ParameterType(ValueType.XsInteger, SequenceMultiplicity.ExactlyOne)
    }, 2,
    (context, parameters, staticContext, args) =>
    {
        Console.WriteLine("Called add function");
        return new SingletonSequence(new IntValue(args[0].First()!.GetAs<IntValue>(ValueType.XsInteger)!.Value +
                                                  args[1].First()!.GetAs<IntValue>(ValueType.XsInteger)!.Value));
    }, "add", "", new SequenceType(ValueType.XsInteger, SequenceMultiplicity.ExactlyOne)));

staticContext.RegisterFunctionDefinition(new FunctionProperties(
    new[]
    {
        new ParameterType(ValueType.XsInteger, SequenceMultiplicity.ExactlyOne),
        new ParameterType(ValueType.XsInteger, SequenceMultiplicity.ExactlyOne),
        new ParameterType(ValueType.XsInteger, SequenceMultiplicity.ExactlyOne)
    }, 3,
    (context, parameters, staticContext, args) =>
    {
        Console.WriteLine("Called add function");
        return new SingletonSequence(new IntValue(args[0].First()!.GetAs<IntValue>(ValueType.XsInteger)!.Value +
                                                  args[1].First()!.GetAs<IntValue>(ValueType.XsInteger)!.Value +
                                                  args[2].First()!.GetAs<IntValue>(ValueType.XsInteger)!.Value));
    }, "add", "", new SequenceType(ValueType.XsInteger, SequenceMultiplicity.ExactlyOne)));

foreach (var function in BuiltInFunctions.Declarations)
    staticContext.RegisterFunctionDefinition(new FunctionProperties(function.ArgumentTypes,
        function.ArgumentTypes.Length, function.CallFunction, function.LocalName,
        function.NamespaceUri, function.ReturnType));

expr.PerformStaticEvaluation(staticContext);
Console.WriteLine(expr.Evaluate(new DynamicContext(new NodeValue(document), 0), new ExecutionParameters(document)));