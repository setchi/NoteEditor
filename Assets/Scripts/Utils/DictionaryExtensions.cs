using System.Collections.Generic;

public static class DictionaryExtensions
{
    public static void Set<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, TValue value)
    {
        if (source.ContainsKey(key))
        {
            source[key] = value;
        }
        else
        {
            source.Add(key, value);
        }
    }

    public static void Set<TKey, TValue>(this IDictionary<TKey, TValue> source, KeyValuePair<TKey, TValue> keyValue)
    {
        source.Set(keyValue.Key, keyValue.Value);
    }
}
