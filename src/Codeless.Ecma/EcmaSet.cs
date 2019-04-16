using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;

namespace Codeless.Ecma {
  public class EcmaSet : EcmaMapBase {
    public EcmaSet()
      : base(WellKnownObject.SetPrototype) { }

    public EcmaSet(RuntimeObject constructor)
      : base(WellKnownObject.SetPrototype, constructor) { }

    protected override WellKnownObject DefaultIteratorPrototype => WellKnownObject.SetIteratorPrototype;

    public EcmaSet Add(EcmaValue key) {
      SetItem(key, key);
      return this;
    }
  }
}
