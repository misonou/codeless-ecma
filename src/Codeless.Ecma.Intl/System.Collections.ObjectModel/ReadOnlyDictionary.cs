#if NET35 || NET40
using Codeless.Ecma;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections.ObjectModel {
  public class ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue> {
    readonly IDictionary<TKey, TValue> dictionary;

    public ReadOnlyDictionary(IDictionary<TKey, TValue> dictionary) {
      Guard.ArgumentNotNull(dictionary, "dictionary");
      this.dictionary = dictionary;
    }

    public bool ContainsKey(TKey key) {
      return dictionary.ContainsKey(key);
    }

    public ICollection<TKey> Keys {
      get { return dictionary.Keys; }
    }

    public ICollection<TValue> Values {
      get { return dictionary.Values; }
    }

    public int Count {
      get { return dictionary.Count; }
    }

    public bool TryGetValue(TKey key, out TValue value) {
      return dictionary.TryGetValue(key, out value);
    }

    public TValue this[TKey key] {
      get { return dictionary[key]; }
    }

    public bool Contains(KeyValuePair<TKey, TValue> item) {
      return dictionary.Contains(item);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
      dictionary.CopyTo(array, arrayIndex);
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
      return dictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return dictionary.GetEnumerator();
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly {
      get { return true; }
    }

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys {
      get { return dictionary.Keys; }
    }

    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values {
      get { return dictionary.Values; }
    }

    TValue IDictionary<TKey, TValue>.this[TKey key] {
      get { return this[key]; }
      set { throw new InvalidOperationException("Dictionary is read-only"); }
    }

    void IDictionary<TKey, TValue>.Add(TKey key, TValue value) {
      throw new InvalidOperationException("Dictionary is read-only");
    }

    bool IDictionary<TKey, TValue>.Remove(TKey key) {
      throw new InvalidOperationException("Dictionary is read-only");
    }

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) {
      throw new InvalidOperationException("Dictionary is read-only");
    }

    void ICollection<KeyValuePair<TKey, TValue>>.Clear() {
      throw new InvalidOperationException("Dictionary is read-only");
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) {
      throw new InvalidOperationException("Dictionary is read-only");
    }
  }
}
#endif
