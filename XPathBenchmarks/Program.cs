﻿// To run the benchmarks, execute the following commands:
// dotnet build -c Release
// cd .\bin\Release\net6.0\
// .\XPathBenchmarks.exe
//

using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using FontoXPathCSharp;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Parsing;
using FontoXPathCSharp.Sequences;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;

#if DEBUG
BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, new DebugBuildConfig());
#else
BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
#endif

internal static class Helper
{
    internal static AbstractExpression<XmlNode> CompileExpression(string expression)
    {
        var result = XPathParser.Parse(expression, new ParseOptions(false, true)).UnwrapOr((expected, fatal) =>
        {
            Console.WriteLine("Parsing error ({0}): {1}", fatal, string.Join(", ", expected));
            Environment.Exit(1);
            return new Ast(AstNodeName.All);
        });

        return CompileAstToExpression<XmlNode>.CompileAst(result, new CompilationOptions(true, false, true, true));
    }
}

[MemoryDiagnoser]
public class BooleanExpressionBenchmark
{
    private static readonly XNode Source = new XElement("test");
    private static readonly XPathExpression CompiledExpression = XPathExpression.Compile("false()");
    private static readonly AbstractExpression<XmlNode> Expr = Helper.CompileExpression("false()");

    [Benchmark(Baseline = true)]
    public bool BuiltIn_Evaluate()
    {
        return (bool)Source.XPathEvaluate("false()");
    }

    [Benchmark]
    public bool BuiltIn_Compiled()
    {
        var navigator = Source.CreateNavigator();
        return (bool)navigator.Evaluate(CompiledExpression);
    }

    [Benchmark]
    public object FontoXPath()
    {
        return Expr.Evaluate(new DynamicContext(
                null,
                0,
                SequenceFactory.CreateEmpty(),
                new Dictionary<string, Func<ISequence>>()),
            null);
    }
}

[MemoryDiagnoser]
public class SimpleExpressionBenchmark
{
    private const string Selector = "self::p";

    private static readonly AbstractExpression<XmlNode> Expr = Helper.CompileExpression(Selector);
    private static readonly XPathExpression CompiledExpression = XPathExpression.Compile(Selector);
    private readonly DomFacade<XmlNode> _domFacade = new(new XmlNodeDomFacade());
    private readonly XmlNode _source;

    public SimpleExpressionBenchmark()
    {
        var doc = new XmlDocument();
        doc.LoadXml("<p />");
        _source = doc.DocumentElement!;
    }

    [Benchmark(Baseline = true)]
    public object BuiltIn_Evaluate()
    {
        var navigator = _source.CreateNavigator()!;
        return navigator.Evaluate(Selector);
    }

    [Benchmark]
    public object BuiltIn_Compiled()
    {
        var navigator = _source.CreateNavigator()!;
        return navigator.Evaluate(CompiledExpression);
    }

    [Benchmark]
    public object FontoXPath()
    {
        return Expr.Evaluate(
            new DynamicContext(
                new NodeValue<XmlNode>(_source, _domFacade),
                0,
                SequenceFactory.CreateEmpty(),
                new Dictionary<string, Func<ISequence>>()),
            new ExecutionParameters<XmlNode>(false, false, _domFacade, _source));
    }
}

public class AttributeBenchmark
{
    private const string Selector = "self::p[@class='title']";
    private static readonly AbstractExpression<XmlNode> Expr = Helper.CompileExpression(Selector);
    private static readonly XPathExpression CompiledExpression = XPathExpression.Compile(Selector);
    private readonly DomFacade<XmlNode> _domFacade = new(new XmlNodeDomFacade());
    private readonly XmlNode _source;

    public AttributeBenchmark()
    {
        var doc = new XmlDocument();
        doc.LoadXml("<p class='title'></p>");
        _source = doc.DocumentElement!;
    }

    [Benchmark(Baseline = true)]
    public object BuiltIn_Evaluate()
    {
        var navigator = _source.CreateNavigator()!;
        return navigator.Evaluate(Selector);
    }

    [Benchmark]
    public object BuiltIn_Compiled()
    {
        var navigator = _source.CreateNavigator()!;
        return navigator.Evaluate(CompiledExpression);
    }

    [Benchmark]
    public object FontoXPath()
    {
        return Expr.Evaluate(
            new DynamicContext(
                new NodeValue<XmlNode>(_source, _domFacade),
                0,
                SequenceFactory.CreateEmpty(), new Dictionary<string, Func<ISequence>>()),
            new ExecutionParameters<XmlNode>(false, false, _domFacade, _source));
    }
}


public class PropertyBenchmark
{
    private const string Selector = "self::p[parent::sec]";
    private static readonly AbstractExpression<XmlNode> Expr = Helper.CompileExpression(Selector);
    private static readonly XPathExpression CompiledExpression = XPathExpression.Compile(Selector);
    private readonly DomFacade<XmlNode> _domFacade = new(new XmlNodeDomFacade());
    private readonly XmlNode _source;

    public PropertyBenchmark()
    {
        var doc = new XmlDocument();
        doc.LoadXml("<p><q title='q'>r</q><sec>q</sec></p>");
        _source = doc.DocumentElement!;
    }

    [Benchmark(Baseline = true)]
    public object BuiltIn_Evaluate()
    {
        var navigator = _source.CreateNavigator()!;
        return navigator.Evaluate(Selector);
    }

    [Benchmark]
    public object BuiltIn_Compiled()
    {
        var navigator = _source.CreateNavigator()!;
        return navigator.Evaluate(CompiledExpression);
    }

    [Benchmark]
    public object FontoXPath()
    {
        return Expr.Evaluate(
            new DynamicContext(
                new NodeValue<XmlNode>(_source, _domFacade),
                0,
                SequenceFactory.CreateEmpty(), new Dictionary<string, Func<ISequence>>()),
            new ExecutionParameters<XmlNode>(false, false, _domFacade, _source));
    }
}