#if NET35 || NET40
using System;
using System.Collections;
using System.Reflection;

namespace System.Collections.Generic {
  /// <summary>
  /// Read-only collection of key/value pairs.
  /// </summary>
  /// <typeparam name="TKey">Key type.</typeparam>
  /// <typeparam name="TValue">Value type.</typeparam>
  public interface IReadOnlyDictionary<TKey, TValue> : IReadOnlyCollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable {
    /// <summary>
    /// Gets the element that has the specified key in the dictionary.
    /// </summary>
    /// <param name="key">Key to locate.</param>
    /// <returns>Element with the specified key in the dictionary.</returns>
    TValue this[TKey key] { get; }

    /// <summary>
    /// Keys in the dictionary.
    /// </summary>
    IEnumerable<TKey> Keys { get; }

    /// <summary>
    /// Values in the dictionary.
    /// </summary>
    IEnumerable<TValue> Values { get; }

    /// <summary>
    /// Determines whether the dictionary contains an element with the specified key.
    /// </summary>
    /// <param name="key">Key to locate.</param>
    /// <returns>true if the dictionary contains the specified key; otherwise false.</returns>
    bool ContainsKey(TKey key);

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">Key to locate.</param>
    /// <param name="value">Value located, if successful.</param>
    /// <returns>true if the key was found; otherwise false.</returns>
    bool TryGetValue(TKey key, out TValue value);
  }
}
#endif
