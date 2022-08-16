using System;
using System.Xml;
using System.Xml.Linq;
using XPathTest.Qt3Tests;
using XPathTest.UnitTests;
using Xunit;

namespace XPathTest;

public class Qt3TestRunners : IClassFixture<TestLoggingFixture>
{
    private readonly TestLoggingFixture _loggingFixture;

    public Qt3TestRunners(TestLoggingFixture loggingFixture)
    {
        _loggingFixture = loggingFixture;
    }

    [Theory(Timeout = 60000, DisplayName = "Qt3 Tests: XmlNode API")]
    [ClassData(typeof(Qt3TestDataXmlNode))]
    public void RunQt3TestXmlNode(
        string name,
        string testSetName,
        string description,
        XmlNode testCase,
        Qt3TestArguments<XmlNode> arguments,
        NodeUtils<XmlNode> nodeUtils)
    {
        AsserterCall<XmlNode> asserter;
        try
        {
            asserter = Qt3Assertions<XmlNode>.GetExpressionBackendAsserterForTest(
                arguments.BaseUrl,
                testCase,
                arguments.Language,
                arguments.DomFacade,
                nodeUtils);
        }
        catch (Exception ex)
        {
            // Let logging fixture 
            _loggingFixture.ProcessError(ex, name, testSetName, description);
            throw;
        }

        try
        {
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
        catch (Exception ex)
        {
            _loggingFixture.ProcessError(ex, name, testSetName, description);
            throw;
        }
    }

    [Theory(Timeout = 60000, DisplayName = "Qt3 Tests: LINQ XML API")]
    [ClassData(typeof(Qt3TestDataXObject))]
    public void RunQt3TestXObject(
        string name,
        string testSetName,
        string description,
        XObject testCase,
        Qt3TestArguments<XObject> arguments,
        NodeUtils<XObject> nodeUtils)
    {
        AsserterCall<XObject> asserter;
        try
        {
            asserter = Qt3Assertions<XObject>.GetExpressionBackendAsserterForTest(
                arguments.BaseUrl,
                testCase,
                arguments.Language,
                arguments.DomFacade,
                nodeUtils);
        }
        catch (Exception ex)
        {
            _loggingFixture.ProcessError(ex, name, testSetName, description);
            throw;
        }

        try
        {
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
        catch (Exception ex)
        {
            _loggingFixture.ProcessError(ex, name, testSetName, description);
            throw;
        }
    }
}