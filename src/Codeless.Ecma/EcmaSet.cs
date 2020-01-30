using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;

namespace Codeless.Ecma {
  [Cloneable(false)]
  public class EcmaSet : EcmaMapBase {
    public EcmaSet()
      : base(WellKnownObject.SetPrototype) { }

    public EcmaSet Add(EcmaValue key) {
      SetItem(key, key);
      return this;
    }

    public override EcmaIterator Keys() {
      return new EcmaSetIterator(this, EcmaIteratorResultKind.Key);
    }

    public override EcmaIterator Values() {
      return new EcmaSetIterator(this, EcmaIteratorResultKind.Value);
    }

    public override EcmaIterator Entries() {
      return new EcmaSetIterator(this, EcmaIteratorResultKind.Entry);
    }
  }
}
