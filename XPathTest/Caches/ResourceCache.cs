using System.Collections.Generic;

namespace XPathTest.Caches;

public abstract class ResourceCache<TKey, TRes> where TKey : notnull
{
    protected Dictionary<TKey, TRes> Cache = new();

    protected abstract TRes? Load(TKey key);

    public virtual TRes? GetResource(TKey key)
    {
        if (Cache.ContainsKey(key)) return Cache[key];
        var loaded = Load(key);
        if (loaded != null) Cache[key] = loaded;
        return loaded;
    }

    public TRes InsertResource(TKey key, TRes res)
    {
        Cache[key] = res;
        return res;
    }
}