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

        RegisterFunctions<XmlNode>();
    }

    private static void VerifyDynamicContext<TNode>(DynamicContextAdapter<TNode>? dynamicContext) where TNode : notnull
    {
        Assert.True(dynamicContext != null, "A dynamic context has not been passed");
        Assert.True(dynamicContext?.DomFacade != null, "A domFacade has not been passed");
    }

    private static void RegisterFunctions<TNode>() where TNode : notnull
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
                return new DateTime(2018, 6, 22,10, 25, 30, DateTimeKind.Utc);
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
                return new DateTime(2018, 6, 22,10, 25, 30, DateTimeKind.Utc);
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
            new QName("test", "http://www.example.com/customFunctionTest"),
            Array.Empty<string>(),
            "xs:boolean*",
            dynamicContext =>
            {
                VerifyDynamicContext(dynamicContext);
                Assert.Equal(123456789, dynamicContext!.CurrentContext);
                return true;
            }
        );

        var variables = new Dictionary<string, object> { { "nodeId", 123456789 } };

        Assert.True(
            Evaluate.EvaluateXPathToBoolean(
                $"Q{{{namespaceUri}}}test()",
                XmlNodeEmptyContext,
                XmlNodeDomFacade,
                XmlNodeOptions,
                variables
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
            new[] { "abc", "123", "XYZ" },
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
                new Dictionary<string, object?> {{"str", null}}
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
}