using System.Collections.Generic;

namespace Codeless.Ecma {
  public class EcmaMapIterator : EcmaIterator {
    public EcmaMapIterator(EcmaMap target, EcmaIteratorResultKind kind)
      : base(target, kind, WellKnownObject.MapIteratorPrototype) { }

    protected override IEnumerator<KeyValuePair<EcmaValue, EcmaValue>> GetEnumerator(object runtimeObject) {
      return ((EcmaMap)runtimeObject).GetEnumerator();
    }
  }
}
