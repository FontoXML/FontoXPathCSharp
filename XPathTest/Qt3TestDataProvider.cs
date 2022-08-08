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
        // return (IEnumerator<object[]>)Array.Empty<object[]>().GetEnumerator();
        
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
        var allTestSets =  GetAllTestSets(qt3Tests, domFacade).SelectMany<string, object[]>(testSetFileName =>
        {
            var testSetData = Qt3TestUtils.LoadFileToXmlNode(testSetFileName);

            var testSetName = Evaluate.EvaluateXPathToString("/test-set/@name",
                testSetData,
                domFacade,
                null,
                new Options<XmlNode>(namespaceResolver: _ => "http://www.w3.org/2010/09/qt-fots-catalog"));

            var testCaseNodes = new List<XmlNode>(Evaluate.EvaluateXPathToNodes(Qt3TestQueries.AllTestsQuery,
                testSetData,
                domFacade,
                new Dictionary<string, AbstractValue>(),
                new Options<XmlNode>(namespaceResolver: _ => "http://www.w3.org/2010/09/qt-fots-catalog")));

            if (!testCaseNodes.Any()) return Array.Empty<object[]>() ;

            var testCasesReturn = testCaseNodes.Aggregate(new List<object[]>(), (testCases, testCase) =>
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

            return testCasesReturn;
        });

        return allTestSets.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private string GetTestDescription(string testSetName, string testName, XmlNode testCase, XmlNodeDomFacade domFacade)
    {
        //Fix this, something about it does not work yet
        var descriptionString = Evaluate.EvaluateXPathToString(
            "if (description/text()) then description else test",
            testCase,
            domFacade,
            new Dictionary<string, AbstractValue>(),
            new Options<XmlNode>(namespaceResolver: _ => "http://www.w3.org/2010/09/qt-fots-catalog")
        );
        
        return $"{testSetName}~{testName}~{descriptionString}";
    }

    private string GetTestName(XmlNode testCase, XmlNodeDomFacade domFacade)
    {
        return Evaluate.EvaluateXPathToString(
            "./@name",
            testCase,
            domFacade,
            new Dictionary<string, AbstractValue>(),
            new Options<XmlNode>(namespaceResolver: _ => "http://www.w3.org/2010/09/qt-fots-catalog"))!;
    }

    private static bool ParseBooleanNoFail(string input)
    {
        bool.TryParse(input, out var res);
        return res;
    }


    private IEnumerable<string> GetAllTestSets(XmlNode catalog, XmlNodeDomFacade domFacade)
    {
        return Evaluate.EvaluateXPathToNodes("/catalog/test-set", catalog, domFacade,
                new Dictionary<string, AbstractValue>(),
                new Options<XmlNode>(namespaceResolver: s => "http://www.w3.org/2010/09/qt-fots-catalog"))
            .Where(testSetNode => _shouldRunTestByName.Contains(Evaluate.EvaluateXPathToString("@name",
                                                                    testSetNode,
                                                                    domFacade,
                                                                    new Dictionary<string, AbstractValue>(),
                                                                    new Options<XmlNode>(namespaceResolver: _ =>
                                                                        "http://www.w3.org/2010/09/qt-fots-catalog")) ??
                                                                string.Empty))
            .Select(testSetNode => Evaluate.EvaluateXPathToString("@file",
                testSetNode,
                domFacade,
                new Dictionary<string, AbstractValue>(),
                new Options<XmlNode>(namespaceResolver: _ => "http://www.w3.org/2010/09/qt-fots-catalog")))
            .Cast<string>();
    }
}