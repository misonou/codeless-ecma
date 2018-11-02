using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime {
  public class ImmutablePrototypeObject : RuntimeObject {
    public ImmutablePrototypeObject(RuntimeObject constructor)
      : base(WellKnownObject.ObjectPrototype, constructor) { }

    [EcmaSpecification("SetImmutablePrototype", EcmaSpecificationKind.AbstractOperations)]
    public override bool SetPrototypeOf(RuntimeObject proto) {
      return EcmaValue.Equals(GetPrototypeOf(), proto, EcmaValueComparison.SameValue);
    }
  }
}
