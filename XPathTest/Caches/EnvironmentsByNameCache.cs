using System;
using System.Collections.Generic;
using System.Xml;
using FontoXPathCSharp.Value;

namespace XPathTest.Caches;

public class EnvironmentsByNameCache<TNode> : ResourceCache<string, TestingUtils.Environment<TNode>>
{
    private NodeUtils<TNode> _nodeUtils;
    public static EnvironmentsByNameCache<TNode> Instance { get; } = new();

    protected override TestingUtils.Environment<TNode>? Load(string key)
    {
        return new TestingUtils.Environment<TNode>(_nodeUtils.CreateDocument(), s => null, new Dictionary<string, object>());
        throw new NotImplementedException("Loading Environments from a Cache is not implemented yet.");
    }

    public void LoadDefaultEnvironments(TNode catalog, NodeUtils<TNode> nodeUtils)
    {
        _nodeUtils = nodeUtils;
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