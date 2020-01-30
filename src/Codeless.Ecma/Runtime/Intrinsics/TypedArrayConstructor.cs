using System;
using System.Collections.Generic;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.TypedArray)]
  internal static class TypedArrayConstructor {
    [IntrinsicConstructor(Global = false, Prototype = WellKnownObject.TypedArrayPrototype)]
    public static void TypedArray() {
      throw new EcmaTypeErrorException(InternalString.Error.TypedArrayIsAbstract);
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue From([This] EcmaValue thisValue, EcmaValue source, EcmaValue mapFn, EcmaValue thisArg) {
      if (!thisValue.IsCallable || !thisValue.ToObject().IsConstructor) {
        throw new EcmaTypeErrorException(InternalString.Error.NotConstructor);
      }
      if (mapFn != default) {
        Guard.ArgumentIsCallable(mapFn);
      }
      if (source.IsNullOrUndefined) {
        return thisValue.Construct(0);
      }
      RuntimeObject obj = source.ToObject();
      RuntimeObject iterator = obj.GetMethod(WellKnownSymbol.Iterator);
      TypedArray array;
      if (iterator != null) {
        List<EcmaValue> values = new List<EcmaValue>(obj.GetIterator());
        array = thisValue.Construct(values.Count).GetUnderlyingObject<TypedArray>();
        if (array.Length < values.Count) {
          throw new EcmaTypeErrorException(InternalString.Error.TypedArrayBufferTooSmall);
        }
        int i = 0;
        foreach (EcmaValue v in values) {
          EcmaValue value = v;
          if (mapFn != default) {
            value = mapFn.Call(thisArg, value, i);
          }
          array.Set(i++, value);
        }
      } else {
        long length = source[WellKnownProperty.Length].ToLength();
        array = thisValue.Construct(length).GetUnderlyingObject<TypedArray>();
        if (array.Length < length) {
          throw new EcmaTypeErrorException(InternalString.Error.TypedArrayBufferTooSmall);
        }
        for (int i = 0; i < length; i++) {
          EcmaValue value = obj.Get(i);
          if (mapFn != default) {
            value = mapFn.Call(thisArg, value, i);
          }
          array.Set(i, value);
        }
      }
      return array;
    }

    [IntrinsicMember(FunctionLength = 0)]
    public static EcmaValue Of([This] EcmaValue thisValue, params EcmaValue[] args) {
      TypedArray array = thisValue.Construct(args.Length).GetUnderlyingObject<TypedArray>();
      if (array.Length < args.Length) {
        throw new EcmaTypeErrorException(InternalString.Error.TypedArrayBufferTooSmall);
      }
      for (int i = 0, len = args.Length; i < len; i++) {
        array.Set(i, args[i]);
      }
      return array;
    }

    [IntrinsicMember(WellKnownSymbol.Species, Getter = true)]
    public static EcmaValue Species([This] EcmaValue thisValue) {
      return thisValue;
    }

    public static EcmaValue ConstructTypedArray(TypedArrayKind kind, EcmaValue thisValue, EcmaValue[] args) {
      TypedArray array = thisValue.GetUnderlyingObject<TypedArray>();
      if (args.Length == 0) {
        array.Init(0);
        return thisValue;
      }
      if (args[0].Type != EcmaValueType.Object) {
        array.Init(args[0].ToIndex());
        return thisValue;
      }
      RuntimeObject obj = args[0].ToObject();
      long bytesPerElement = array.ElementSize;

      if (obj is TypedArray srcArray) {
        Guard.BufferNotDetached(srcArray);
        RuntimeObject bufferConstructor;
        if (srcArray.Buffer.IsShared) {
          bufferConstructor = srcArray.Realm.GetRuntimeObject(WellKnownObject.ArrayBuffer);
        } else {
          bufferConstructor = RuntimeObject.GetSpeciesConstructor(srcArray.Buffer, WellKnownObject.ArrayBuffer);
        }
        if (srcArray.ArrayKind == kind) {
          ArrayBuffer buffer = srcArray.Buffer.Clone(srcArray.ByteOffset, bytesPerElement * srcArray.Length, bufferConstructor);
          array.Init(buffer);
        } else {
          ArrayBuffer buffer = ArrayBuffer.AllocateArrayBuffer(bufferConstructor, bytesPerElement * srcArray.Length);
          Guard.BufferNotDetached(srcArray);
          array.Init(buffer);
          for (int i = 0, j = 0, count = srcArray.Length; i < count; i++, j++) {
            array.SetValueInBuffer(j, srcArray.GetValueFromBuffer(i));
          }
        }
        return thisValue;
      }
      if (obj is ArrayBuffer srcBuffer) {
        long offset = args.Length > 1 ? args[1].ToIndex() : 0;
        if ((offset % bytesPerElement) != 0) {
          throw new EcmaRangeErrorException(InternalString.Error.TypedArrayInvalidOffset, TypedArrayInfo.GetTypedArrayName(array.ArrayKind), bytesPerElement);
        }
        long length;
        if (args.Length <= 2 || args[2] == default) {
          Guard.BufferNotDetached(srcBuffer);
          if ((srcBuffer.ByteLength % bytesPerElement) != 0) {
            throw new EcmaRangeErrorException(InternalString.Error.TypedArrayInvalidByteLength, TypedArrayInfo.GetTypedArrayName(array.ArrayKind), bytesPerElement);
          }
          if (srcBuffer.ByteLength < offset) {
            throw new EcmaRangeErrorException(InternalString.Error.BufferOffsetOutOfBound, offset);
          }
          length = srcBuffer.ByteLength - offset;
        } else {
          length = args[2].ToIndex() * bytesPerElement;
          Guard.BufferNotDetached(srcBuffer);
          if (offset + length > srcBuffer.ByteLength) {
            throw new EcmaRangeErrorException(InternalString.Error.TypedArrayInvalidLength, length);
          }
        }
        array.Init(srcBuffer, offset, length);
        return thisValue;
      }
      RuntimeObject iterator = obj.GetMethod(WellKnownSymbol.Iterator);
      if (iterator != null) {
        List<EcmaValue> values = new List<EcmaValue>(obj.GetIterator());
        array.Init(values.Count);
        int i = 0;
        foreach (EcmaValue v in values) {
          array.Set(i++, v);
        }
      } else {
        long length = obj[WellKnownProperty.Length].ToLength();
        array.Init((int)length);
        for (int i = 0; i < length; i++) {
          array.Set(i, obj[i]);
        }
      }
      return thisValue;
    }
  }

  #region Derived constructors
  [IntrinsicObject(WellKnownObject.Float32Array)]
  internal static class Float32ArrayConstructor {
    [IntrinsicMember("BYTES_PER_ELEMENT", EcmaPropertyAttributes.None)]
    public const int BytesPerElement = sizeof(float);

    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.DenyCall, ObjectType = typeof(Float32Array), SuperClass = WellKnownObject.TypedArray, Prototype = WellKnownObject.Float32ArrayPrototype)]
    [IntrinsicMember(FunctionLength = 3)]
    public static EcmaValue Float32Array([This] EcmaValue thisValue, params EcmaValue[] args) {
      return TypedArrayConstructor.ConstructTypedArray(TypedArrayKind.Float32Array, thisValue, args);
    }
  }

  [IntrinsicObject(WellKnownObject.Float64Array)]
  internal static class Float64ArrayConstructor {
    [IntrinsicMember("BYTES_PER_ELEMENT", EcmaPropertyAttributes.None)]
    public const int BytesPerElement = sizeof(double);

    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.DenyCall, ObjectType = typeof(Float64Array), SuperClass = WellKnownObject.TypedArray, Prototype = WellKnownObject.Float64ArrayPrototype)]
    [IntrinsicMember(FunctionLength = 3)]
    public static EcmaValue Float64Array([This] EcmaValue thisValue, params EcmaValue[] args) {
      return TypedArrayConstructor.ConstructTypedArray(TypedArrayKind.Float64Array, thisValue, args);
    }
  }

  [IntrinsicObject(WellKnownObject.Int8Array)]
  internal static class Int8ArrayConstructor {
    [IntrinsicMember("BYTES_PER_ELEMENT", EcmaPropertyAttributes.None)]
    public const int BytesPerElement = sizeof(sbyte);

    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.DenyCall, ObjectType = typeof(Int8Array), SuperClass = WellKnownObject.TypedArray, Prototype = WellKnownObject.Int8ArrayPrototype)]
    [IntrinsicMember(FunctionLength = 3)]
    public static EcmaValue Int8Array([This] EcmaValue thisValue, params EcmaValue[] args) {
      return TypedArrayConstructor.ConstructTypedArray(TypedArrayKind.Int8Array, thisValue, args);
    }
  }

  [IntrinsicObject(WellKnownObject.Int16Array)]
  internal static class Int16ArrayConstructor {
    [IntrinsicMember("BYTES_PER_ELEMENT", EcmaPropertyAttributes.None)]
    public const int BytesPerElement = sizeof(short);

    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.DenyCall, ObjectType = typeof(Int16Array), SuperClass = WellKnownObject.TypedArray, Prototype = WellKnownObject.Int16ArrayPrototype)]
    [IntrinsicMember(FunctionLength = 3)]
    public static EcmaValue Int16Array([This] EcmaValue thisValue, params EcmaValue[] args) {
      return TypedArrayConstructor.ConstructTypedArray(TypedArrayKind.Int16Array, thisValue, args);
    }
  }

  [IntrinsicObject(WellKnownObject.Int32Array)]
  internal static class Int32ArrayConstructor {
    [IntrinsicMember("BYTES_PER_ELEMENT", EcmaPropertyAttributes.None)]
    public const int BytesPerElement = sizeof(int);

    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.DenyCall, ObjectType = typeof(Int32Array), SuperClass = WellKnownObject.TypedArray, Prototype = WellKnownObject.Int32ArrayPrototype)]
    [IntrinsicMember(FunctionLength = 3)]
    public static EcmaValue Int32Array([This] EcmaValue thisValue, params EcmaValue[] args) {
      return TypedArrayConstructor.ConstructTypedArray(TypedArrayKind.Int32Array, thisValue, args);
    }
  }

  [IntrinsicObject(WellKnownObject.Uint8Array)]
  internal static class Uint8ArrayConstructor {
    [IntrinsicMember("BYTES_PER_ELEMENT", EcmaPropertyAttributes.None)]
    public const int BytesPerElement = sizeof(byte);

    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.DenyCall, ObjectType = typeof(Uint8Array), SuperClass = WellKnownObject.TypedArray, Prototype = WellKnownObject.Uint8ArrayPrototype)]
    [IntrinsicMember(FunctionLength = 3)]
    public static EcmaValue Uint8Array([This] EcmaValue thisValue, params EcmaValue[] args) {
      return TypedArrayConstructor.ConstructTypedArray(TypedArrayKind.Uint8Array, thisValue, args);
    }
  }

  [IntrinsicObject(WellKnownObject.Uint8ClampedArray)]
  internal static class Uint8ClampedArrayConstructor {
    [IntrinsicMember("BYTES_PER_ELEMENT", EcmaPropertyAttributes.None)]
    public const int BytesPerElement = sizeof(byte);

    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.DenyCall, ObjectType = typeof(Uint8ClampedArray), SuperClass = WellKnownObject.TypedArray, Prototype = WellKnownObject.Uint8ClampedArrayPrototype)]
    [IntrinsicMember(FunctionLength = 3)]
    public static EcmaValue Uint8ClampedArray([This] EcmaValue thisValue, params EcmaValue[] args) {
      return TypedArrayConstructor.ConstructTypedArray(TypedArrayKind.Uint8ClampedArray, thisValue, args);
    }
  }

  [IntrinsicObject(WellKnownObject.Uint16Array)]
  internal static class Uint16ArrayConstructor {
    [IntrinsicMember("BYTES_PER_ELEMENT", EcmaPropertyAttributes.None)]
    public const int BytesPerElement = sizeof(ushort);

    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.DenyCall, ObjectType = typeof(Uint16Array), SuperClass = WellKnownObject.TypedArray, Prototype = WellKnownObject.Uint16ArrayPrototype)]
    [IntrinsicMember(FunctionLength = 3)]
    public static EcmaValue Uint16Array([This] EcmaValue thisValue, params EcmaValue[] args) {
      return TypedArrayConstructor.ConstructTypedArray(TypedArrayKind.Uint16Array, thisValue, args);
    }
  }

  [IntrinsicObject(WellKnownObject.Uint32Array)]
  internal static class Uint32ArrayConstructor {
    [IntrinsicMember("BYTES_PER_ELEMENT", EcmaPropertyAttributes.None)]
    public const int BytesPerElement = sizeof(uint);

    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.DenyCall, ObjectType = typeof(Uint32Array), SuperClass = WellKnownObject.TypedArray, Prototype = WellKnownObject.Uint32ArrayPrototype)]
    [IntrinsicMember(FunctionLength = 3)]
    public static EcmaValue Uint32Array([This] EcmaValue thisValue, params EcmaValue[] args) {
      return TypedArrayConstructor.ConstructTypedArray(TypedArrayKind.Uint32Array, thisValue, args);
    }
  }

  [IntrinsicObject(WellKnownObject.BigInt64Array)]
  internal static class BigInt64ArrayConstructor {
    [IntrinsicMember("BYTES_PER_ELEMENT", EcmaPropertyAttributes.None)]
    public const int BytesPerElement = sizeof(long);

    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.DenyCall, ObjectType = typeof(BigInt64Array), SuperClass = WellKnownObject.TypedArray, Prototype = WellKnownObject.BigInt64ArrayPrototype)]
    [IntrinsicMember(FunctionLength = 3)]
    public static EcmaValue BigInt64Array([This] EcmaValue thisValue, params EcmaValue[] args) {
      return TypedArrayConstructor.ConstructTypedArray(TypedArrayKind.BigInt64Array, thisValue, args);
    }
  }

  [IntrinsicObject(WellKnownObject.BigUint64Array)]
  internal static class BigUint64ArrayConstructor {
    [IntrinsicMember("BYTES_PER_ELEMENT", EcmaPropertyAttributes.None)]
    public const int BytesPerElement = sizeof(ulong);

    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.DenyCall, ObjectType = typeof(BigUint64Array), SuperClass = WellKnownObject.TypedArray, Prototype = WellKnownObject.BigUint64ArrayPrototype)]
    [IntrinsicMember(FunctionLength = 3)]
    public static EcmaValue BigUint64Array([This] EcmaValue thisValue, params EcmaValue[] args) {
      return TypedArrayConstructor.ConstructTypedArray(TypedArrayKind.BigUint64Array, thisValue, args);
    }
  }
  #endregion
}
