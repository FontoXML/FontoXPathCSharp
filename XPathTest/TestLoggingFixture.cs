using System;
using System.Collections.Concurrent;
using System.Linq;
using XPathTest.Qt3Tests;

namespace XPathTest;

public class TestLoggingFixture : IDisposable
{
    // All of these are concurrent because XUnit might execute things in parallel.
    private readonly ConcurrentDictionary<string, string> _castingErrors = new();
    private readonly ConcurrentDictionary<string, string> _failedTestsWithErrors = new();
    private readonly ConcurrentDictionary<string, string> _nonParseErrors = new();
    private readonly ConcurrentDictionary<string, string> _nullPointerExceptions = new();
    private readonly ConcurrentDictionary<string, string> _parseErrors = new();
    // Just keeping this one around, since it comes in handy sometimes.
    private readonly ConcurrentDictionary<string, string> _specificErrors = new();

    public void Dispose()
    {
        TestingUtils.WriteDictionaryToCsv(_failedTestsWithErrors, "unrunnableTestCases.csv", ';', true);
        TestingUtils.SortFileLines("unrunnableTestCases.csv");
        
        if (!TestFileSystem.DirExists("debug")) TestFileSystem.CreateDir("debug");

        TestingUtils.WriteDictionaryToCsv(_nonParseErrors, "debug/nonParseUnrunnableTestCases.csv",';' );
        TestingUtils.WriteDictionaryToCsv(_parseErrors, "debug/parseUnrunnableTestCases.csv", ';' );
        TestingUtils.WriteDictionaryToCsv(_nullPointerExceptions, "debug/nullPointerExceptions.csv", ';');
        TestingUtils.WriteDictionaryToCsv(_castingErrors, "debug/castingExceptions.csv", ';');
        TestingUtils.WriteDictionaryToCsv(_specificErrors, "debug/specificErrors.csv", ';');

        TestingUtils.WriteOccurenceToCsv(_nonParseErrors.Values, "debug/mostCommonNonParseErrors.csv", ';');
        TestingUtils.WriteOccurenceToCsv(_failedTestsWithErrors.Values, "debug/mostCommonErrors.csv", ';');
    }
    
    public void ProcessError<TNode>(
        Exception ex, 
        string testName, 
        string testSetName, 
        string description, 
        Qt3TestArguments<TNode> arguments) where TNode : notnull
    {
        var exceptionString = ex.Message
            .ReplaceLineEndings()
            .Split(Environment.NewLine)
            .First();
        
        var query = arguments.TestQuery;

        if (ex is NullReferenceException) _nullPointerExceptions[testName] = ex.ToString();
        if (ex is InvalidCastException) _castingErrors[testName] = ex.ToString();

        // if (exceptionString.Contains("Invalid prefix for input"))
            // _specificErrors[testName] = $"Query: '{arguments.TestQuery}', Error: '{exceptionString}'";
        
        _failedTestsWithErrors[testName] = exceptionString;
        if (!exceptionString.Contains("PRSC Error")) _nonParseErrors[testName] = exceptionString;
        else _parseErrors[testName] = exceptionString;
    }
}