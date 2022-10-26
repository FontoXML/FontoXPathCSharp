using System;
using System.Collections.Concurrent;
using System.Linq;
using XPathTest.Qt3Tests;

namespace XPathTest;

public class TestLoggingFixture : IDisposable
{
    //All of these are concurrent because the 
    private readonly ConcurrentDictionary<string, string> _castingErrors = new();
    private readonly ConcurrentDictionary<string, string> _failedTestsWithErrors = new();
    private readonly ConcurrentDictionary<string, string> _nonParseErrors = new();
    private readonly ConcurrentDictionary<string, string> _nullPointerExceptions = new();
    private readonly ConcurrentDictionary<string, string> _parseErrors = new();
    private readonly ConcurrentDictionary<string, string> _documentNodeQueries = new();

    public void Dispose()
    {
        TestingUtils.WriteDictionaryToCsv(_failedTestsWithErrors, "unrunnableTestCases.csv", ';', true);
        TestingUtils.SortFileLines("unrunnableTestCases.csv");
        
        if (!TestFileSystem.DirExists("debug")) TestFileSystem.CreateDir("debug");

        TestingUtils.WriteDictionaryToCsv(_nonParseErrors, "debug/nonParseUnrunnableTestCases.csv");
        TestingUtils.WriteDictionaryToCsv(_parseErrors, "debug/parseUnrunnableTestCases.csv");
        TestingUtils.WriteDictionaryToCsv(_nullPointerExceptions, "debug/nullPointerExceptions.csv");
        TestingUtils.WriteDictionaryToCsv(_castingErrors, "debug/castingExceptions.csv");
        TestingUtils.WriteDictionaryToCsv(_documentNodeQueries, "debug/failedDocumentNodeQueries.csv");

        TestingUtils.WriteOccurenceToCsv(_nonParseErrors.Values, "debug/mostCommonNonParseErrors.csv");
        TestingUtils.WriteOccurenceToCsv(_failedTestsWithErrors.Values, "debug/mostCommonErrors.csv");
    }

    public void ProcessError<TNode>(Exception ex, string testName, string testSetName, string description, Qt3TestArguments<TNode> testArguments) where TNode : notnull
    {
        var exceptionString = ex.Message
            .ReplaceLineEndings()
            .Split(Environment.NewLine)
            .First();

        var query = testArguments.TestQuery;

        if (query.Contains("document-node")) _documentNodeQueries[testName] = $"Query: '{query}' Error: {exceptionString}";
        
        if (ex is NullReferenceException) _nullPointerExceptions[testName] = ex.ToString();
        if (ex is InvalidCastException) _castingErrors[testName] = ex.ToString();

        _failedTestsWithErrors[testName] = exceptionString;
        if (!exceptionString.Contains("PRSC Error")) _nonParseErrors[testName] = exceptionString;
        else _parseErrors[testName] = exceptionString;
    }
}