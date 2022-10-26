using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using FontoXPathCSharp;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Types;

namespace XPathTest.Qt3Tests;

public class Qt3TestDataXmlNode : Qt3TestDataProvider<XmlNode>
{
    public Qt3TestDataXmlNode() : base(new XmlNodeDomFacade(), new XmlNodeUtils())
    {
    }
}

public class Qt3TestDataXObject : Qt3TestDataProvider<XObject>
{
    public Qt3TestDataXObject() : base(new XObjectDomFacade(), new XObjectUtils())
    {
    }
}

public abstract class Qt3TestDataProvider<TNode> : IEnumerable<object[]> where TNode : notnull
{
    private readonly IDomFacade<TNode> _domFacade;

    private readonly INodeUtils<TNode> _nodeUtils;

    private readonly Options<TNode> _options =
        new(_ => "http://www.w3.org/2010/09/qt-fots-catalog");

    private HashSet<string> _shouldRunTestByName = new();

    // private readonly List<object[]> _testCases = new();

    private HashSet<string> _unrunnableTestCasesByName = new();

    public Qt3TestDataProvider(IDomFacade<TNode> domFacade, INodeUtils<TNode> nodeUtils)
    {
        _domFacade = domFacade;
        _nodeUtils = nodeUtils;
    }

    public IEnumerator<object[]> GetEnumerator()
    {
        // return Array.Empty<object[]>().Cast<object[]>().GetEnumerator();
        if (TestFileSystem.FileExists("runnableTestSets.csv"))
            _shouldRunTestByName = TestFileSystem.ReadFile("runnableTestSets.csv")
                .Split(Environment.NewLine)
                .AsParallel()
                .Aggregate(new HashSet<string>(), (acc, line) =>
                {
                    var cols = line.Split(',');
                    if (cols.Length > 1 && !acc.Contains(cols[0]) && ParseBooleanNoFail(cols[1])) acc.Add(cols[0]);
                    return acc;
                });

        // if (Environment.GetEnvironmentVariable("REGENERATE") == "FALSE")
        // {
        if (TestFileSystem.FileExists("unrunnableTestCases.csv"))
            _unrunnableTestCasesByName = TestFileSystem.ReadFile("unrunnableTestCases.csv")
                .Split(Environment.NewLine)
                .AsParallel()
                .Aggregate(new HashSet<string>(), (acc, line) =>
                {
                    var cols = line.Split(';');
                    if (cols.Length > 1) acc.Add(cols[0]);
                    return acc;
                });
        // }

        var qt3Tests = _nodeUtils.LoadFileToXmlNode("catalog.xml")!;
        var allTestSets = GetAllTestSets(qt3Tests).SelectMany<string, object[]>(testSetFileName =>
        {
            var testSetData = _nodeUtils.LoadFileToXmlNode(testSetFileName)!;

            var testSetName = Evaluate.EvaluateXPathToString("/test-set/@name",
                testSetData,
                _domFacade,
                _options
            );

            var testCaseNodes = new List<TNode>(
                Evaluate.EvaluateXPathToNodes(Qt3TestQueries.AllTestsQuery,
                    testSetData,
                    _domFacade,
                    _options)
            );

            if (!testCaseNodes.Any()) return Array.Empty<object[]>();

            var testCasesReturn = testCaseNodes.Aggregate(new List<object[]>(), (testCases, testCase) =>
            {
                var testName = GetTestName(testCase);
                if (_unrunnableTestCasesByName.Contains(testName)) return testCases;

                try
                {
                    var description = GetTestDescription(testSetName!, testName, testCase);
                    var arguments = new Qt3TestArguments<TNode>(
                        testSetFileName,
                        testCase,
                        _domFacade,
                        _options,
                        _nodeUtils
                    );
                    testCases.Add(new object[]
                        { testName, testSetName!, description, testCase, arguments, _nodeUtils });
                }
                catch (FileNotFoundException ex)
                {
                    Console.WriteLine(ex.Message);
                }

                return testCases;
            });


            return testCasesReturn;
        });

        return allTestSets.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private string GetTestDescription(string testSetName, string testName, TNode testCase)
    {
        //Fix this, something about it does not work yet
        var descriptionString = Evaluate.EvaluateXPathToString(
            "if (description/text()) then description else test",
            testCase,
            _domFacade,
            _options
        );

        return $"{testSetName}~{testName}~{descriptionString}";
    }

    private string GetTestName(TNode testCase)
    {
        return Evaluate.EvaluateXPathToString(
            "./@name",
            testCase,
            _domFacade,
            _options)!;
    }

    private static bool ParseBooleanNoFail(string input)
    {
        bool.TryParse(input, out var res);
        return res;
    }


    private IEnumerable<string> GetAllTestSets(TNode catalog)
    {
        return Evaluate.EvaluateXPathToNodes(
                "/catalog/test-set",
                catalog,
                _domFacade,
                _options)
            .Where(testSetNode => _shouldRunTestByName.Contains(
                Evaluate.EvaluateXPathToString(
                    "@name",
                    testSetNode,
                    _domFacade,
                    _options) ??
                string.Empty))
            .Select(testSetNode => Evaluate.EvaluateXPathToString(
                "@file",
                testSetNode,
                _domFacade,
                _options))
            .Cast<string>();
    }
}