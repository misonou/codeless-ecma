using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Codeless.Ecma.Intl.Utilities {
  [DebuggerDisplay("Count = {Count}")]
  internal class BcpSubtagCollection : IReadOnlyList<string>, ICollection<string> {
    private readonly HashSet<string> collection = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public BcpSubtagCollection() { }

    public BcpSubtagCollection(HashSet<string> collection, bool isReadOnly) {
      Guard.ArgumentNotNull(collection, "collection");
      this.collection.UnionWith(collection);
      this.IsReadOnly = isReadOnly;
    }

    public BcpSubtagCollection(SortedSet<string> collection, bool isReadOnly) {
      Guard.ArgumentNotNull(collection, "collection");
      this.collection.UnionWith(collection);
      this.IsReadOnly = isReadOnly;
    }

    public int Count => collection.Count;

    public bool IsReadOnly { get; }

    public string this[int index] {
      get {
        if (index >= collection.Count) {
          throw new ArgumentOutOfRangeException("index");
        }
        return collection.ElementAt(index);
      }
    }

    public void Add(string item) {
      ThrowIfReadOnly();
      if (Validate(item) && collection.Add(item)) {
        OnChange();
      }
    }

    public void Clear() {
      ThrowIfReadOnly();
      if (collection.Count > 0) {
        collection.Clear();
        OnChange();
      }
    }

    public bool Contains(string item) {
      return collection.Contains(item);
    }

    public void CopyTo(string[] array, int arrayIndex) {
      collection.CopyTo(array, arrayIndex);
    }

    public int IndexOf(string item) {
      int i = 0;
      if (collection.Contains(item)) {
        foreach (string value in collection) {
          if (collection.Comparer.Equals(value, item)) {
            return i;
          }
          i++;
        }
      }
      return -1;
    }

    public bool Remove(string item) {
      ThrowIfReadOnly();
      if (collection.Remove(item)) {
        OnChange();
        return true;
      }
      return false;
    }

    public IEnumerator<string> GetEnumerator() {
      return collection.GetEnumerator();
    }

    protected virtual void OnChange() { }

    protected virtual bool Validate(string value) {
      return true;
    }

    private void ThrowIfReadOnly() {
      if (this.IsReadOnly) {
        throw new InvalidOperationException("Collection is read-only");
      }
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return collection.GetEnumerator();
    }
  }
}
