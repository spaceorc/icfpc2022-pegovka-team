using System;
using System.Collections.Generic;

namespace lib;

public static class DictionaryExtensions
{
    public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        if (!dictionary.ContainsKey(key))
            dictionary.Add(key, value);
        else
            dictionary[key] = value;
    }

    public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value, Func<TValue, TValue> update)
    {
        if (dictionary.TryGetValue(key, out var currentValue))
            dictionary[key] = update(currentValue);
        else
            dictionary[key] = value;
    }

    public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value, Action<TValue> update)
    {
        if (dictionary.TryGetValue(key, out var currentValue))
            update(currentValue);
        else
            dictionary[key] = value;
    }
}
