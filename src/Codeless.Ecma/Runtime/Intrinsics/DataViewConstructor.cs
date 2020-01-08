using System;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.DataView)]
  internal static class DataViewConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.DenyCall, ObjectType = typeof(DataView))]
    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue DataView([This] EcmaValue thisValue, EcmaValue buffer, EcmaValue offset, EcmaValue length) {
      DataView view = thisValue.GetUnderlyingObject<DataView>();
      ArrayBuffer arrayBuffer = buffer.GetUnderlyingObject<ArrayBuffer>();
      long offsetInt = offset == default ? 0 : offset.ToIndex();
      long lengthInt = length == default ? arrayBuffer.ByteLength - offsetInt : length.ToIndex();
      view.Init(arrayBuffer, offsetInt, lengthInt);
      return thisValue;
    }
  }
}
