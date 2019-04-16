using Codeless.Ecma.Runtime;

namespace Codeless.Ecma {
  public class EcmaMap : EcmaMapBase {
    public EcmaMap()
      : base(WellKnownObject.MapPrototype) { }

    public EcmaMap(RuntimeObject constructor)
      : base(WellKnownObject.MapPrototype, constructor) { }

    protected override WellKnownObject DefaultIteratorPrototype => WellKnownObject.MapIteratorPrototype;

    public EcmaValue Get(EcmaValue key) {
      return GetItem(key);
    }

    public EcmaMap Set(EcmaValue key, EcmaValue value) {
      SetItem(key, value);
      return this;
    }
  }
}
