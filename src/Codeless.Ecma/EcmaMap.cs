using Codeless.Ecma.Runtime;

namespace Codeless.Ecma {
  [Cloneable(false)]
  public class EcmaMap : EcmaMapBase {
    public EcmaMap()
      : base(WellKnownObject.MapPrototype) { }

    public EcmaValue Get(EcmaValue key) {
      return GetItem(key);
    }

    public EcmaMap Set(EcmaValue key, EcmaValue value) {
      SetItem(key, value);
      return this;
    }

    public override EcmaIterator Keys() {
      return new EcmaMapIterator(this, EcmaIteratorResultKind.Key);
    }

    public override EcmaIterator Values() {
      return new EcmaMapIterator(this, EcmaIteratorResultKind.Value);
    }

    public override EcmaIterator Entries() {
      return new EcmaMapIterator(this, EcmaIteratorResultKind.Entry);
    }
  }
}
