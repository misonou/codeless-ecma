#if NET35
using Codeless.Ecma;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections.Generic {
  public class SortedSet<T> : ICollection<T> {
    private readonly SortedDictionary<T, bool> dictionary;

    public SortedSet() {
      dictionary = new SortedDictionary<T, bool>();
    }

    public SortedSet(IComparer<T> comparer) {
      Guard.ArgumentNotNull(comparer, "comparer");
      dictionary = new SortedDictionary<T, bool>(comparer);
    }

    public SortedSet(IEnumerable<T> collection) {
      Guard.ArgumentNotNull(collection, "collection");
      dictionary = new SortedDictionary<T, bool>();
      foreach (T item in collection) {
        Add(item);
      }
    }

    public SortedSet(IEnumerable<T> collection, IComparer<T> comparer) {
      Guard.ArgumentNotNull(collection, "collection");
      Guard.ArgumentNotNull(comparer, "comparer");
      dictionary = new SortedDictionary<T, bool>(comparer);
      foreach (T item in collection) {
        Add(item);
      }
    }

    public int Count => dictionary.Count;

    public bool IsReadOnly => false;

    public IComparer<T> Comparer => dictionary.Comparer;

    public bool Add(T item) {
      if (!dictionary.ContainsKey(item)) {
        dictionary.Add(item, true);
        return true;
      }
      return false;
    }

    public void Clear() {
      dictionary.Clear();
    }

    public bool Contains(T item) {
      return dictionary.ContainsKey(item);
    }

    public void CopyTo(T[] array, int arrayIndex) {
      dictionary.Keys.CopyTo(array, arrayIndex);
    }

    public IEnumerator<T> GetEnumerator() {
      return dictionary.Keys.GetEnumerator();
    }

    public bool Remove(T item) {
      return dictionary.Remove(item);
    }

    void ICollection<T>.Add(T item) {
      Add(item);
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return dictionary.Keys.GetEnumerator();
    }
  }
}
#endif
