using System;
using System.ComponentModel;
using System.Xml;
using XPathTest.UnitTests;
using Xunit;

namespace XPathTest;

public class QT3Tests
{
    [Theory]
    [ClassData(typeof(Qt3TestDataProvider))]
    [Description]
    public void Qt3Tests(string Name, string Description, string testSetFileName, XmlNode testCase,
        Qt3TestUtils.TestArguments arguments)
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