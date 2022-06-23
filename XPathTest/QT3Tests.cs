using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.Types;
using Xunit;
using Xunit.Abstractions;

namespace XPathTest;

public class QT3Tests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private Dictionary<string, bool> ShouldRunTestByName = new();

    public QT3Tests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void Qt3TestSet()
    {
        var qt3tests = new XmlDocument();
        qt3tests.Load("../../../assets/QT3TS/catalog.xml");
        GetAllTestSets(qt3tests).ForEach(testSetFileName => { _testOutputHelper.WriteLine(testSetFileName); });
    }

    private List<string> GetAllTestSets(XmlNode catalog)
    {
        return Evaluate.EvaluateXPathToNodes("/catalog/test-set", catalog, null,
                new Dictionary<string, IExternalValue>(), new Options())
            .Where(testSetNode =>
                {
                    // TODO: Remove this try-catch when we properly support things.
                    try
                    {
                        return ShouldRunTestByName[
                            Evaluate.EvaluateXPathToString("@name",
                                testSetNode,
                                null,
                                new Dictionary<string, IExternalValue>(),
                                new Options())];
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }
            )
            .Select(testSetNode => Evaluate.EvaluateXPathToString("@file",
                testSetNode,
                null,
                new Dictionary<string, IExternalValue>(),
                new Options()))
            .ToList();
    }
}