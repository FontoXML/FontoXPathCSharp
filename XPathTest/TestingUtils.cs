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

    /**
     * Takes a set of key-value pairs, such as a dictionary, and writes it to disk in csv format,
     * delimited with commas for columns and newlines for rows.
     */
    public static void WriteKvpCollectionToDisk<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> dict,
        string fileName)
    {
        TestFileSystem.WriteFile(
            fileName,
			string.Join(
				Environment.NewLine,
				dict.Select(d => $"{d.Key},{d.Value}")
            ),
			true);
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

    public static string? LoadFileToString(string filename)
    {
        return DocumentsByPathCache.Instance.GetResource($"qt3tests/{PreprocessFilename(filename)}");
    }
}
