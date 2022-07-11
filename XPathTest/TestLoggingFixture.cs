using System;
using System.Collections.Concurrent;
using System.Linq;

namespace XPathTest;

public class TestLoggingFixture : IDisposable
{
    private readonly ConcurrentDictionary<string, string> _failedTestsWithErrors = new();
    private readonly ConcurrentDictionary<string, string> _nonParseErrors = new();


    public void Dispose()
    {
        TestingUtils.WriteKvpCollectionToDisk(_failedTestsWithErrors, "unrunnableTestCases.csv");
        TestingUtils.WriteKvpCollectionToDisk(_nonParseErrors, "nonParseUnrunnableTestCases.csv");
        TestingUtils.WriteKvpCollectionToDisk(
            TestingUtils.GetSortedValueOccurrences(_nonParseErrors.Values), "mostCommonNonParseErrors.csv");
        TestingUtils.WriteKvpCollectionToDisk(
            TestingUtils.GetSortedValueOccurrences(_failedTestsWithErrors.Values), "mostCommonErrors.csv");
    }

    public void ProcessError(Exception ex, string testName, string testSetName, string description)
    {
        var exceptionString = ex.Message
            .Replace(',', ' ')
            .ReplaceLineEndings()
            .Split(Environment.NewLine)
            .First();

        _failedTestsWithErrors[testName] = exceptionString;
        if (!exceptionString.Contains("PRSC Error")) _nonParseErrors[testName] = exceptionString;
    }
}