using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Codeless.Ecma.Intl.Utilities {
  [DebuggerDisplay("Count = {Count}")]
  internal class BcpSubtagDictionary : IDictionary<string, string>, IReadOnlyDictionary<string, string> {
    private readonly IDictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public BcpSubtagDictionary() { }

    public BcpSubtagDictionary(IDictionary<string, string> dictionary, bool isReadOnly) {
      Guard.ArgumentNotNull(dictionary, "dictionary");
      foreach (KeyValuePair<string, string> e in dictionary) {
        this.dictionary[e.Key] = e.Value;
      }
      this.IsReadOnly = isReadOnly;
    }

    public string this[string key] {
      get {
        return dictionary.TryGetValue(key, out string current) ? current : null;
      }
      set {
        ThrowIfReadOnly();
        if (value == null) {
          Remove(key);
        } else if (Validate(key, value)) {
          if (!dictionary.TryGetValue(key, out string current) || current != value) {
            dictionary[key] = value;
            OnChange(new[] { key });
          }
        }
      }
    }

    public ICollection<string> Keys => dictionary.Keys;

    public ICollection<string> Values => dictionary.Values;

    public int Count => dictionary.Count;

    public bool IsReadOnly { get; }

    public void Add(string key, string value) {
      ThrowIfReadOnly();
      if (value != null && Validate(key, value)) {
        dictionary.Add(key, value);
        OnChange(new[] { key });
      }
    }

    public void Add(KeyValuePair<string, string> item) {
      Add(item.Key, item.Value);
    }

    public void Clear() {
      ThrowIfReadOnly();
      if (dictionary.Count > 0) {
        string[] keys = dictionary.Keys.ToArray();
        dictionary.Clear();
        OnChange(keys);
      }
    }

    public bool Contains(KeyValuePair<string, string> item) {
      return dictionary.Contains(item);
    }

    public bool ContainsKey(string key) {
      return dictionary.ContainsKey(key);
    }

    public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex) {
      dictionary.CopyTo(array, arrayIndex);
    }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() {
      return dictionary.GetEnumerator();
    }

    public bool Remove(string key) {
      ThrowIfReadOnly();
      if (dictionary.Remove(key)) {
        OnChange(new[] { key });
        return true;
      }
      return false;
    }

    public bool Remove(KeyValuePair<string, string> item) {
      ThrowIfReadOnly();
      if (dictionary.Remove(item)) {
        OnChange(new[] { item.Key });
        return true;
      }
      return false;
    }

    public bool TryGetValue(string key, out string value) {
      return dictionary.TryGetValue(key, out value);
    }

    protected virtual void OnChange(string[] keys) { }

    protected virtual bool Validate(string key, string value) {
      return true;
    }

    private void ThrowIfReadOnly() {
      if (this.IsReadOnly) {
        throw new InvalidOperationException("Dictionary is read-only");
      }
    }

    IEnumerable<string> IReadOnlyDictionary<string, string>.Keys => dictionary.Keys;

    IEnumerable<string> IReadOnlyDictionary<string, string>.Values => dictionary.Values;

    IEnumerator IEnumerable.GetEnumerator() {
      return dictionary.GetEnumerator();
    }
  }
}
