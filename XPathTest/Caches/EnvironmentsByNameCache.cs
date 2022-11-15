using System;
using System.Collections.Generic;
using System.Linq;
using FontoXPathCSharp;
using FontoXPathCSharp.DomFacade;
using FontoXPathCSharp.Types;
using XPathTest.Qt3Tests;

namespace XPathTest.Caches;

public class EnvironmentsByNameCache<TNode> : ResourceCache<string, Qt3TestEnvironment<TNode>> where TNode : notnull
{
    public static EnvironmentsByNameCache<TNode> Instance { get; } = new();

    public override Qt3TestEnvironment<TNode>? GetResource(string? key)
    {
        return key == null ? _cache["empty"] : base.GetResource(key);
    }

    protected override Qt3TestEnvironment<TNode> Load(string key)
    {
        // If the environment was not added in during initial loading, it is not part of the cache.
        Console.WriteLine(
            $"Environment '{key}' was not found in environment cache, loading empty environment instead.");
        return _cache["empty"];
    }

    public void LoadDefaultEnvironments(TNode catalog, IDomFacade<TNode> domFacade, INodeUtils<TNode> nodeUtils,
        Options<TNode> catalogOptions)
    {
        Console.WriteLine("Initializing default environment cache.");

        _cache = Evaluate.EvaluateXPathToNodes("/catalog/environment", catalog, domFacade, catalogOptions).Aggregate(
            new Dictionary<string, Qt3TestEnvironment<TNode>>(),
            (envByName, environmentNode) =>
            {
                envByName[Evaluate.EvaluateXPathToString("@name", environmentNode, domFacade, catalogOptions)!]
                    = new Qt3TestEnvironment<TNode>("", environmentNode, domFacade, nodeUtils, catalogOptions);
                return envByName;
            }
        );

        Console.WriteLine($"Finished loading default environments, loaded {_cache.Count} environments.");
    }
}