using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.Types;
using Xunit;

namespace XPathTest;

public class QT3Tests
{
    private Dictionary<string, bool> ShouldRunTestByName = new();

    [Fact]
    public void Qt3TestSet()
    {
        var qt3tests = new XmlDocument();
        qt3tests.Load("../../../assets/QT3TS/catalog.xml");
        GetAllTestSets(qt3tests).ForEach(testSetFileName =>
        {
            Console.WriteLine(testSetFileName);
        });
    }

    private List<string> GetAllTestSets(XmlNode catalog)
    {
        return Evaluate.EvaluateXPathToNodes("/catalog/test-set", catalog, null,
                new Dictionary<string, IExternalValue>(), new Options())
            .Where(testSetNode =>
                ShouldRunTestByName[
                    Evaluate.EvaluateXPathToString("@name",
                        testSetNode,
                        null,
                        new Dictionary<string, IExternalValue>(),
                        new Options())])
            .Select(testSetNode => Evaluate.EvaluateXPathToString("@file",
                testSetNode,
                null,
                new Dictionary<string, IExternalValue>(),
                new Options()))
            .ToList();
    }
}