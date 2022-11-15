using System;
using System.Collections.Generic;
using System.Xml;
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
        RunTest(name, testSetName, description, testCase, arguments, nodeUtils);
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
        RunTest(name, testSetName, description, testCase, arguments, nodeUtils);
    }

    private void RunTest<TNode>(
        string name,
        string testSetName,
        string description,
        TNode testCase,
        Qt3TestArguments<TNode> arguments,
        INodeUtils<TNode> nodeUtils) where TNode : notnull
    {
        AsserterCall<TNode> asserter;
        try
        {
            asserter = Qt3Assertions<TNode>.GetExpressionBackendAsserterForTest(
                arguments.BaseUrl,
                testCase,
                arguments.Language,
                arguments.DomFacade,
                nodeUtils
            );
        }
        catch (Exception ex)
        {
            _loggingFixture.ProcessError(ex, name, testSetName, description, arguments);
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
            _loggingFixture.ProcessError(ex, name, testSetName, description, arguments);
            throw;
        }
    }
}