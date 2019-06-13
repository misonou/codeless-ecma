using Codeless.Ecma.Runtime;

namespace Codeless.Ecma {
  public class EcmaWeakMap : RuntimeObject {
    private readonly WeakKeyedCollection collection = new WeakKeyedCollection();

    public EcmaWeakMap()
      : base(WellKnownObject.WeakMapPrototype) { }

    public EcmaValue Get(RuntimeObject key) {
      Entry entry = collection.TryGet<Entry>(key);
      return entry != null ? entry.Value : default;
    }

    public EcmaWeakMap Set(RuntimeObject key, EcmaValue value) {
      if (key is TransientPrimitiveObject) {
        throw new EcmaTypeErrorException("Invalid value used as weak map key");
      }
      Entry entry = collection.GetOrAdd(new Entry(key, value));
      entry.Value = value;
      return this;
    }

    public bool Has(RuntimeObject key) {
      return collection.TryGet<Entry>(key) != null;
    }

    public bool Delete(RuntimeObject key) {
      return collection.TryRemove(key);
    }

    private class Entry : WeakKeyedItem {
      public Entry(object target, EcmaValue value)
        : base(target) {
        this.Value = value;
      }

      public EcmaValue Value { get; set; }
    }
  }
}
