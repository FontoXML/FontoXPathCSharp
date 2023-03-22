using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Expressions;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;
using Xunit;

namespace XPathTest.UnitTests;

public class TestRegisterCustomXPathFunction
{
    private static readonly XmlDocument XmlNodeEmptyContext;
    private static readonly XmlNodeDomFacade XmlNodeDomFacade;
    private static readonly Options<XmlNode> XmlNodeOptions;

    private static readonly XmlDocument XmlNodeWorksMod;

    private static readonly NamespaceResolver IdentityNamespaceResolver = prefix => prefix;

    static TestRegisterCustomXPathFunction()
    {
        XmlNodeEmptyContext = new XmlDocument();
        XmlNodeDomFacade = new XmlNodeDomFacade();
        XmlNodeOptions = new Options<XmlNode>(IdentityNamespaceResolver);

        XmlNodeWorksMod = new XmlDocument();
        XmlNodeWorksMod.LoadXml(TestFileSystem.ReadFile("qt3tests/docs/works-mod.xml"));

        RegisterSimpleFunctions<XmlNode>();
        RegisterComplexFunctions<XmlNode>();
    }

    private static void VerifyDynamicContext<TNode>(DynamicContextAdapter<TNode>? dynamicContext) where TNode : notnull
    {
        Assert.True(dynamicContext != null, "A dynamic context has not been passed");
        Assert.True(dynamicContext?.DomFacade != null, "A domFacade has not been passed");
    }

    private static void RegisterComplexFunctions<TNode>() where TNode : notnull
    {
        RegisterCustomXPathFunction<TNode>.RegisterFunction<string?, bool>(
            new QName("custom-function1", "test"),
            new[] { "xs:string?" },
            "xs:boolean",
            (dynamicContext, arg1) =>
            {
                VerifyDynamicContext(dynamicContext);
                return arg1 is null or "test";
            }
        );

        RegisterCustomXPathFunction<TNode>.RegisterFunction<string, bool, bool>(
            new QName("custom-function2", "test"),
            new[] { "xs:string", "xs:boolean" },
            "xs:boolean",
            (dynamicContext, stringArg, boolArg) =>
            {
                VerifyDynamicContext(dynamicContext);
                return stringArg == "test" && boolArg;
            }
        );

        RegisterCustomXPathFunction<TNode>.RegisterFunction<object[], object>(
            new QName("custom-function3", "test"),
            new[] { "item()*" },
            "item()",
            (dynamicContext, args) =>
            {
                VerifyDynamicContext(dynamicContext);
                return args[0];
            }
        );


        RegisterCustomXPathFunction<TNode>.RegisterFunction<string[], string[]>(
            new QName("custom-function4", "test"),
            new[] { "xs:string*" },
            "xs:string*",
            (dynamicContext, stringArray) =>
            {
                VerifyDynamicContext(dynamicContext);
                return stringArray.Select(stringValue => stringValue + "-test").ToArray();
            }
        );

        RegisterCustomXPathFunction<TNode>.RegisterFunction<string?, string?>(
            new QName("custom-function5", "test"),
            new[] { "xs:string?" },
            "xs:string?",
            (dynamicContext, stringValue) =>
            {
                VerifyDynamicContext(dynamicContext);
                return stringValue switch
                {
                    null => "nullIsPassed",
                    "returnNull" => null,
                    _ => "test"
                };
            }
        );

        RegisterCustomXPathFunction<TNode>.RegisterFunction(
            new QName("custom-date-function", "test"),
            Array.Empty<string>(),
            "xs:date",
            dynamicContext =>
            {
                VerifyDynamicContext(dynamicContext);
                return new DateTimeOffset(2018, 6, 22, 10, 25, 30, TimeSpan.Zero);
            }
        );

        //Added this one to do simple datetime as well.
        RegisterCustomXPathFunction<TNode>.RegisterFunction(
            new QName("custom-date-function-simple", "test"),
            Array.Empty<string>(),
            "xs:date",
            dynamicContext =>
            {
                VerifyDynamicContext(dynamicContext);
                return new DateTime(2018, 6, 22, 10, 25, 30, DateTimeKind.Utc);
            }
        );

        RegisterCustomXPathFunction<TNode>.RegisterFunction(
            new QName("custom-time-function", "test"),
            Array.Empty<string>(),
            "xs:time",
            dynamicContext =>
            {
                VerifyDynamicContext(dynamicContext);
                return new DateTimeOffset(2018, 6, 22, 10, 25, 30, TimeSpan.Zero);
            }
        );

        RegisterCustomXPathFunction<TNode>.RegisterFunction(
            new QName("custom-time-function-simple", "test"),
            Array.Empty<string>(),
            "xs:time",
            dynamicContext =>
            {
                VerifyDynamicContext(dynamicContext);
                return new DateTime(2018, 6, 22, 10, 25, 30, DateTimeKind.Utc);
            }
        );

        RegisterCustomXPathFunction<TNode>.RegisterFunction(
            new QName("custom-datetime-function", "test"),
            Array.Empty<string>(),
            "xs:dateTime",
            dynamicContext =>
            {
                VerifyDynamicContext(dynamicContext);
                return new DateTimeOffset(2018, 6, 22, 10, 25, 30, TimeSpan.Zero);
            }
        );

        RegisterCustomXPathFunction<TNode>.RegisterFunction(
            new QName("custom-datetime-function-simple", "test"),
            Array.Empty<string>(),
            "xs:dateTime",
            dynamicContext =>
            {
                VerifyDynamicContext(dynamicContext);
                return new DateTime(2018, 6, 22, 10, 25, 30, DateTimeKind.Utc);
            }
        );

        RegisterCustomXPathFunction<TNode>.RegisterFunction(
            new QName("custom-gYearMonth-function", "test"),
            Array.Empty<string>(),
            "xs:gYearMonth",
            dynamicContext =>
            {
                VerifyDynamicContext(dynamicContext);
                return new DateTimeOffset(2018, 6, 22, 10, 25, 30, TimeSpan.Zero);
            }
        );

        RegisterCustomXPathFunction<TNode>.RegisterFunction(
            new QName("custom-gYearMonth-function-simple", "test"),
            Array.Empty<string>(),
            "xs:gYearMonth",
            dynamicContext =>
            {
                VerifyDynamicContext(dynamicContext);
                return new DateTime(2018, 6, 22, 10, 25, 30, DateTimeKind.Utc);
            }
        );

        RegisterCustomXPathFunction<TNode>.RegisterFunction(
            new QName("custom-gYear-function", "test"),
            Array.Empty<string>(),
            "xs:gYear",
            dynamicContext =>
            {
                VerifyDynamicContext(dynamicContext);
                return new DateTimeOffset(2018, 6, 22, 10, 25, 30, TimeSpan.Zero);
            }
        );

        RegisterCustomXPathFunction<TNode>.RegisterFunction(
            new QName("custom-gYear-function-simple", "test"),
            Array.Empty<string>(),
            "xs:gYear",
            dynamicContext =>
            {
                VerifyDynamicContext(dynamicContext);
                return new DateTime(2018, 6, 22, 10, 25, 30, DateTimeKind.Utc);
            }
        );

        RegisterCustomXPathFunction<TNode>.RegisterFunction(
            new QName("custom-gMonthDay-function", "test"),
            Array.Empty<string>(),
            "xs:gMonthDay",
            dynamicContext =>
            {
                VerifyDynamicContext(dynamicContext);
                return new DateTimeOffset(2018, 6, 22, 10, 25, 30, TimeSpan.Zero);
            }
        );

        RegisterCustomXPathFunction<TNode>.RegisterFunction(
            new QName("custom-gMonthDay-function-simple", "test"),
            Array.Empty<string>(),
            "xs:gMonthDay",
            dynamicContext =>
            {
                VerifyDynamicContext(dynamicContext);
                return new DateTime(2018, 6, 22, 10, 25, 30, DateTimeKind.Utc);
            }
        );

        RegisterCustomXPathFunction<TNode>.RegisterFunction(
            new QName("custom-gMonth-function", "test"),
            Array.Empty<string>(),
            "xs:gMonth",
            dynamicContext =>
            {
                VerifyDynamicContext(dynamicContext);
                return new DateTimeOffset(2018, 6, 22, 10, 25, 30, TimeSpan.Zero);
            }
        );

        RegisterCustomXPathFunction<TNode>.RegisterFunction(
            new QName("custom-gMonth-function-simple", "test"),
            Array.Empty<string>(),
            "xs:gMonth",
            dynamicContext =>
            {
                VerifyDynamicContext(dynamicContext);
                return new DateTime(2018, 6, 22, 10, 25, 30, DateTimeKind.Utc);
            }
        );

        RegisterCustomXPathFunction<TNode>.RegisterFunction(
            new QName("custom-gDay-function", "test"),
            Array.Empty<string>(),
            "xs:gDay",
            dynamicContext =>
            {
                VerifyDynamicContext(dynamicContext);
                return new DateTimeOffset(2018, 6, 22, 10, 25, 30, TimeSpan.Zero);
            }
        );

        RegisterCustomXPathFunction<TNode>.RegisterFunction(
            new QName("custom-gDay-function-simple", "test"),
            Array.Empty<string>(),
            "xs:gDay",
            dynamicContext =>
            {
                VerifyDynamicContext(dynamicContext);
                return new DateTime(2018, 6, 22, 10, 25, 30, DateTimeKind.Utc);
            }
        );
    }

    [Fact]
    public void BooleanReturnValueTest()
    {
        Assert.True(
            Evaluate.EvaluateXPathToBoolean(
                "test:custom-function1('test')",
                XmlNodeEmptyContext,
                XmlNodeDomFacade,
                XmlNodeOptions
            )
        );
        Assert.False(
            Evaluate.EvaluateXPathToBoolean(
                "test:custom-function1('bla')",
                XmlNodeEmptyContext,
                XmlNodeDomFacade,
                XmlNodeOptions
            )
        );
        Assert.True(
            Evaluate.EvaluateXPathToBoolean(
                "test:custom-function1(())",
                XmlNodeEmptyContext,
                XmlNodeDomFacade,
                XmlNodeOptions
            )
        );
    }

    [Fact]
    public void TwoArgumentFunctionTest()
    {
        Assert.True(Evaluate.EvaluateXPathToBoolean(
            "test:custom-function2('test', true())",
            XmlNodeEmptyContext,
            XmlNodeDomFacade,
            XmlNodeOptions
        ));

        Assert.False(Evaluate.EvaluateXPathToBoolean(
            "test:custom-function2('test', false())",
            XmlNodeEmptyContext,
            XmlNodeDomFacade,
            XmlNodeOptions
        ));
    }

    [Fact]
    public void EvaluateToStringFunctionTest()
    {
        Assert.Equal(
            "test",
            Evaluate.EvaluateXPathToString(
                "test:custom-function3('test')",
                XmlNodeEmptyContext,
                XmlNodeDomFacade,
                XmlNodeOptions)
        );
    }
    
    [Fact]
    public void RegisterFunctionInNamespaceTest()
    {
        const string namespaceUri = "http://www.example.com/customFunctionTest";
        RegisterCustomXPathFunction<XmlNode>.RegisterFunction(
            new QName("test", namespaceUri),
            Array.Empty<string>(),
            "xs:boolean",
            dynamicContext =>
            {
                VerifyDynamicContext(dynamicContext);
                Assert.Equal(123456789, (dynamicContext!.CurrentContext as Dictionary<string, object>)?["nodeId"]);
                return true;
            }
        );

        var optionsWithCtx = new Options<XmlNode>(IdentityNamespaceResolver);
        optionsWithCtx.CurrentContext = new Dictionary<string, object> { { "nodeId", 123456789 } };

        Assert.True(
            Evaluate.EvaluateXPathToBoolean(
                $"Q{{{namespaceUri}}}test()",
                XmlNodeEmptyContext,
                XmlNodeDomFacade,
                optionsWithCtx
            ),
            "Attempt to access the function using the namespace uri"
        );
    }

    [Fact]
    private void DisallowRegisteringDefaultNamespaceTest()
    {
        Exception? exception = null;
        try
        {
            RegisterCustomXPathFunction<XmlNode>.RegisterFunction(
                new QName("custom-function-in-no-ns", ""),
                new string[] { },
                "xs:boolean",
                _ => true);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        Assert.True(
            exception != null, "Do not register custom functions in the default function namespace"
        );
    }

    [Fact]
    private void DisallowAttributesAsParametersTest()
    {
        Exception? exception = null;
        try
        {
            Evaluate.EvaluateXPathToString(
                "test:custom-function3(//@*)",
                XmlNodeEmptyContext,
                XmlNodeDomFacade,
                XmlNodeOptions
            );
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        Assert.True(exception != null, "Cannot pass attribute nodes");
    }

    [Fact]
    private void CustomFunctionReturnValueArrayTest()
    {
        Assert.Equal(
            new[] { "abc-test", "123-test", "XYZ-test" },
            Evaluate.EvaluateXPathToStrings(
                "test:custom-function4(('abc', '123', 'XYZ'))",
                XmlNodeEmptyContext,
                XmlNodeDomFacade,
                XmlNodeOptions
            )
        );

        Assert.Equal(
            "abc-test",
            Evaluate.EvaluateXPathToString(
                "test:custom-function4(('abc'))",
                XmlNodeEmptyContext,
                XmlNodeDomFacade,
                XmlNodeOptions
            )
        );

        Assert.Equal(
            Array.Empty<string>(),
            Evaluate.EvaluateXPathToStrings(
                "test:custom-function4(())",
                XmlNodeEmptyContext,
                XmlNodeDomFacade,
                XmlNodeOptions
            )
        );
    }

    [Fact]
    private void RegisteredFunctionCanReturnNullTest()
    {
        Assert.Null(
            Evaluate.EvaluateXPathToString(
                "test:custom-function5('returnNull')",
                XmlNodeEmptyContext,
                XmlNodeDomFacade,
                XmlNodeOptions
            )
        );
    }

    [Fact]
    private void RegisteredFunctionCanReturnValueTest()
    {
        Assert.Equal(
            "test",
            Evaluate.EvaluateXPathToString(
                "test:custom-function5('abc')",
                XmlNodeEmptyContext,
                XmlNodeDomFacade,
                XmlNodeOptions
            )
        );
    }

    [Fact]
    private void RegisteredFunctionNullValueVariableTest()
    {
        Assert.Equal(
            "nullIsPassed",
            Evaluate.EvaluateXPathToString(
                "test:custom-function5($str)",
                XmlNodeEmptyContext,
                XmlNodeDomFacade,
                XmlNodeOptions,
                new Dictionary<string, object?> { { "str", null } }
            )
        );
    }

    [Fact]
    private void RegisteredFunctionDateTest()
    {
        Assert.Equal(
            "2018-06-22Z",
            Evaluate.EvaluateXPathToString(
                "test:custom-date-function()",
                XmlNodeEmptyContext,
                XmlNodeDomFacade,
                XmlNodeOptions
            )
        );

        Assert.Equal(
            "2018-06-22Z",
            Evaluate.EvaluateXPathToString(
                "test:custom-date-function-simple()",
                XmlNodeEmptyContext,
                XmlNodeDomFacade,
                XmlNodeOptions
            )
        );
    }


    [Fact]
    private void RegisteredFunctionTimeTest()
    {
        Assert.Equal(
            "10:25:30Z",
            Evaluate.EvaluateXPathToString(
                "test:custom-time-function()",
                XmlNodeEmptyContext,
                XmlNodeDomFacade,
                XmlNodeOptions
            )
        );

        Assert.Equal(
            "10:25:30Z",
            Evaluate.EvaluateXPathToString(
                "test:custom-time-function-simple()",
                XmlNodeEmptyContext,
                XmlNodeDomFacade,
                XmlNodeOptions
            )
        );
    }

    [Fact]
    private void RegisteredFunctionDateTimeTest()
    {
        Assert.Equal(
            "2018-06-22T10:25:30Z",
            Evaluate.EvaluateXPathToString(
                "test:custom-datetime-function()",
                XmlNodeEmptyContext,
                XmlNodeDomFacade,
                XmlNodeOptions
            )
        );

        Assert.Equal(
            "2018-06-22T10:25:30Z",
            Evaluate.EvaluateXPathToString(
                "test:custom-datetime-function-simple()",
                XmlNodeEmptyContext,
                XmlNodeDomFacade,
                XmlNodeOptions
            )
        );
    }

    [Fact]
    private void RegisteredFunctionGYearMonthTest()
    {
        Assert.Equal(
            "2018-06Z",
            Evaluate.EvaluateXPathToString(
                "test:custom-gYearMonth-function()",
                XmlNodeEmptyContext,
                XmlNodeDomFacade,
                XmlNodeOptions
            )
        );

        Assert.Equal(
            "2018-06Z",
            Evaluate.EvaluateXPathToString(
                "test:custom-gYearMonth-function-simple()",
                XmlNodeEmptyContext,
                XmlNodeDomFacade,
                XmlNodeOptions
            )
        );
    }

    [Fact]
    private void RegisteredFunctionGYearTest()
    {
        Assert.Equal(
            "2018Z",
            Evaluate.EvaluateXPathToString(
                "test:custom-gYear-function()",
                XmlNodeEmptyContext,
                XmlNodeDomFacade,
                XmlNodeOptions
            )
        );

        Assert.Equal(
            "2018Z",
            Evaluate.EvaluateXPathToString(
                "test:custom-gYear-function-simple()",
                XmlNodeEmptyContext,
                XmlNodeDomFacade,
                XmlNodeOptions
            )
        );
    }

    [Fact]
    private void RegisteredFunctionGMonthDayTest()
    {
        Assert.Equal(
            "--06-22Z",
            Evaluate.EvaluateXPathToString(
                "test:custom-gMonthDay-function()",
                XmlNodeEmptyContext,
                XmlNodeDomFacade,
                XmlNodeOptions
            )
        );

        Assert.Equal(
            "--06-22Z",
            Evaluate.EvaluateXPathToString(
                "test:custom-gMonthDay-function-simple()",
                XmlNodeEmptyContext,
                XmlNodeDomFacade,
                XmlNodeOptions
            )
        );
    }

    [Fact]
    private void RegisteredFunctionGMonthTest()
    {
        Assert.Equal(
            "--06Z",
            Evaluate.EvaluateXPathToString(
                "test:custom-gMonth-function()",
                XmlNodeEmptyContext,
                XmlNodeDomFacade,
                XmlNodeOptions
            )
        );

        Assert.Equal(
            "--06Z",
            Evaluate.EvaluateXPathToString(
                "test:custom-gMonth-function-simple()",
                XmlNodeEmptyContext,
                XmlNodeDomFacade,
                XmlNodeOptions
            )
        );
    }

    [Fact]
    private void RegisteredFunctionGDayTest()
    {
        Assert.Equal(
            "---22Z",
            Evaluate.EvaluateXPathToString(
                "test:custom-gDay-function()",
                XmlNodeEmptyContext,
                XmlNodeDomFacade,
                XmlNodeOptions
            )
        );

        Assert.Equal(
            "---22Z",
            Evaluate.EvaluateXPathToString(
                "test:custom-gDay-function-simple()",
                XmlNodeEmptyContext,
                XmlNodeDomFacade,
                XmlNodeOptions
            )
        );
    }

    [Fact]
    private void KeepsDomFacadeIntactTest()
    {
        var outerDomFacade = new OuterDomFacade();
        RegisterCustomXPathFunction<XmlNode>.RegisterFunction(
            new QName("custom-function-keeps-the-dom-facade", "test"),
            Array.Empty<string>(),
            "xs:boolean",
            dynamicContext =>
            {
                Assert.Equal(outerDomFacade, dynamicContext?.DomFacade);
                return dynamicContext?.DomFacade is OuterDomFacade outer && OuterDomFacade.ThisIsTheOuterOne;
            }
        );
        Assert.True(
            Evaluate.EvaluateXPathToBoolean(
                "test:custom-function-keeps-the-dom-facade()",
                XmlNodeEmptyContext,
                outerDomFacade,
                XmlNodeOptions
            )
        );
    }

    [Fact]
    private void ThrowsWhenRegisteringWithUriTest()
    {
        XPathException? xqst0060 = null;
        try
        {
            RegisterCustomXPathFunction<XmlNode>.RegisterFunction(
                new QName("empty-uri", ""),
                Array.Empty<string>(),
                "xs:boolean",
                _ => true
            );
        }
        catch (XPathException ex)
        {
            xqst0060 = ex;
        }

        Assert.True(xqst0060 != null && xqst0060.ErrorCode == "XQST0060");

        xqst0060 = null;

        try
        {
            RegisterCustomXPathFunction<XmlNode>.RegisterFunction(
                new QName("empty-uri"),
                Array.Empty<string>(),
                "xs:boolean",
                _ => true
            );
        }
        catch (XPathException ex)
        {
            xqst0060 = ex;
        }

        Assert.True(xqst0060 != null && xqst0060.ErrorCode == "XQST0060");
    }

    [Fact(Skip = "The functionality for this test does not really exist yet I think.")]
    private void GetNodeWithoutWrappingPointersTest()
    {
    }

    private static void RegisterSimpleFunctions<TNode>() where TNode : notnull
    {
        RegisterCustomXPathFunction<TNode>.RegisterFunction<DateTimeOffset, DateTimeOffset>(
            new QName("custom-date-function-param", "test"),
            new[] { "xs:date" },
            "xs:date",
            (_, date) =>
            {
                Assert.True(date is DateTimeOffset, "Parameter is not of type DateTimeOffset");
                return date;
            }
        );

        RegisterCustomXPathFunction<TNode>.RegisterFunction<DateTimeOffset?, DateTimeOffset?>(
            new QName("custom-date-function-optional-param", "test"),
            new[] { "xs:date?" },
            "xs:date?",
            (_, date) =>
            {
                Assert.True(date is null or DateTimeOffset, "Parameter is not null or of type DateTimeOffset");
                return date;
            }
        );

        RegisterCustomXPathFunction<TNode>.RegisterFunction<DateTimeOffset[], DateTimeOffset[]>(
            new QName("custom-date-function-zero-to-many-param", "test"),
            new[] { "xs:date*" },
            "xs:date*",
            (_, dates) =>
            {
                Assert.True(dates is Array, "Parameter is not an array.");

                Assert.True(dates.All(d => d is DateTimeOffset), "Parameter is not of type DateTimeOffset");

                return dates;
            }
        );

        // There were more tests of a similar nature, but they all do the same, which is a waste in a strongly typed language.
    }

    [Fact]
    private void PassesDateTimeOffsetTest()
    {
        Assert.NotNull(
            Evaluate.EvaluateXPathToString(
                "test:custom-date-function-param(xs:date('2019-08-29'))",
                XmlNodeEmptyContext,
                XmlNodeDomFacade,
                XmlNodeOptions
            ));
    }

    [Fact]
    private void PassesDateTimeOffsetOptionalTest()
    {
        Assert.NotNull(
            Evaluate.EvaluateXPathToString(
                "test:custom-date-function-optional-param(xs:date('2019-08-29'))",
                XmlNodeEmptyContext,
                XmlNodeDomFacade,
                XmlNodeOptions
            ));

        Assert.Null(
            Evaluate.EvaluateXPathToString(
                "test:custom-date-function-optional-param(())",
                XmlNodeEmptyContext,
                XmlNodeDomFacade,
                XmlNodeOptions
            ));
    }

    [Fact]
    private void PassesDateTimeOffsetZeroManyTest()
    {
        Assert.NotNull(
            Evaluate.EvaluateXPathToString(
                "test:custom-date-function-zero-to-many-param((xs:date('2019-08-29'), xs:date('2019-08-31')))",
                XmlNodeEmptyContext,
                XmlNodeDomFacade,
                XmlNodeOptions
            ));
    }

    [Fact]
    private void NoUnsupportedTypesTest()
    {
        XPathException? err = null;
        try
        {
            RegisterCustomXPathFunction<XmlNode>.RegisterFunction(
                new QName("func", "a-random-prefix-to-prevent-collisions"),
                Array.Empty<string>(),
                "this-type::does-not-exist",
                _ => true
            );
        }
        catch (XPathException ex)
        {
            err = ex;
        }

        Assert.True(err != null && err.ErrorCode == "XPST0081");
    }

    private class OuterDomFacade : XmlNodeDomFacade
    {
        public const bool ThisIsTheOuterOne = true;
    }
}