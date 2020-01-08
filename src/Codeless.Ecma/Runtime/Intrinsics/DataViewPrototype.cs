using System;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.DataViewPrototype)]
  internal static class DataViewPrototype {
    [IntrinsicMember(WellKnownSymbol.ToStringTag, EcmaPropertyAttributes.Configurable)]
    public const string ToStringTag = InternalString.ObjectTag.DataView;

    [IntrinsicMember(EcmaPropertyAttributes.Configurable, Getter = true)]
    public static EcmaValue Buffer([This] EcmaValue thisValue) {
      DataView view = thisValue.GetUnderlyingObject<DataView>();
      return view.Buffer;
    }

    [IntrinsicMember(EcmaPropertyAttributes.Configurable, Getter = true)]
    public static EcmaValue ByteOffset([This] EcmaValue thisValue) {
      DataView view = thisValue.GetUnderlyingObject<DataView>();
      Guard.BufferNotDetached(view);
      return view.ByteOffset;
    }

    [IntrinsicMember(EcmaPropertyAttributes.Configurable, Getter = true)]
    public static EcmaValue ByteLength([This] EcmaValue thisValue) {
      DataView view = thisValue.GetUnderlyingObject<DataView>();
      Guard.BufferNotDetached(view);
      return view.ByteLength;
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue GetFloat32([This] EcmaValue thisValue, EcmaValue byteOffset, EcmaValue isLittleEndian) {
      DataView view = thisValue.GetUnderlyingObject<DataView>();
      return view.GetFloat32(byteOffset.ToIndex(), ToEndiannessEnum(isLittleEndian));
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue GetFloat64([This] EcmaValue thisValue, EcmaValue byteOffset, EcmaValue isLittleEndian) {
      DataView view = thisValue.GetUnderlyingObject<DataView>();
      return view.GetFloat64(byteOffset.ToIndex(), ToEndiannessEnum(isLittleEndian));
    }

    [IntrinsicMember]
    public static EcmaValue GetInt8([This] EcmaValue thisValue, EcmaValue byteOffset) {
      DataView view = thisValue.GetUnderlyingObject<DataView>();
      return view.GetInt8(byteOffset.ToIndex());
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue GetInt16([This] EcmaValue thisValue, EcmaValue byteOffset, EcmaValue isLittleEndian) {
      DataView view = thisValue.GetUnderlyingObject<DataView>();
      return view.GetInt16(byteOffset.ToIndex(), ToEndiannessEnum(isLittleEndian));
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue GetInt32([This] EcmaValue thisValue, EcmaValue byteOffset, EcmaValue isLittleEndian) {
      DataView view = thisValue.GetUnderlyingObject<DataView>();
      return view.GetInt32(byteOffset.ToIndex(), ToEndiannessEnum(isLittleEndian));
    }

    [IntrinsicMember]
    public static EcmaValue GetUint8([This] EcmaValue thisValue, EcmaValue byteOffset) {
      DataView view = thisValue.GetUnderlyingObject<DataView>();
      return view.GetUInt8(byteOffset.ToIndex());
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue GetUint16([This] EcmaValue thisValue, EcmaValue byteOffset, EcmaValue isLittleEndian) {
      DataView view = thisValue.GetUnderlyingObject<DataView>();
      return view.GetUInt16(byteOffset.ToIndex(), ToEndiannessEnum(isLittleEndian));
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue GetUint32([This] EcmaValue thisValue, EcmaValue byteOffset, EcmaValue isLittleEndian) {
      DataView view = thisValue.GetUnderlyingObject<DataView>();
      return view.GetUInt32(byteOffset.ToIndex(), ToEndiannessEnum(isLittleEndian));
    }

    [IntrinsicMember(FunctionLength = 2)]
    public static EcmaValue SetFloat32([This] EcmaValue thisValue, EcmaValue byteOffset, EcmaValue value, EcmaValue isLittleEndian) {
      DataView view = thisValue.GetUnderlyingObject<DataView>();
      view.SetFloat32(byteOffset.ToIndex(), value, ToEndiannessEnum(isLittleEndian));
      return default;
    }

    [IntrinsicMember(FunctionLength = 2)]
    public static EcmaValue SetFloat64([This] EcmaValue thisValue, EcmaValue byteOffset, EcmaValue value, EcmaValue isLittleEndian) {
      DataView view = thisValue.GetUnderlyingObject<DataView>();
      view.SetFloat64(byteOffset.ToIndex(), value, ToEndiannessEnum(isLittleEndian));
      return default;
    }

    [IntrinsicMember]
    public static EcmaValue SetInt8([This] EcmaValue thisValue, EcmaValue byteOffset, EcmaValue value) {
      DataView view = thisValue.GetUnderlyingObject<DataView>();
      view.SetInt8(byteOffset.ToIndex(), value);
      return default;
    }

    [IntrinsicMember(FunctionLength = 2)]
    public static EcmaValue SetInt16([This] EcmaValue thisValue, EcmaValue byteOffset, EcmaValue value, EcmaValue isLittleEndian) {
      DataView view = thisValue.GetUnderlyingObject<DataView>();
      view.SetInt16(byteOffset.ToIndex(), value, ToEndiannessEnum(isLittleEndian));
      return default;
    }

    [IntrinsicMember(FunctionLength = 2)]
    public static EcmaValue SetInt32([This] EcmaValue thisValue, EcmaValue byteOffset, EcmaValue value, EcmaValue isLittleEndian) {
      DataView view = thisValue.GetUnderlyingObject<DataView>();
      view.SetInt32(byteOffset.ToIndex(), value, ToEndiannessEnum(isLittleEndian));
      return default;
    }

    [IntrinsicMember]
    public static EcmaValue SetUint8([This] EcmaValue thisValue, EcmaValue byteOffset, EcmaValue value) {
      DataView view = thisValue.GetUnderlyingObject<DataView>();
      view.SetUInt8(byteOffset.ToIndex(), value);
      return default;
    }

    [IntrinsicMember(FunctionLength = 2)]
    public static EcmaValue SetUint16([This] EcmaValue thisValue, EcmaValue byteOffset, EcmaValue value, EcmaValue isLittleEndian) {
      DataView view = thisValue.GetUnderlyingObject<DataView>();
      view.SetUInt16(byteOffset.ToIndex(), value, ToEndiannessEnum(isLittleEndian));
      return default;
    }

    [IntrinsicMember(FunctionLength = 2)]
    public static EcmaValue SetUint32([This] EcmaValue thisValue, EcmaValue byteOffset, EcmaValue value, EcmaValue isLittleEndian) {
      DataView view = thisValue.GetUnderlyingObject<DataView>();
      view.SetUInt32(byteOffset.ToIndex(), value, ToEndiannessEnum(isLittleEndian));
      return default;
    }

    private static DataViewEndianness ToEndiannessEnum(EcmaValue isLittleEndian) {
      return isLittleEndian == default || !isLittleEndian.ToBoolean() ? DataViewEndianness.BigEndian : DataViewEndianness.LittleEndian;
    }
  }
}
