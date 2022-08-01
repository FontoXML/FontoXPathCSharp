using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;

namespace XPathTest;

public class Qt3TestDataProvider : IEnumerable<object[]>
{
    private readonly HashSet<string> _shouldRunTestByName = new();

    private readonly List<object[]> _testCases = new();

    private readonly HashSet<string> _unrunnableTestCasesByName = new();

    public IEnumerator<object[]> GetEnumerator()
    {
        if (TestFileSystem.FileExists("runnableTestSets.csv"))
            File.ReadLines("../../../assets/runnableTestSets.csv")
                .Select(line => line.Split(','))
                .DistinctBy(l => l[0])
                .Where(l => ParseBooleanNoFail(l[1]))
                .Select(l => l[0])
                .ToList()
                .ForEach(x => _shouldRunTestByName.Add(x));

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

        var domFacade = new XmlNodeDomFacade();

        var qt3Tests = Qt3TestUtils.LoadFileToXmlNode("catalog.xml");
        GetAllTestSets(qt3Tests, domFacade).ForEach(testSetFileName =>
        {
            var testSetData = Qt3TestUtils.LoadFileToXmlNode(testSetFileName);
            var testSet = new NodeValue<XmlNode>(testSetData, domFacade);

            var testSetName = Evaluate.EvaluateXPathToString("/test-set/@name",
                testSet,
                domFacade,
                new Dictionary<string, AbstractValue>(),
                new Options<XmlNode>(namespaceResolver: _ => "http://www.w3.org/2010/09/qt-fots-catalog"));

            var testCaseNodes = new List<XmlNode>(Evaluate.EvaluateXPathToNodes(Qt3TestQueries.AllTestsQuery,
                testSet,
                domFacade,
                new Dictionary<string, AbstractValue>(),
                new Options<XmlNode>(namespaceResolver: _ => "http://www.w3.org/2010/09/qt-fots-catalog")));

            if (!testCaseNodes.Any()) return;

            var testCases = testCaseNodes.Aggregate(new List<object[]>(), (testCases, testCase) =>
            {
                var testName = GetTestName(testCase, domFacade);
                if (!_unrunnableTestCasesByName.Contains(testName))
                    try
                    {
                        var name = GetTestName(testCase, domFacade);
                        var description = GetTestDescription(testSetName, name, testCase, domFacade);
                        var arguments = Qt3TestUtils.GetArguments(testSetFileName, testCase, domFacade);
                        testCases.Add(new object[] { name, testSetName, description, testCase, arguments });
                    }
                    catch (FileNotFoundException ex)
                    {
                        /* Test file was probably not found. */
                    }

                return testCases;
            });

            _testCases.AddRange(testCases);
        });
        return _testCases.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private string GetTestDescription(string testSetName, string testName, XmlNode testCase, XmlNodeDomFacade domFacade)
    {
        // return $"{testSetName}~{testName}";
        //TODO: More descriptive test description.
        return
            $"{testSetName}~{testName}~" +
            Evaluate.EvaluateXPathToString(
                "if (description/text()) then description else test",
                new NodeValue<XmlNode>(testCase, domFacade),
                domFacade,
                new Dictionary<string, AbstractValue>(),
                new Options<XmlNode>(namespaceResolver: _ => "http://www.w3.org/2010/09/qt-fots-catalog")
            );
    }

    private string GetTestName(XmlNode testCase, XmlNodeDomFacade domFacade)
    {
        return Evaluate.EvaluateXPathToString(
            "./@name",
            new NodeValue<XmlNode>(testCase, domFacade),
            domFacade,
            new Dictionary<string, AbstractValue>(),
            new Options<XmlNode>(namespaceResolver: _ => "http://www.w3.org/2010/09/qt-fots-catalog"))!;
    }

    private static bool ParseBooleanNoFail(string input)
    {
        bool.TryParse(input, out var res);
        return res;
    }


    private List<string> GetAllTestSets(XmlNode catalog, XmlNodeDomFacade domFacade)
    {
        return Evaluate.EvaluateXPathToNodes("/catalog/test-set", new NodeValue<XmlNode>(catalog, domFacade), domFacade,
                new Dictionary<string, AbstractValue>(),
                new Options<XmlNode>(namespaceResolver: s => "http://www.w3.org/2010/09/qt-fots-catalog"))
            .Where(testSetNode => _shouldRunTestByName.Contains(Evaluate.EvaluateXPathToString("@name",
                                                                    new NodeValue<XmlNode>(testSetNode, domFacade),
                                                                    domFacade,
                                                                    new Dictionary<string, AbstractValue>(),
                                                                    new Options<XmlNode>(namespaceResolver: _ =>
                                                                        "http://www.w3.org/2010/09/qt-fots-catalog")) ??
                                                                string.Empty))
            .Select(testSetNode => Evaluate.EvaluateXPathToString("@file",
                new NodeValue<XmlNode>(testSetNode, domFacade),
                domFacade,
                new Dictionary<string, AbstractValue>(),
                new Options<XmlNode>(namespaceResolver: _ => "http://www.w3.org/2010/09/qt-fots-catalog")))
            .Cast<string>()
            .ToList();
    }
}