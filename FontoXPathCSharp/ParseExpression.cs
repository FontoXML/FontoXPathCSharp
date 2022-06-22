using FontoXPathCSharp.Parsing;
using FontoXPathCSharp.Types;
using PrscSharp;

namespace FontoXPathCSharp;

public class ParseExpression
{
    public static Ast ParseXPathOrXQueryExpression(string xPathString, CompilationOptions compilationOptions)
    {
        var options = new ParseOptions(
            compilationOptions.Debug,
            compilationOptions.AllowXQuery
        );

        return XPathParser.Parse(xPathString, options) switch
        {
            Err<Ast> err => throw new Exception("PRSC Error:\n" + string.Join('\n', err.Expected) + " Actual: " +
                                                xPathString[err.Offset]),
            Ok<Ast> ok => ok.Unwrap(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}