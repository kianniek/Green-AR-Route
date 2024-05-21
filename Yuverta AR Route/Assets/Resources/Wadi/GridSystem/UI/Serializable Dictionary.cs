using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver
{
    [SerializeField]
    public List<TKey> keys = new List<TKey>();

    [SerializeField]
    public List<TValue> values = new List<TValue>();

    private Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

    // Add a new key-value pair to the dictionary
    public void Add(TKey key, TValue value)
    {
        if (!dictionary.ContainsKey(key))
        {
            keys.Add(key);
            values.Add(value);
            dictionary.Add(key, value);
        }
        else
        {
            Debug.LogWarning($"Key '{key}' already exists in the dictionary.");
        }
    }

    public void Remove(TKey key)
    {
        if (dictionary.ContainsKey(key))
        {
            int index = keys.IndexOf(key);
            keys.RemoveAt(index);
            values.RemoveAt(index);
            dictionary.Remove(key);
        }
        else
        {
            Debug.LogWarning($"Key '{key}' does not exist in the dictionary.");
        }
    }

    public void Clear()
    {
        dictionary.Clear();
        keys.Clear();
        values.Clear();
    }

    // Retrieve the value associated with the specified key
    public TValue GetValue(TKey key)
    {
        return dictionary[key];
    }

    // Implementation of ISerializationCallbackReceiver interface
    public void OnBeforeSerialize()
    {
        // Nothing needs to be done here
    }

    public void OnAfterDeserialize()
    {
        dictionary = new Dictionary<TKey, TValue>();

        for (int i = 0; i < keys.Count; i++)
        {
            dictionary.Add(keys[i], values[i]);
        }
    }
}