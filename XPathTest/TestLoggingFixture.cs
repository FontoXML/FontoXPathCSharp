using System;
using System.Collections.Concurrent;
using System.Linq;

namespace XPathTest;

public class TestLoggingFixture : IDisposable
{
    private readonly ConcurrentDictionary<string, string> _failedTestsWithErrors = new();
    private readonly ConcurrentDictionary<string, string> _nonParseErrors = new();
    private readonly ConcurrentDictionary<string, string> _parseErrors = new();
    private readonly ConcurrentDictionary<string, string> _nullPointerExceptions = new();


    public void Dispose()
    {
        TestingUtils.WriteKvpCollectionToDisk(_failedTestsWithErrors, "unrunnableTestCases.csv");
        TestingUtils.WriteKvpCollectionToDisk(_nonParseErrors, "nonParseUnrunnableTestCases.csv");
        TestingUtils.WriteKvpCollectionToDisk(_parseErrors, "parseUnrunnableTestCases.csv");
        TestingUtils.WriteKvpCollectionToDisk(
            TestingUtils.GetSortedValueOccurrences(_nonParseErrors.Values), "mostCommonNonParseErrors.csv");
        TestingUtils.WriteKvpCollectionToDisk(
            TestingUtils.GetSortedValueOccurrences(_failedTestsWithErrors.Values), "mostCommonErrors.csv");
        TestingUtils.WriteKvpCollectionToDisk(_nullPointerExceptions, "nullPointerExceptions.csv");
    }

    public void ProcessError(Exception ex, string testName, string testSetName, string description)
    {
        var exceptionString = ex.Message
            .Replace(',', ' ')
            .ReplaceLineEndings()
            .Split(Environment.NewLine)
            .First();

        if (exceptionString.Contains("Object reference not set"))
        {
            _nullPointerExceptions[testName] = ex.Message
                .Replace(',', ' ')
                .ReplaceLineEndings()
                .Replace(Environment.NewLine, "/");
        }
        
        _failedTestsWithErrors[testName] = exceptionString;
        if (!exceptionString.Contains("PRSC Error")) _nonParseErrors[testName] = exceptionString;
        else { _parseErrors[testName] = exceptionString; }
    }
}