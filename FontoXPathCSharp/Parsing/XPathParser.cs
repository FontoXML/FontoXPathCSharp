using FontoXPathCSharp.Expressions;
using PrscSharp;
using static PrscSharp.PrscSharp;
using static FontoXPathCSharp.Parsing.NameParser;
using static FontoXPathCSharp.Parsing.WhitespaceParser;

namespace FontoXPathCSharp.Parsing;

public static class XPathParser
{
    private static ParseFunc<ParseResult<string>> ForwardAxis()
    {
        return Map(Or(new[]
        {
            Token("self::")
            // TODO: add other variants
        }), x => x[..^2]);
    }


    private static ParseFunc<ParseResult<QName>> UnprefixedName()
    {
        return Map(NcName(), x =>
            new QName(x, null, ""));
    }

    private static ParseFunc<ParseResult<QName>> QName()
    {
        return Or(new[]
        {
            UnprefixedName()
            // TODO: add prefixed name
        });
    }

    private static ParseFunc<ParseResult<QName>> EqName()
    {
        return Or(new[]
        {
            QName()
            // TODO: add other options
        });
    }

    private static ParseFunc<ParseResult<Ast>> NameTest()
    {
        // TODO: add wildcard
        return Map(EqName(), x =>
            new Ast("nameTest")
            {
                StringAttributes =
                {
                    ["URI"] = x.NamespaceUri!,
                    ["prefix"] = x.Prefix!
                },
                TextContent = x.LocalName
            });
    }

    private static ParseFunc<ParseResult<Ast>> NodeTest()
    {
        return Or(new[] {NameTest()});
    }

    private static ParseFunc<ParseResult<Ast>> ForwardStep()
    {
        return Or(new[]
        {
            Then(ForwardAxis(), NodeTest(),
                (axis, test) =>
                {
                    var ast = new Ast("stepExpr");
                    ast.Children.Add(new Ast("xpathAxis") {TextContent = axis});
                    ast.Children.Add(test);
                    return ast;
                })
        });
    }

    private static ParseFunc<ParseResult<Ast>> AxisStep()
    {
        // TODO: add predicateList
        return Or(new[]
        {
            ForwardStep()
            // TODO: add reverse step
        });
    }

    private static ParseFunc<ParseResult<Ast>> StepExprWithForcedStep()
    {
        // TODO: add postfix expr with step
        return Or(new[]
        {
            AxisStep()
        });
    }

    private static ParseFunc<ParseResult<Ast>> RelativePathExpr()
    {
        return Or(new[]
        {
            // TODO: add other variants
            Map(StepExprWithForcedStep(), x =>
            {
                var ast = new Ast("pathExpr");
                ast.Children.Add(x);
                return ast;
            })
        });
    }

    public static ParseFunc<ParseResult<Ast>> PathExpr()
    {
        return Or(new[]
        {
            RelativePathExpr()
            // TODO: add other variants
        });
    }

    public static ParseFunc<ParseResult<string>> ReservedFunctionNames()
    {
        return Or(new[]
        {
            "array",
            "attribute",
            "comment",
            "document-node",
            "element",
            "empty-sequence",
            "function",
            "if",
            "item",
            "map",
            "namespace-node",
            "node",
            "processing-instruction",
            "schema-attribute",
            "schema-element",
            "switch",
            "text",
            "typeswitch"
        }.Select(Token).ToArray());
    }

    public static ParseFunc<ParseResult<Ast>> FunctionCall()
    {
        return Preceded(
            Not(Followed(ReservedFunctionNames(), new[] {Whitespace(), Token("(")}),
                new[] {"cannot use reserved keyword for function names"}),
            // TODO: add support for function arguments
            Then(EqName(), Token("()"),
                (name, args) =>
                {
                    var ast = new Ast("functionCallExpr");
                    ast.Children.Add(name.GetAst("functionName"));
                    // TODO: add children here
                    return ast;
                }
            )
        );
    }
}