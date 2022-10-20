using System;
using System.Collections.Concurrent;
using System.Linq;

namespace XPathTest;

public class TestLoggingFixture : IDisposable
{
    //All of these are concurrent because the 
    private readonly ConcurrentDictionary<string, string> _castingErrors = new();
    private readonly ConcurrentDictionary<string, string> _failedTestsWithErrors = new();
    private readonly ConcurrentDictionary<string, string> _nonParseErrors = new();
    private readonly ConcurrentDictionary<string, string> _nullPointerExceptions = new();
    private readonly ConcurrentDictionary<string, string> _parseErrors = new();

    public void Dispose()
    {
        TestingUtils.WriteDictionaryToCsv(_failedTestsWithErrors, "unrunnableTestCases.csv", ',', true);

        if (!TestFileSystem.DirExists("debug")) TestFileSystem.CreateDir("debug");

        TestingUtils.WriteDictionaryToCsv(_nonParseErrors, "debug/nonParseUnrunnableTestCases.csv");
        TestingUtils.WriteDictionaryToCsv(_parseErrors, "debug/parseUnrunnableTestCases.csv");
        TestingUtils.WriteDictionaryToCsv(_nullPointerExceptions, "debug/nullPointerExceptions.csv");
        TestingUtils.WriteDictionaryToCsv(_castingErrors, "debug/castingExceptions.csv");
        TestingUtils.WriteOccurenceToCsv(_nonParseErrors.Values, "debug/mostCommonNonParseErrors.csv");
        TestingUtils.WriteOccurenceToCsv(_failedTestsWithErrors.Values, "debug/mostCommonErrors.csv");
    }

    public void ProcessError(Exception ex, string testName, string testSetName, string description)
    {
        var exceptionString = ex.Message
            .Replace(",", "<comma>")
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