using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;
using Xunit.Abstractions;

namespace XPathTest;

public class Qt3TestDataProvider : IEnumerable<TestCase>
{
    private IEnumerable<TestCase> _testCases;

    private readonly List<string> _loadedTestSets;

    private readonly HashSet<string> _shouldRunTestByName;
    
    private Dictionary<string, string> _unrunnableTestCasesByName = new();

    public Qt3TestDataProvider(IEnumerable<TestCase> testCases)
    {
        _shouldRunTestByName = File.ReadLines("../../../assets/runnableTestSets.csv")
            .Select(line => line.Split(','))
            .DistinctBy(l => l[0])
            .Where(l => ParseBooleanNoFail(l[1]))
            .Select(l => l[0])
            .ToHashSet();

        var qt3tests = new XmlDocument();
        qt3tests.Load("../../../assets/QT3TS/catalog.xml");
        _loadedTestSets = GetAllTestSets(qt3tests);
        Console.WriteLine($"Qt3 Testsets loaded: {_loadedTestSets.Count}");

        _loadedTestSets.ForEach(testSetFileName =>
        {
            var testSet = Qt3TestUtils.LoadFileToXmlNode(testSetFileName);

            var testSetName = Evaluate.EvaluateXPathToString("/test-set/@name",
                testSet,
                null,
                new Dictionary<string, AbstractValue>(),
                new Options(namespaceResolver: _ => "http://www.w3.org/2010/09/qt-fots-catalog"));

            var testCaseNodes = new List<XmlNode>(Evaluate.EvaluateXPathToNodes(Qt3TestQueries.AllTestsQuery,
                testSet,
                null,
                new Dictionary<string, AbstractValue>(),
                new Options(namespaceResolver: _ => "http://www.w3.org/2010/09/qt-fots-catalog")));

            Console.WriteLine($"Loaded Testset Data: Set FileName: {testSetFileName}, Set Name: {testSetName}, Test Case Nodes: {testCaseNodes.Count}");

            var testName = Evaluate.EvaluateXPathToString(
                Qt3TestQueries.AllTestNameQuery,
                testSet,
                null,
                new Dictionary<string, AbstractValue>(),
                new Options(namespaceResolver: _ => "http://www.w3.org/2010/09/qt-fots-catalog"));

            if (!testCaseNodes.Any()) return;


            foreach (var testCase in testCaseNodes)
            {
                var name = GetTestName(testCase);
                var description = GetTestDescription(testSetName, name, testCase);
                var skip = _unrunnableTestCasesByName.ContainsKey(name);
                
                
            }
        });
    }

    private string GetTestDescription(string testSetName, string testName, XmlNode testCase)
    {
        return testSetName +
               '~' +
               testName +
               '~' + 
               Evaluate.EvaluateXPathToString(
                   "if (description/text()) then description else test",
                   testCase,
                   null,
                   new Dictionary<string, AbstractValue>(),
                   new Options(namespaceResolver: _ => "http://www.w3.org/2010/09/qt-fots-catalog"));
    }

    private string GetTestName(XmlNode testCase)
    {
        return Evaluate.EvaluateXPathToString(
            "./@name",
            testCase,
            null,
            new Dictionary<string, AbstractValue>(),
            new Options(namespaceResolver: _ => "http://www.w3.org/2010/09/qt-fots-catalog"));
    }

    private static bool ParseBooleanNoFail(string input)
    {
        bool.TryParse(input, out var res);
        return res;
    }



    private List<string> GetAllTestSets(XmlNode catalog)
    {
        return Evaluate.EvaluateXPathToNodes("/catalog/test-set", catalog, null,
                new Dictionary<string, AbstractValue>(),
                new Options(namespaceResolver: s => "http://www.w3.org/2010/09/qt-fots-catalog"))
            .Where(testSetNode => _shouldRunTestByName.Contains(Evaluate.EvaluateXPathToString("@name",
                testSetNode,
                null,
                new Dictionary<string, AbstractValue>(),
                new Options(namespaceResolver: _ => "http://www.w3.org/2010/09/qt-fots-catalog"))))
            .Select(testSetNode => Evaluate.EvaluateXPathToString("@file",
                testSetNode,
                null,
                new Dictionary<string, AbstractValue>(),
                new Options(namespaceResolver: _ => "http://www.w3.org/2010/09/qt-fots-catalog")))
            .ToList();
    }

    public IEnumerator<TestCase> GetEnumerator()
    {
        return _testCases.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}