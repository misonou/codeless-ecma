using Codeless.Ecma.Diagnostics;
using Codeless.Ecma.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Codeless.Ecma {
  public abstract class EcmaMapBase : RuntimeObject, IEnumerable<KeyValuePair<EcmaValue, EcmaValue>>, IInspectorMetaProvider {
    private Dictionary<EcmaValue, Entry> dict = new Dictionary<EcmaValue, Entry>(EcmaValueEqualityComparer.SameValueZero);
    private HashSet<Enumerator> iterators;

    public EcmaMapBase(WellKnownObject proto)
      : base(proto) { }

    public int Size {
      get { return dict.Count; }
    }

    public bool Has(EcmaValue key) {
      return TryGetValueChecked(key, out Entry entry);
    }

    protected EcmaValue GetItem(EcmaValue key) {
      if (TryGetValueChecked(key, out Entry entry)) {
        return entry.Value;
      }
      return default;
    }

    protected void SetItem(EcmaValue key, EcmaValue value) {
      if (key == EcmaValue.NegativeZero) {
        key = 0;
      }
      Entry entry;
      if (dict.TryGetValue(key, out entry)) {
        entry.Removed = false;
        entry.Value = value;
      } else {
        entry = new Entry(value);
        dict[key] = entry;
        if (iterators != null) {
          foreach (Enumerator current in iterators) {
            current.AppendNewEntry(key, entry);
          }
        }
      }
    }

    public bool Delete(EcmaValue key) {
      if (TryGetValueChecked(key, out Entry entry)) {
        entry.Removed = true;
      }
      return dict.Remove(key);
    }

    public void Clear() {
      foreach (Entry entry in dict.Values) {
        entry.Removed = true;
      }
      dict.Clear();
    }

    public void ForEach(Action<EcmaValue, EcmaValue> callback) {
      IEnumerator<KeyValuePair<EcmaValue, EcmaValue>> iterator = GetEnumerator();
      while (iterator.MoveNext()) {
        KeyValuePair<EcmaValue, EcmaValue> entry = iterator.Current;
        callback(entry.Value, entry.Key);
      }
    }

    public abstract EcmaIterator Keys();

    public abstract EcmaIterator Values();

    public abstract EcmaIterator Entries();

    public IEnumerator<KeyValuePair<EcmaValue, EcmaValue>> GetEnumerator() {
      return new Enumerator(this);
    }

    protected override void OnCloned(RuntimeObject sourceObj, bool isTransfer, CloneContext context) {
      base.OnCloned(sourceObj, isTransfer, context);
      dict = new Dictionary<EcmaValue, Entry>();
      iterators = null;
      foreach (KeyValuePair<EcmaValue, Entry> e in ((EcmaMapBase)sourceObj).dict) {
        if (!e.Value.Removed) {
          dict[context.Clone(e.Key)] = new Entry(context.Clone(e.Value.Value));
        }
      }
    }

    private bool TryGetValueChecked(EcmaValue key, out Entry entry) {
      if (dict.TryGetValue(key, out entry) && !entry.Removed) {
        return true;
      }
      entry = null;
      return false;
    }

    void IInspectorMetaProvider.FillInInspectorMetaObject(InspectorMetaObject meta) {
      meta.EnumerableProperties.Add("[[Entries]]", dict.Select(v => new[] { v.Key, v.Value.Value }).ToList());
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }

    private class Entry {
      public Entry(EcmaValue value) {
        this.Value = value;
      }

      public bool Removed { get; set; }
      public EcmaValue Value { get; set; }
    }

    private class Enumerator : IEnumerator<KeyValuePair<EcmaValue, EcmaValue>> {
      private readonly List<KeyValuePair<EcmaValue, Entry>> entries;
      private readonly EcmaMapBase target;
      private bool disposed;
      private int nextIndex = -1;

      public Enumerator(EcmaMapBase target) {
        this.target = target;
        this.entries = target.dict.ToList();
        if (target.iterators == null) {
          target.iterators = new HashSet<Enumerator>();
        }
        target.iterators.Add(this);
      }

      public KeyValuePair<EcmaValue, EcmaValue> Current {
        get { return new KeyValuePair<EcmaValue, EcmaValue>(entries[nextIndex].Key, entries[nextIndex].Value.Value); }
      }

      public void AppendNewEntry(EcmaValue key, Entry e) {
        entries.Add(new KeyValuePair<EcmaValue, Entry>(key, e));
      }

      public void Dispose() {
        if (!disposed) {
          target.iterators.Remove(this);
          disposed = true;
        }
      }

      public bool MoveNext() {
        while ((++nextIndex) < entries.Count) {
          if (!entries[nextIndex].Value.Removed) {
            return true;
          }
        }
        Dispose();
        return false;
      }

      public void Reset() {
        throw new NotSupportedException();
      }

      object IEnumerator.Current {
        get { return this.Current; }
      }

      ~Enumerator() {
        Dispose();
      }
    }
  }
}
