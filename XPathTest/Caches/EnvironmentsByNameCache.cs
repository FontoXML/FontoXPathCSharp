using System.Collections.Generic;
using System.Linq;
using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;

namespace XPathTest.Caches;

public class EnvironmentsByNameCache : ResourceCache<string, XmlNode>
{
    protected override XmlNode? Load(string key)
    {
        return null;
    }

    public void LoadDefaultEnvironments(XmlNode catalog)
    {
        var result = Evaluate.EvaluateXPathToNodes("/catalog/environment",
                catalog,
                null,
                new Dictionary<string, AbstractValue>(),
                new Options())
            .Aggregate(
                new Dictionary<string, XmlNode> {["empty"] = new XmlDocument()},
                (envByName, environmentNode) =>
                {
                    var name = Evaluate.EvaluateXPathToString("@name", environmentNode, null,
                        new Dictionary<string, AbstractValue>(),
                        new Options());

                    // envByName[name] = CreateEnvironment("", environmentNode);
                    return envByName;
                });
    }
}