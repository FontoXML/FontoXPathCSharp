using System;
using System.Collections.Generic;
using System.Xml;
using FontoXPathCSharp.Value;

namespace XPathTest.Caches;

public class EnvironmentsByNameCache : ResourceCache<string, Qt3TestUtils.Environment>
{
    public static EnvironmentsByNameCache Instance { get; } = new();

    protected override Qt3TestUtils.Environment? Load(string key)
    {
        return new Qt3TestUtils.Environment(new XmlDocument(), s => null, new Dictionary<string, AbstractValue?>());
        throw new NotImplementedException("Loading Environments from a Cache is not implemented yet.");
    }

    public void LoadDefaultEnvironments(XmlNode catalog)
    {
        throw new NotImplementedException("Loading Default Environments not implemented yet.");
        // TODO: Implementing default environments loading.
        // var result = Evaluate.EvaluateXPathToNodes("/catalog/environment",
        //         catalog,
        //         null,
        //         new Dictionary<string, AbstractValue>(),
        //         new Options())
        //     .Aggregate(
        //         new Dictionary<string, XmlNode> {["empty"] = new XmlDocument()},
        //         (envByName, environmentNode) =>
        //         {
        //             var name = Evaluate.EvaluateXPathToString("@name", environmentNode, null,
        //                 new Dictionary<string, AbstractValue>(),
        //                 new Options());
        //
        //             envByName[name] = CreateEnvironment("", environmentNode);
        //             return envByName;
        //         });
    }
}