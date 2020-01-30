using System.Collections.Generic;
using Codeless.Ecma.Runtime;

namespace Codeless.Ecma {
  public class EcmaArrayIterator : EcmaIterator {
    public EcmaArrayIterator(EcmaValue target, EcmaIteratorResultKind kind)
      : base(target.ToObject(), kind, WellKnownObject.ArrayIteratorPrototype) { }

    public EcmaArrayIterator(IEnumerator<KeyValuePair<EcmaValue, EcmaValue>> target, EcmaIteratorResultKind kind)
      : base(target, kind, WellKnownObject.ArrayIteratorPrototype) { }

    protected override IEnumerator<KeyValuePair<EcmaValue, EcmaValue>> GetEnumerator(object runtimeObject) {
      return runtimeObject as IEnumerator<KeyValuePair<EcmaValue, EcmaValue>> ?? new EcmaArrayEnumerator((RuntimeObject)runtimeObject);
    }
  }
}
