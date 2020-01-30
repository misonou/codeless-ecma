using System.Collections.Generic;

namespace Codeless.Ecma {
  public class EcmaSetIterator : EcmaIterator {
    public EcmaSetIterator(EcmaSet target, EcmaIteratorResultKind kind)
      : base(target, kind, WellKnownObject.SetIteratorPrototype) { }

    protected override IEnumerator<KeyValuePair<EcmaValue, EcmaValue>> GetEnumerator(object runtimeObject) {
      return ((EcmaSet)runtimeObject).GetEnumerator();
    }
  }
}
