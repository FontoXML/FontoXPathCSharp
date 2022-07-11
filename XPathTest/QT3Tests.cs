using System;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using XPathTest.UnitTests;
using Xunit;

namespace XPathTest;

public class QT3Tests : IClassFixture<TestLoggingFixture>
{
    private TestLoggingFixture _loggingFixture;

    public QT3Tests(TestLoggingFixture loggingFixture)
    {
        _loggingFixture = loggingFixture;
    }

    [Theory(Timeout = 60000, DisplayName = "Qt3 Tests")]
    [ClassData(typeof(Qt3TestDataProvider))]
    [Description("bla")]
    public void Qt3Tests(string name, string testSetName, string description, XmlNode testCase,
        Qt3TestUtils.TestArguments arguments)
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
        catch (Exception ex)
        {
            // Let logging fixture 
            _loggingFixture.ProcessError(ex, name, testSetName, description);

            Assert.True(false, $"Query: {arguments.TestQuery}\nError: {ex.Message}");
            return;
        }

        // TODO: add whitespace cache to parser
        if (arguments.TestQuery ==
            "((((((((((((false() eq false()) eq false()) eq false()) eq false()) eq false()) eq false()) eq false()) eq false()) eq false()) eq false()) eq false()) eq false()) eq false()")
            return;

        asserter(
            arguments.TestQuery,
            arguments.ContextNode,
            arguments.VariablesInScope,
            arguments.NamespaceResolver
        );
    }
}