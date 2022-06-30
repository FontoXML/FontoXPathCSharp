using System.Diagnostics;
using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.Functions;
using FontoXPathCSharp.Parsing;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;
using Xunit;
using Xunit.Abstractions;
using ValueType = FontoXPathCSharp.Value.Types.ValueType;

public class Runners{
    private readonly ITestOutputHelper _testOutputHelper;

    public Runners(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void BaseCase()
    {
        const string query = "/catalog/test-set";
        const string xml = "<p>Test</p>";

        Console.WriteLine($"Running: `{query}`\n");

        var result = XPathParser.Parse(query, new ParseOptions(false, false)).UnwrapOr((expected, fatal) =>
        {
            Console.WriteLine("Parsing error ({0}): expected {1}", fatal, string.Join(", ", expected.Distinct()));
            Environment.Exit(1);
            return new Ast(AstNodeName.All);
        });
        Console.WriteLine("Parsed query:\n" + result);
    }

    [Fact]
    public void Qt3TestNodes()
    {
        var qt3tests = new XmlDocument();
        qt3tests.Load("../../../../XPathTest/assets/QT3TS/catalog.xml");
        var results = Evaluate.EvaluateXPathToNodes("/catalog/test-set", qt3tests, null,
            new Dictionary<string, IExternalValue>(), new Options(namespaceResolver: s => "http://www.w3.org/2010/09/qt-fots-catalog"));
        var joinedResult = $"[ {string.Join("\n", results.Select(r => r.Attributes?["name"]?.Value))} ]";

        _testOutputHelper.WriteLine("Selector resulted in: " + joinedResult);
    }
    

    [Fact]
    public void Qt3TestsLoad()
    {
        var qt3tests = new XmlDocument();
        qt3tests.Load("../../../../XPathTest/assets/QT3TS/catalog.xml");
        
        var tests = Evaluate.EvaluateXPathToString("@version", qt3tests.DocumentElement, null,
        new Dictionary<string, IExternalValue>(), new Options());

        _testOutputHelper.WriteLine($"Last query returned: {tests}");


        // .Where(testSetNode =>
        // {
        //     var res = Evaluate.EvaluateXPathToString("@name",
        //         testSetNode,
        //         null,
        //         new Dictionary<string, IExternalValue>(),
        //         new Options());
        //     Console.WriteLine("Evaluated to: {0}", res);
        //     return true;
        // })
        // .Select(testSetNode => Evaluate.EvaluateXPathToString("@file",
        //     testSetNode,
        //     null,
        //     new Dictionary<string, IExternalValue>(),
        //     new Options()))
        // .ToList();
    }
}



// var xmlDocument = new XmlDocument();
// xmlDocument.Load("../../../../XPathTest/assets/QT3TS/catalog.xml");
// var document = xmlDocument;

// var expr = CompileAstToExpression.CompileAst(result, new CompilationOptions(true, false, true, true));
// var executionContext =
//     new ExecutionSpecificStaticContext(s => null, new Dictionary<string, IExternalValue>(),
//         BuiltInUri.FUNCTIONS_NAMESPACE_URI.GetBuiltinNamespaceUri(), (_, _) => null);
// var staticContext = new StaticContext(executionContext);

// hours_from_duration()

// foreach (var function in BuiltInFunctions.Declarations)
// {
//     FunctionRegistry.RegisterFunction(function.NamespaceUri, function.LocalName, function.ArgumentTypes,
//         function.ReturnType, function.CallFunction);
//
//     var functionProperties =
//         FunctionRegistry.GetFunctionByArity(function.NamespaceUri, function.LocalName, function.ArgumentTypes.Length);
//     staticContext.RegisterFunctionDefinition(functionProperties!);
// }

// Console.WriteLine(executionContext);
// Console.WriteLine(staticContext);

// expr.PerformStaticEvaluation(staticContext);
// var resultSequence = expr.Evaluate(new DynamicContext(new NodeValue(document), 0), new ExecutionParameters(document));
//
// Console.WriteLine("\nResult:");
// resultSequence.GetAllValues().ToList().ForEach(r => Console.WriteLine(r.GetAs<NodeValue>(ValueType.Node)?.Value.Attributes?["file"]?.Value));






    
    