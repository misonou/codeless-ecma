using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Codeless.Ecma {
  public class WeakKeyedCollection : WeakKeyedItem {
    private static readonly HashSet<WeakKeyedItem> collections = new HashSet<WeakKeyedItem>();
    private static bool finalExit;

    private readonly ConcurrentDictionary<WeakKeyedItem, WeakKeyedItem> dictionary = new ConcurrentDictionary<WeakKeyedItem, WeakKeyedItem>();

    static WeakKeyedCollection() {
      AppDomain.CurrentDomain.DomainUnload += OnDomainUnload;
      new Thread(RemoveFreedObjects).Start();
    }

    public WeakKeyedCollection() {
      lock (collections) {
        collections.Add(this);
      }
    }

    public WeakKeyedCollection(IEnumerable<WeakKeyedItem> items)
      : this() {
      Guard.ArgumentNotNull(items, "items");
      foreach(WeakKeyedItem item in items) {
        dictionary.TryAdd(item, item);
      }
    }

    ~WeakKeyedCollection() {
      lock (collections) {
        collections.Remove(this);
      }
    }

    public T TryGet<T>(object key) where T : class {
      WeakKeyedItem value;
      if (dictionary.TryGetValue(new WeakKeyedItem(key), out value)) {
        return value as T;
      }
      return null;
    }

    public T GetOrAdd<T>(T item) where T : WeakKeyedItem {
      if (item.Target == item) {
        return item;
      }
      return (T)dictionary.GetOrAdd(item, item);
    }

    private static void OnDomainUnload(object sender, EventArgs e) {
      finalExit = true;
      GC.CancelFullGCNotification();
    }

    private static void RemoveFreedObjects() {
      while (!finalExit) {
        List<WeakKeyedItem> list;
        lock (collections) {
          list = new List<WeakKeyedItem>(collections);
        }
        List<WeakKeyedItem> listToRemove = new List<WeakKeyedItem>();
        foreach (WeakKeyedItem pointer in list) {
          WeakKeyedCollection collection = pointer.Target as WeakKeyedCollection;
          if (collection == null) {
            listToRemove.Add(pointer);
            continue;
          }
          foreach (WeakKeyedItem item in collection.dictionary.Keys) {
            if (item.Target == null) {
              WeakKeyedItem dummy;
              collection.dictionary.TryRemove(item, out dummy);
            }
          }
        }
        if (listToRemove.Count > 0) {
          lock (collections) {
            foreach (WeakKeyedItem item in listToRemove) {
              collections.Remove(item);
            }
          }
        }
        Thread.Sleep(1000);
      }
    }
  }
}
