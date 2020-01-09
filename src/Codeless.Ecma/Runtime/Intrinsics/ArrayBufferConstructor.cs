using System;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.ArrayBuffer)]
  internal static class ArrayBufferConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.DenyCall, ObjectType = typeof(ArrayBuffer), Prototype = WellKnownObject.ArrayBufferPrototype)]
    public static EcmaValue ArrayBuffer([This] EcmaValue thisValue, EcmaValue length) {
      ArrayBuffer buffer = thisValue.GetUnderlyingObject<ArrayBuffer>();
      buffer.Init(length.ToIndex());
      return thisValue;
    }

    [IntrinsicMember]
    public static EcmaValue IsView(EcmaValue buffer) {
      return buffer.GetUnderlyingObject() is IArrayBufferView;
    }

    [IntrinsicMember(WellKnownSymbol.Species, Getter = true)]
    public static EcmaValue Species([This] EcmaValue thisValue) {
      return thisValue;
    }
  }
}
