using Codeless.Ecma.Runtime;

namespace Codeless.Ecma {
  public class EcmaWeakSet : RuntimeObject {
    private readonly WeakKeyedCollection collection = new WeakKeyedCollection();

    public EcmaWeakSet()
      : base(WellKnownObject.WeakSetPrototype) { }

    public EcmaWeakSet Add(RuntimeObject item) {
      if (item is TransientPrimitiveObject) {
        throw new EcmaTypeErrorException(InternalString.Error.InvalidWeakSetValue);
      }
      collection.GetOrAdd(new WeakKeyedItem(item));
      return this;
    }

    public bool Has(RuntimeObject item) {
      return collection.TryGet<object>(item) != null;
    }

    public bool Delete(RuntimeObject item) {
      return collection.TryRemove(item);
    }
  }
}
