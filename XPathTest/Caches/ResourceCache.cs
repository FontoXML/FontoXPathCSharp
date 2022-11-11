using System.Collections.Generic;

namespace XPathTest.Caches;

public abstract class ResourceCache<TKey, TRes> where TKey : notnull
{
    protected Dictionary<TKey, TRes> _cache = new();

    protected abstract TRes? Load(TKey key);

    public TRes? GetResource(TKey key)
    {
        if (_cache.ContainsKey(key)) return _cache[key];
        var loaded = Load(key);
        if (loaded != null) _cache[key] = loaded;
        return loaded;
    }

    public TRes InsertResource(TKey key, TRes res)
    {
        _cache[key] = res;
        return res;
    }
}