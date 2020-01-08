using System;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.SharedArrayBuffer)]
  internal static class SharedArrayBufferConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.DenyCall, ObjectType = typeof(SharedArrayBuffer))]
    public static EcmaValue SharedArrayBuffer([This] EcmaValue thisValue, EcmaValue length) {
      SharedArrayBuffer buffer = thisValue.GetUnderlyingObject<SharedArrayBuffer>();
      buffer.Init(length.ToIndex());
      return thisValue;
    }

    [IntrinsicMember(WellKnownSymbol.Species, Getter = true)]
    public static EcmaValue Species([This] EcmaValue thisValue) {
      return thisValue;
    }
  }
}
