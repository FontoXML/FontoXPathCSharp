using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using Xunit;

namespace XPathTest.Qt3Tests;

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
        INodeUtils<XmlNode> nodeUtils)
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
            asserter(
                arguments.TestQuery,
                arguments.ContextNode!,
                arguments.VariablesInScope ?? new Dictionary<string, object>(),
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
        INodeUtils<XObject> nodeUtils)
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
            asserter(
                arguments.TestQuery,
                arguments.ContextNode,
                arguments.VariablesInScope ?? new Dictionary<string, object>(),
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