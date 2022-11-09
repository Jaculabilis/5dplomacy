namespace System.Collections.Generic;

public static class AdjudicationDictionaryExtensions
{
    /// <summary>
    /// Create and add a value to a dictionary only if the key is not already present.
    /// </summary>
    /// <param name="dictionary">The dictionary to check for the key.</param>
    /// <param name="key">The key to check and use if it isn't already present.</param>
    /// <param name="valueFunc">A function that returns the value to insert if the key is not present.</param>
    public static void Ensure<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        TKey key,
        Func<TValue> valueFunc)
    {
        if (!dictionary.ContainsKey(key))
        {
            dictionary[key] = valueFunc();
        }
    }
}