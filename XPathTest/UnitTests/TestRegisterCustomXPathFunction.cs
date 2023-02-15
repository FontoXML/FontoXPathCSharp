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
        RegisterCustomXPathFunction<TNode>.RegisterFunction<string?, bool>(
            new QName("custom-function1", "test"),
            new[] { "xs:string?" },
            "xs:boolean",
            (dynamicContext, arg1) =>
            {
                Assert.True(dynamicContext != null, "A dynamic context has not been passed");
                Assert.True(dynamicContext?.DomFacade != null, "A domFacade has not been passed");
                return arg1 is null or "test";
            }
        );

        RegisterCustomXPathFunction<TNode>.RegisterFunction<string, bool, bool>(
            new QName("custom-function2", "test"),
            new[] { "xs:string", "xs:boolean" },
            "xs:boolean",
            (dynamicContext, stringArg, boolArg) =>
            {
                Assert.True(dynamicContext != null, "A dynamic context has not been passed");
                Assert.True(dynamicContext?.DomFacade != null, "A domFacade has not been passed");
                return stringArg == "test" && boolArg;
            }
        );

        // RegisterCustomXPathFunction<TNode>.RegisterFunction(
        // 	"test:custom-function3",
        // 	new []{"item()*"},
        // 	"item()",
        // 	(dynamicContext, args) => { 
        // 		Assert.True(dynamicContext != null, "A dynamic context has not been passed");
        // 		Assert.True(dynamicContext?.DomFacade != null, "A domFacade has not been passed");
        //
        // 		return args[0];
        // 	}
        // );
        //
        // registerCustomXPathFunction(
        // 	{ namespaceURI: 'test', localName: 'custom-function4' },
        // 	['xs:string*'],
        // 	'xs:string*',
        // 	(dynamicContext, stringArray) => {
        // 		chai.assert.isOk(dynamicContext, 'A dynamic context has not been passed');
        // 		chai.assert.isOk(dynamicContext.domFacade, 'A domFacade has not been passed');
        //
        // 		return stringArray.map((stringValue) => {
        // 			return stringValue + '-test';
        // 		});
        // 	}
        // );
        //
        // registerCustomXPathFunction(
        // 	{ namespaceURI: 'test', localName: 'custom-function5' },
        // 	['xs:string?'],
        // 	'xs:string?',
        // 	(dynamicContext, stringValue) => {
        // 		chai.assert.isOk(dynamicContext, 'A dynamic context has not been passed');
        // 		chai.assert.isOk(dynamicContext.domFacade, 'A domFacade has not been passed');
        //
        // 		if (stringValue === 'returnNull') {
        // 			return null;
        // 		} else if (stringValue === null) {
        // 			return 'nullIsPassed';
        // 		} else {
        // 			return stringValueFactory('test', dynamicContext.domFacade);
        // 		}
        // 	}
        // );    
    }

    [Fact]
    public void TwoArgumentFunction()
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