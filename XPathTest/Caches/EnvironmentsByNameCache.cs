using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using FontoXPathCSharp;
using FontoXPathCSharp.Types;
using FontoXPathCSharp.Value;
using Environment = XPathTest.Qt3TestUtils.Environment;


namespace XPathTest.Caches;

public class EnvironmentsByNameCache : ResourceCache<string, Environment>
{
    public static EnvironmentsByNameCache Instance { get; } = new();
    
    protected override Environment? Load(string key)
    {
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