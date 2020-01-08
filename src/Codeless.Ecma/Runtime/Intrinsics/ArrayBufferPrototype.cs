using System;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.ArrayBufferPrototype)]
  internal static class ArrayBufferPrototype {
    [IntrinsicMember(WellKnownSymbol.ToStringTag, EcmaPropertyAttributes.Configurable)]
    public const string ToStringTag = InternalString.ObjectTag.ArrayBuffer;

    [IntrinsicMember(EcmaPropertyAttributes.Configurable, Getter = true)]
    public static EcmaValue ByteLength([This] EcmaValue thisValue) {
      ArrayBuffer buffer = thisValue.GetUnderlyingObject<ArrayBuffer>();
      if (buffer.IsShared) {
        throw new EcmaTypeErrorException(InternalString.Error.IncompatibleObject);
      }
      if (buffer.IsDetached) {
        throw new EcmaTypeErrorException(InternalString.Error.BufferDetached);
      }
      return buffer.ByteLength;
    }

    [IntrinsicMember]
    public static EcmaValue Slice([This] EcmaValue thisValue, EcmaValue start, EcmaValue end) {
      ArrayBuffer buffer = thisValue.GetUnderlyingObject<ArrayBuffer>();
      if (buffer.IsShared) {
        throw new EcmaTypeErrorException(InternalString.Error.IncompatibleObject);
      }
      if (buffer.IsDetached) {
        throw new EcmaTypeErrorException(InternalString.Error.BufferDetached);
      }
      long startPos = ArrayHelper.GetBoundIndex(start, buffer.ByteLength, 0);
      long endPos = ArrayHelper.GetBoundIndex(end, buffer.ByteLength, buffer.ByteLength);
      return buffer.Slice(startPos, endPos);
    }
  }
}
