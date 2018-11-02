using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma {
  internal class Map<T1, T2> : IEnumerable {
    private Dictionary<T1, T2> forward = new Dictionary<T1, T2>();
    private Dictionary<T2, T1> reverse = new Dictionary<T2, T1>();

    public Map() {
      this.Forward = new Indexer<T1, T2>(forward);
      this.Reverse = new Indexer<T2, T1>(reverse);
    }

    public class Indexer<T3, T4> : IReadOnlyDictionary<T3, T4> {
      private Dictionary<T3, T4> dictionary;

      internal Indexer(Dictionary<T3, T4> dictionary) {
        this.dictionary = dictionary;
      }

      public T4 this[T3 index] {
        get { return dictionary[index]; }
      }

      public int Count {
        get { return dictionary.Count; }
      }

      public bool ContainsKey(T3 key) {
        return dictionary.ContainsKey(key);
      }

      public bool TryGetValue(T3 key, out T4 value) {
        return dictionary.TryGetValue(key, out value);
      }

      public IEnumerable<T3> Keys {
        get { return dictionary.Keys; }
      }

      public IEnumerable<T4> Values {
        get { return dictionary.Values; }
      }

      IEnumerator IEnumerable.GetEnumerator() {
        return dictionary.GetEnumerator();
      }

      IEnumerator<KeyValuePair<T3, T4>> IEnumerable<KeyValuePair<T3, T4>>.GetEnumerator() {
        return dictionary.GetEnumerator();
      }
    }

    public void Add(T1 t1, T2 t2) {
      forward.Add(t1, t2);
      reverse.Add(t2, t1);
    }

    public Indexer<T1, T2> Forward { get; private set; }

    public Indexer<T2, T1> Reverse { get; private set; }

    IEnumerator IEnumerable.GetEnumerator() {
      return forward.GetEnumerator();
    }
  }
}
