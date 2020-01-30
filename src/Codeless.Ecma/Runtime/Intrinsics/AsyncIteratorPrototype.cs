using System;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.AsyncIteratorPrototype)]
  internal static class AsyncIteratorPrototype {
    [IntrinsicMember(WellKnownSymbol.AsyncIterator)]
    public static EcmaValue AsyncIterator([This] EcmaValue thisArgs) {
      return thisArgs;
    }
  }
}
