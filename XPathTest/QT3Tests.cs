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
    public void Qt3Tests(string name, string testSetName, string description, XmlNode testCase, Qt3TestUtils.TestArguments arguments)
    {
        AsserterCall asserter;
        try
        {
            asserter = Qt3Assertions.GetExpressionBackendAsserterForTest(
                arguments.BaseUrl,
                testCase,
                arguments.Language
            );
        }
        catch(Exception ex)
        {
            Assert.True(false, ex.Message);
            return;
        }
        
        asserter(
            arguments.TestQuery,
            arguments.ContextNode,
            arguments.VariablesInScope,
            arguments.NamespaceResolver
        );
    }
}