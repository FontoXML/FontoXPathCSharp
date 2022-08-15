using System;
using System.Collections.Concurrent;
using System.Linq;

namespace XPathTest;

public class TestLoggingFixture : IDisposable
{
    private readonly ConcurrentDictionary<string, string> _castingErrors = new();
    private readonly ConcurrentDictionary<string, string> _failedTestsWithErrors = new();
    private readonly ConcurrentDictionary<string, string> _nonParseErrors = new();
    private readonly ConcurrentDictionary<string, string> _nullPointerExceptions = new();
    private readonly ConcurrentDictionary<string, string> _parseErrors = new();


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
        TestingUtils.WriteKvpCollectionToDisk(_castingErrors, "castingExceptions.csv");
    }

    public void ProcessError(Exception ex, string testName, string testSetName, string description)
    {
        var exceptionString = ex.Message
            .Replace(',', ' ')
            .ReplaceLineEndings()
            .Split(Environment.NewLine)
            .First();

        if (ex is NullReferenceException) _nullPointerExceptions[testName] = ex.ToString();
        if (ex is InvalidCastException) _castingErrors[testName] = ex.ToString();

        _failedTestsWithErrors[testName] = exceptionString;
        if (!exceptionString.Contains("PRSC Error")) _nonParseErrors[testName] = exceptionString;
        else _parseErrors[testName] = exceptionString;
    }
}