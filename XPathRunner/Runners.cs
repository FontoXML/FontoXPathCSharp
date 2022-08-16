using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.Parsing;
using Xunit;
using Xunit.Abstractions;

namespace XPathRunner;

public class Runners
{
    private readonly XmlDocument _qt3Tests;
    private readonly ITestOutputHelper _testOutputHelper;

    public Runners(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _qt3Tests = new XmlDocument();
        _qt3Tests.Load("../../../../XPathTest/assets/qt3tests/catalog.xml");
    }

    [Fact]
    public void BaseCase()
    {
        const string query = "/catalog/test-set";
        const string xml = "<p>Test</p>";

        _testOutputHelper.WriteLine($"Running: `{query}`\n");

        var result = XPathParser.Parse(query, new ParseOptions(false, false)).UnwrapOr((expected, fatal) =>
        {
            _testOutputHelper.WriteLine($"Parsing error ({fatal}): expected {string.Join(", ", expected.Distinct())}");
            Environment.Exit(1);
            return new Ast(AstNodeName.All);
        });
        _testOutputHelper.WriteLine($"Parsed query:\n{result}");
    }

    // [Fact]
    // public void Qt3TestNodes()
    // {
    //     var domFacade = new XmlNodeDomFacade();
    //     var results = Evaluate.EvaluateXPathToNodes<string, XmlNode>(
    //         "/catalog/test-set",
    //         new NodeValue<XmlNode>(_qt3Tests, domFacade),
    //         domFacade,
    //         new Dictionary<string, AbstractValue>(),
    //         new Options<XmlNode>(namespaceResolver: s => "http://www.w3.org/2010/09/qt-fots-catalog"));
    //     var joinedResult =
    //         $"[ {string.Join("\n", results.Select(r => $"{r.Attributes?["name"]?.Value} - {r.Attributes?["file"]?.Value}"))} ]";
    //
    //     _testOutputHelper.WriteLine("Selector resulted in: " + joinedResult);
    // }


    // [Fact]
    // public void Qt3TestsLoad()
    // {
    //     var tests = Evaluate.EvaluateXPathToString("@version", _qt3Tests.DocumentElement, null,
    //         new Dictionary<string, AbstractValue>(), new Options());
    //
    //     _testOutputHelper.WriteLine($"Last query returned: {tests}");
    //
    //     var testFileNames = Evaluate.EvaluateXPathToNodes("/catalog/test-set", _qt3Tests, null,
    //             new Dictionary<string, AbstractValue>(),
    //             new Options(namespaceResolver: s => "http://www.w3.org/2010/09/qt-fots-catalog"))
    //         .Where(testSetNode =>
    //         {
    //             var res = Evaluate.EvaluateXPathToString("@name",
    //                 testSetNode,
    //                 null,
    //                 new Dictionary<string, AbstractValue>(),
    //                 new Options());
    //             return true;
    //         })
    //         .Select(testSetNode => Evaluate.EvaluateXPathToString("@file",
    //             testSetNode,
    //             null,
    //             new Dictionary<string, AbstractValue>(),
    //             new Options()))
    //         .ToList();
    //
    //     var joinedResult = $"[ {string.Join("\n", testFileNames.Select(r => r))} ]";
    //
    //     _testOutputHelper.WriteLine(joinedResult);
    // }
}

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