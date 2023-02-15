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

        // RegisterFunctions<XmlNode>();
    }

    private static void RegisterFunctions<TNode>() where TNode : notnull
    {
        void VerifyDynamicContext(DynamicContextAdapter<TNode>? dynamicContext)
        {
            Assert.True(dynamicContext != null, "A dynamic context has not been passed");
            Assert.True(dynamicContext?.DomFacade != null, "A domFacade has not been passed");
        }

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
        RegisterFunctions<XmlNode>();

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
}