using FontoXPathCSharp.Parsing;
using FontoXPathCSharp.Types;
using PrscSharp;

namespace FontoXPathCSharp;

public static class ParseExpression
{
    public static Ast ParseXPathOrXQueryExpression(string xPathString, CompilationOptions compilationOptions)
    {
        var options = new ParseOptions(
            compilationOptions.Debug,
            compilationOptions.AllowXQuery
        );

        return XPathParser.Parse(xPathString, options) switch
        {
            Err<Ast> err => throw new Exception(
                $"PRSC Error: Failed to parse query '{ReformatXPathString(xPathString)}'\n\n" +
                $"Expected: {string.Join('\n', err.Expected.Distinct())}"),
            Ok<Ast> ok => ok.Unwrap(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static string ReformatXPathString(string input)
    {
        return input.ReplaceLineEndings().Replace(Environment.NewLine, "\\n");
    }
}