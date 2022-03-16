using Xunit;
using System.Xml;

namespace XPathTest.spec.expressions;

[Trait("Category", "Absolute Paths")]
public class AbsolutePathExpression_tests
{
    private XmlDocument documentNode;

    private void beforeEach()
    {
        documentNode = new XmlDocument();
    }
    
    [Fact]
    [Trait("Description", "Supports Absolute Paths")]
    public void SupportsAbsolutePaths()
    {
        beforeEach();
        documentNode.CreateElement("someNode");
        Equals(evaluateXPathToNodes("/someNode", documentNode), new Node[] {});
    }
// it('supports absolute paths', () => {
//     jsonMlMapper.parse(['someNode'], documentNode);
//     chai.assert.deepEqual(evaluateXPathToNodes('/someNode', documentNode), [
//         documentNode.documentElement,
//     ]);
// });

// it('supports chaining from absolute paths', () => {
//     jsonMlMapper.parse(['someNode', ['someChildNode']], documentNode);
//     chai.assert.deepEqual(evaluateXPathToNodes('/someNode/someChildNode', documentNode), [
//         documentNode.documentElement.firstChild,
//     ]);
// });
//
// it('allows (/)', () => {
//     jsonMlMapper.parse(['someNode', ['someChildNode']], documentNode);
//     chai.assert.deepEqual(evaluateXPathToNodes('(/)', documentNode), [documentNode]);
// });
//
// it('disallows / * 5', () => {
//     jsonMlMapper.parse(['someNode', ['someChildNode']], documentNode);
//     chai.assert.throws(() => evaluateXPathToNodes('/ * 5', documentNode), 'XPST0003');
// });
//
// it('disallows / union /', () => {
//     jsonMlMapper.parse(['someNode', ['someChildNode']], documentNode);
//     chai.assert.throws(() => evaluateXPathToNodes('/ union /', documentNode), 'XPST0003');
// });
//
// it('allows / union', () => {
//     jsonMlMapper.parse(['union'], documentNode);
//     chai.assert.deepEqual(evaluateXPathToNodes('/ union', documentNode), [
//         documentNode.documentElement,
//     ]);
// });
//
// it('allows // as root', () => {
//     jsonMlMapper.parse(['someNode', ['someChildNode']], documentNode);
//     chai.assert.deepEqual(evaluateXPathToNodes('//someChildNode', documentNode), [
//         documentNode.documentElement.firstChild,
//     ]);
// });
//
// it('targets descendants with //', () => {
//     jsonMlMapper.parse(['someNode', ['someChildNode', ['someDescendantNode']]], documentNode);
//     chai.assert.deepEqual(evaluateXPathToNodes('//someDescendantNode', documentNode), [
//         documentNode.documentElement.firstChild.firstChild,
//     ]);
// });
}