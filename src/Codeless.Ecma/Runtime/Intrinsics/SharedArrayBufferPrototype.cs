using System;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.SharedArrayBufferPrototype)]
  internal static class SharedArrayBufferPrototype {
    [IntrinsicMember(WellKnownSymbol.ToStringTag, EcmaPropertyAttributes.Configurable)]
    public const string ToStringTag = InternalString.ObjectTag.SharedArrayBuffer;

    [IntrinsicMember(EcmaPropertyAttributes.Configurable, Getter = true)]
    public static EcmaValue ByteLength([This] EcmaValue thisValue) {
      SharedArrayBuffer buffer = thisValue.GetUnderlyingObject<SharedArrayBuffer>();
      return buffer.ByteLength;
    }

    [IntrinsicMember]
    public static EcmaValue Slice([This] EcmaValue thisValue, EcmaValue start, EcmaValue end) {
      SharedArrayBuffer buffer = thisValue.GetUnderlyingObject<SharedArrayBuffer>();
      long startPos = ArrayHelper.GetBoundIndex(start, buffer.ByteLength, 0);
      long endPos = ArrayHelper.GetBoundIndex(end, buffer.ByteLength, buffer.ByteLength);
      return buffer.Slice(startPos, endPos);
    }
  }
}
