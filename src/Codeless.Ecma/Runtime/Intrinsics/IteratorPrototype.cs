using System;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.IteratorPrototype)]
  internal static class IteratorPrototype {
    [IntrinsicMember(WellKnownSymbol.Iterator)]
    public static EcmaValue Iterator([This] EcmaValue thisArgs) {
      return thisArgs;
    }
  }
}
