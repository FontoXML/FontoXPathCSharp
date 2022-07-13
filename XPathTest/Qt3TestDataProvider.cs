using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;

namespace XPathTest;

public class Qt3TestDataProvider : IEnumerable<object[]>
{
    private readonly List<string> _loadedTestSets;

    private readonly HashSet<string> _shouldRunTestByName;

    private readonly HashSet<string> _unrunnableTestCasesByName = new();
    private readonly List<object[]> _testCases;

    public Qt3TestDataProvider()
    {
        if (TestFileSystem.FileExists("runnableTestSets.csv"))
        {
            _shouldRunTestByName = File.ReadLines("../../../assets/runnableTestSets.csv")
                .Select(line => line.Split(','))
                .DistinctBy(l => l[0])
                .Where(l => ParseBooleanNoFail(l[1]))
                .Select(l => l[0])
                .ToHashSet();
        }


        // _shouldRunTestByName = TestFileSystem.ReadFile("runnableTestSets.csv")
        //     .Split(Environment.NewLine)
        //     .Select(line => line.Split(','))
        //     .DistinctBy(l => l[0])
        //     .Where(l => ParseBooleanNoFail(l[1]))
        //     .Select(l => l[0])
        //     .ToHashSet();

        // Addinf failed test cases that come from parse errors to the ignore set.
        if (TestFileSystem.FileExists("parseUnrunnableTestCases.csv"))
        {
            var parseErrorCases = TestFileSystem.ReadFile("parseUnrunnableTestCases.csv")
                .Split(Environment.NewLine)
                .Select(e => e.Split(','))
                .Where(e => e.Length > 1)
                .ToDictionary(
                    e => e[0],
                    e => e[1]
                );

            parseErrorCases.Aggregate(
                _unrunnableTestCasesByName,
                (acc, val) =>
                {
                    acc.Add(val.Key);
                    return acc;
                }
            );
        }


        _testCases = new List<object[]>();

        var qt3Tests = Qt3TestUtils.LoadFileToXmlNode("catalog.xml");

        GetAllTestSets(qt3Tests).ForEach(testSetFileName =>
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

            if (!testCaseNodes.Any()) return;

            var testCases = testCaseNodes.Aggregate(new List<object[]>(), (testCases, testCase) =>
            {
                var testName = GetTestName(testCase);
                if (!_unrunnableTestCasesByName.Contains(testName))
                {
                    try
                    {
                        var name = GetTestName(testCase);
                        var description = GetTestDescription(testSetName, name, testCase);
                        var arguments = Qt3TestUtils.GetArguments(testSetFileName, testCase);
                        testCases.Add(new object[] { name, testSetName, description, testCase, arguments });
                    }
                    catch (FileNotFoundException ex)
                    {
                        /* Test file was probably not found. */
                    }
                }
                return testCases;
            });

            _testCases.AddRange(testCases);
        });
    }

    public IEnumerator<object[]> GetEnumerator()
    {
        return _testCases.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private string GetTestDescription(string testSetName, string testName, XmlNode testCase)
    {
        return $"{testSetName}~{testName}";
        //TODO: More descriptive test description.
        // return testSetName +
        //        '~' +
        //        testName +
        //        '~' +
        //        Evaluate.EvaluateXPathToString(
        //            "if (description/text()) then description else test",
        //            testCase,
        //            null,
        //            new Dictionary<string, AbstractValue>(),
        //            new Options(namespaceResolver: _ => "http://www.w3.org/2010/09/qt-fots-catalog"));
    }

    private string GetTestName(XmlNode testCase)
    {
        return Evaluate.EvaluateXPathToString(
            "./@name",
            testCase,
            null,
            new Dictionary<string, AbstractValue>(),
            new Options(namespaceResolver: _ => "http://www.w3.org/2010/09/qt-fots-catalog"))!;
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
}