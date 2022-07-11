using System;
using System.Collections.Generic;
using System.Linq;

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
        TestFileSystem.WriteFile(fileName, string.Join(
            Environment.NewLine,
            dict.Select(d => $"{d.Key},{d.Value}")
        ));
    }
}