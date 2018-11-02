#if NET35 || NET40
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Collections.Generic {
  /// <summary>
  /// Represents a generic read-only collection of key/value pairs.
  /// </summary>
  /// <typeparam name="TKey">The type of keys in the read-only dictionary.</typeparam>
  /// <typeparam name="TValue">The type of values in the read-only dictionary.</typeparam>
  public interface IReadOnlyDictionary<TKey, TValue> : IEnumerable, IEnumerable<KeyValuePair<TKey, TValue>> {
    /// <summary>
    /// Determines whether the read-only dictionary contains an element that has the specified key.
    /// </summary>
    /// <param name="key">The key to locate.</param>
    /// <returns>*true* if the read-only dictionary contains an element that has the specified key; otherwise, *false*.</returns>
    bool ContainsKey(TKey key);

    /// <summary>
    /// Gets the value that is associated with the specified key.
    /// </summary>
    /// <param name="key">The key to locate.</param>
    /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
    /// <returns>*true* if the object that implements the <see cref="IReadOnlyDictionary{TKey,TValue}"/> interface contains an element that has the specified key; otherwise, *false*.</returns>
    bool TryGetValue(TKey key, out TValue value);

    /// <summary>
    /// Gets an enumerable collection that contains the keys in the read-only dictionary.
    /// </summary>
    /// <remarks>
    /// The order of the keys in the enumerable collection is unspecified, but the implementation must guarantee that the keys 
    /// are in the same order as the corresponding values in the enumerable collection that is returned by the <see cref="Values"/> property.
    /// </remarks>
    IEnumerable<TKey> Keys { get; }

    /// <summary>
    /// Gets the element that has the specified key in the read-only dictionary.
    /// </summary>
    /// <param name="key">The key to locate.</param>
    /// <returns>The element that has the specified key in the read-only dictionary.</returns>
    TValue this[TKey key] { get; }

    /// <summary>
    /// Gets an enumerable collection that contains the values in the read-only dictionary.
    /// </summary>
    /// <remarks>
    /// The order of the values in the enumerable collection is unspecified, but the implementation must guarantee that the values 
    /// are in the same order as the corresponding keys in the enumerable collection that is returned by the <see cref="Keys"/> property.
    /// </remarks>
    IEnumerable<TValue> Values { get; }
  }
}
#endif