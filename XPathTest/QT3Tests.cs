using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.Types;
using Xunit;
using Xunit.Abstractions;

namespace XPathTest;

public class QT3Tests
{
    private const string AllTestNameQuery =
        @"/test-set/@name || ""~"" || /test-set/description";

    private const string AllTestsQuery = @"
/test-set/test-case[
    not(exists((./dependency | ../dependency)[@type=""xml-version"" and @value=""1.1""])) and not(
     (./dependency | ../dependency)/@value/tokenize(.) = (
       ""XQ10"",
       ""XQ20"",
       ""XQ30"",
       ""schemaValidation"",
       ""schemaImport"",
       (:""staticTyping"",:)
       (:""serialization"",:)
       ""infoset-dtd"",
       (:""xpath-1.0-compatibility"",:)
       ""namespace-axis"",
       (:""moduleImport"",:)
       ""schema-location-hint"",
       (:""collection-stability"",:)
       ""directory-as-collation-uri"",
       (:""fn-transform-XSLT"",:)
       (:""fn-transform-XSLT30"",:)
       (:""fn-format-integer-CLDR"",:)
       (:""non-empty-sequence-collection"",:)
       ""non-unicode-codepoint-collation"",
       ""simple-uca-fallback"",
       ""advanced-uca-fallback""))]
";

    private readonly List<string> _loadedTestSets;

    private readonly HashSet<string> _shouldRunTestByName;
    private readonly ITestOutputHelper _testOutputHelper;

    public QT3Tests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _shouldRunTestByName = File.ReadLines("../../../assets/runnableTestSets.csv")
            .Select(line => line.Split(','))
            .DistinctBy(l => l[0])
            .Where(l => ParseBooleanNoFail(l[1]))
            .Select(l => l[0])
            .ToHashSet();

        var qt3tests = new XmlDocument();
        qt3tests.Load("../../../assets/QT3TS/catalog.xml");
        _loadedTestSets = GetAllTestSets(qt3tests);
        _testOutputHelper.WriteLine($"Qt3 Testsets loaded: {_loadedTestSets.Count}");
    }

    private static bool ParseBooleanNoFail(string input)
    {
        bool.TryParse(input, out var res);
        return res;
    }

    [Fact]
    public void RunQt3TestSets()
    {
        _loadedTestSets.ForEach(testSetFileName =>
        {
            var testSet = LoadXmlFile(testSetFileName);

            var testSetName = Evaluate.EvaluateXPathToString("/test-set/@name",
                testSet,
                null,
                new Dictionary<string, IExternalValue>(),
                new Options(namespaceResolver: _ => "http://www.w3.org/2010/09/qt-fots-catalog"));

            var testCases = Evaluate.EvaluateXPathToNodes(AllTestsQuery,
                testSet,
                null,
                new Dictionary<string, IExternalValue>(),
                new Options(namespaceResolver: _ => "http://www.w3.org/2010/09/qt-fots-catalog"));

            _testOutputHelper.WriteLine("LOADED STUFF: {0}: {1}: {2}", testSetFileName, testSetName, testCases.Count());

            var testName = Evaluate.EvaluateXPathToString(
                AllTestNameQuery,
                testSet,
                null,
                new Dictionary<string, IExternalValue>(),
                new Options(namespaceResolver: _ => "http://www.w3.org/2010/09/qt-fots-catalog"));
            
            if (!testCases.Any()) return;
        });
    }

    private XmlDocument LoadXmlFile(string fileName)
    {
        var xmlFile = new XmlDocument();

        xmlFile.Load($"../../../assets/QT3TS/{fileName}");

        return xmlFile;
    }

    private List<string> GetAllTestSets(XmlNode catalog)
    {
        return Evaluate.EvaluateXPathToNodes("/catalog/test-set", catalog, null,
                new Dictionary<string, IExternalValue>(),
                new Options(namespaceResolver: s => "http://www.w3.org/2010/09/qt-fots-catalog"))
            .Where(testSetNode => _shouldRunTestByName.Contains(Evaluate.EvaluateXPathToString("@name",
                testSetNode,
                null,
                new Dictionary<string, IExternalValue>(),
                new Options(namespaceResolver: _ => "http://www.w3.org/2010/09/qt-fots-catalog"))))
            .Select(testSetNode => Evaluate.EvaluateXPathToString("@file",
                testSetNode,
                null,
                new Dictionary<string, IExternalValue>(),
                new Options(namespaceResolver: _ => "http://www.w3.org/2010/09/qt-fots-catalog")))
            .ToList();
    }
}