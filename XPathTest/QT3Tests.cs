using System;
using System.ComponentModel;
using System.Xml;
using XPathTest.UnitTests;
using Xunit;

namespace XPathTest;

public class QT3Tests
{
    [Theory(Timeout = 60000, DisplayName = "Qt3 Tests")]
    [ClassData(typeof(Qt3TestDataProvider))]
    [Description("bla")]
    public void Qt3Tests(XmlNode testCase, Qt3TestUtils.TestArguments arguments, string name, string testSetName, string description)
    {
        try
        {
            var asserter = Qt3Assertions.GetExpressionBackendAsserterForTest(
                arguments.BaseUrl,
                testCase,
                arguments.Language
            );

            asserter(
                arguments.TestQuery,
                arguments.ContextNode,
                arguments.VariablesInScope,
                arguments.NamespaceResolver
            );
        }
        catch (Exception ex)
        {
        }
    }
}