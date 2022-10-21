using System;
using System.Collections.Generic;
using System.Linq;
using XPathTest.Caches;

namespace XPathTest;

public class TestingUtils
{
    /**
     * Takes in an container of entries and creates a list that contains the occurences per entry that is sorted descending by occurence.
     */
    public static List<KeyValuePair<T, int>> GetSortedValueOccurrences<T>(IEnumerable<T> list) where T : notnull
    {
        var occurenceList = list.Aggregate(
            new Dictionary<T, int>(),
            (acc, val) =>
            {
                if (!acc.ContainsKey(val)) acc[val] = 0;
                acc[val]++;
                return acc;
            }
        ).ToList();

        occurenceList.Sort((a, b) => a.Value.CompareTo(b.Value));
        occurenceList.Reverse();

        return occurenceList;
    }

    public static void WriteOccurenceToCsv<T>(
        IEnumerable<T> list,
        string fileName,
        char delimiter = ',',
        bool append = false) where T : notnull
    {
        var occurences = GetSortedValueOccurrences(list);
        TestFileSystem.WriteFile(
            fileName,
            string.Join(
                Environment.NewLine,
                occurences.Select(d => $"{d.Key}{delimiter}{d.Value}")
            ),
            append);
    }

    /**
     * Takes a set of key-value pairs, such as a dictionary, and writes it to disk in csv format,
     * delimited with commas for columns and newlines for rows.
     */
    public static void WriteDictionaryToCsv<TKey, TValue>(
        IDictionary<TKey, TValue> dict,
        string fileName,
        char delimiter = ',',
        bool append = false) where TKey : IComparable<TKey>
    {
        // Sorting keys to avoid churn when entries are added/removed.
        // var keyVals = dict.ToList();
        // keyVals.Sort();

        TestFileSystem.WriteFile(
            fileName,
            string.Join(
                Environment.NewLine,
                dict.Select(d => $"{d.Key}{delimiter}{d.Value}")
            ),
            append);
    }

    public static IDictionary<TKey, TValue> ReadDictionaryFromCsv<TKey, TValue>(
        string fileName,
        Func<string, TKey> toKey,
        Func<string, TValue> toVal,
        char delimiter = ',') where TKey : notnull
    {
        if (!TestFileSystem.FileExists(fileName)) return new Dictionary<TKey, TValue>();

        return TestFileSystem.ReadFile(fileName)
            .Split(Environment.NewLine)
            .AsParallel()
            .Aggregate(new Dictionary<TKey, TValue>(), (acc, line) =>
            {
                var cols = line.Split(delimiter);
                if (cols.Length > 1) acc.TryAdd(toKey(cols[0]), toVal(cols[1]));
                return acc;
            });
    }

    public static void UpdateDictionaryCsv<TKey, TValue>(
        IDictionary<TKey, TValue> dict,
        string fileName,
        Func<string, TKey> toKey,
        Func<string, TValue> toVal,
        char delimiter = ',') where TKey : IComparable<TKey>
    {
        var values = ReadDictionaryFromCsv(fileName, toKey, toVal, delimiter);
        foreach (var entry in dict) values[entry.Key] = entry.Value;
        WriteDictionaryToCsv(values, fileName, delimiter);
    }

    /**
     * Safely turns a list of key-value pairs into a dictionary by making sure there are no duplicate keys.
     */
    public static Dictionary<TKey, TValue> ToDictionarySafe<TKey, TValue>(
        IEnumerable<KeyValuePair<TKey, TValue>> enumerable) where TKey : notnull
    {
        return enumerable
            .DistinctBy(x => x.Key)
            .ToDictionary(x => x.Key, x => x.Value);
    }

    public static string PreprocessFilename(string filename)
    {
        while (filename.Contains(".."))
        {
            var parts = filename.Split('/');

            filename = string.Join('/', parts
                .Take(Array.IndexOf(parts, ".."))
                .Concat(parts.Skip(Array.IndexOf(parts, "..") + 1)));
        }

        return filename;
    }

    public static string? LoadQt3TestFileToString(string filename)
    {
        return DocumentsByPathCache.Instance.GetResource($"qt3tests/{PreprocessFilename(filename)}");
    }
}