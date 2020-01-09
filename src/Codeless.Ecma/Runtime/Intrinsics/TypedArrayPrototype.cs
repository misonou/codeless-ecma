using System;
using System.Collections.Generic;
using System.Text;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.TypedArrayPrototype)]
  internal static class TypedArrayPrototype {
    [IntrinsicMember(EcmaPropertyAttributes.Configurable, Getter = true)]
    public static EcmaValue Buffer([This] EcmaValue thisValue) {
      TypedArray view = thisValue.GetUnderlyingObject<TypedArray>();
      return view.Buffer;
    }

    [IntrinsicMember(EcmaPropertyAttributes.Configurable, Getter = true)]
    public static EcmaValue ByteOffset([This] EcmaValue thisValue) {
      TypedArray view = thisValue.GetUnderlyingObject<TypedArray>();
      return view.ByteOffset;
    }

    [IntrinsicMember(EcmaPropertyAttributes.Configurable, Getter = true)]
    public static EcmaValue ByteLength([This] EcmaValue thisValue) {
      TypedArray view = thisValue.GetUnderlyingObject<TypedArray>();
      return view.ByteLength;
    }

    [IntrinsicMember(EcmaPropertyAttributes.Configurable, Getter = true)]
    public static EcmaValue Length([This] EcmaValue thisValue) {
      TypedArray view = thisValue.GetUnderlyingObject<TypedArray>();
      return view.Length;
    }

    [IntrinsicMember(WellKnownSymbol.ToStringTag, EcmaPropertyAttributes.Configurable, Getter = true)]
    public static EcmaValue ToStringTag([This] EcmaValue thisValue) {
      if (thisValue.GetUnderlyingObject() is TypedArray view) {
        return TypedArrayInfo.GetTypedArrayName(view.ArrayKind);
      }
      return EcmaValue.Undefined;
    }

    [IntrinsicMember]
    public static EcmaValue Entries([This] EcmaValue thisValue) {
      TypedArray array = thisValue.GetUnderlyingObject<TypedArray>();
      Guard.BufferNotDetached(array);
      return new EcmaIterator(thisValue, EcmaIteratorResultKind.Entry, WellKnownObject.ArrayIteratorPrototype);
    }

    [IntrinsicMember]
    public static EcmaValue Keys([This] EcmaValue thisValue) {
      TypedArray array = thisValue.GetUnderlyingObject<TypedArray>();
      Guard.BufferNotDetached(array);
      return new EcmaIterator(thisValue, EcmaIteratorResultKind.Key, WellKnownObject.ArrayIteratorPrototype);
    }

    [IntrinsicMember]
    [IntrinsicMember(WellKnownSymbol.Iterator)]
    public static EcmaValue Values([This] EcmaValue thisValue) {
      TypedArray array = thisValue.GetUnderlyingObject<TypedArray>();
      Guard.BufferNotDetached(array);
      return new EcmaIterator(thisValue, EcmaIteratorResultKind.Value, WellKnownObject.ArrayIteratorPrototype);
    }

    [IntrinsicMember(FunctionLength = 2)]
    public static EcmaValue CopyWithin([This] EcmaValue thisValue, EcmaValue target, EcmaValue start, EcmaValue end) {
      TypedArray array = thisValue.GetUnderlyingObject<TypedArray>();
      Guard.BufferNotDetached(array);
      int len = array.Length;
      int to = ArrayHelper.GetBoundIndex(target, len, 0);
      int from = ArrayHelper.GetBoundIndex(start, len, 0);
      int until = ArrayHelper.GetBoundIndex(end, len, len);
      int count = Math.Min(until - from, len - to);
      if (count > 0) {
        int elementSize = array.ElementSize;
        int srcOffset = (int)(from * elementSize + array.ByteOffset);
        int dstOffset = (int)(to * elementSize + array.ByteOffset);
        Guard.BufferNotDetached(array);
        ArrayBuffer.CopyBytes(array.Buffer, srcOffset, array.Buffer, dstOffset, (int)count * elementSize);
      }
      return thisValue;
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue Every([This] EcmaValue thisValue, EcmaValue callback, EcmaValue thisArg) {
      TypedArray array = thisValue.GetUnderlyingObject<TypedArray>();
      Guard.BufferNotDetached(array);
      Guard.ArgumentIsCallable(callback);
      for (int i = 0, len = array.Length; i < len; i++) {
        Guard.BufferNotDetached(array);
        EcmaValue value = array.GetValueFromBuffer(i);
        if (!(bool)callback.Call(thisArg, value, i, thisValue)) {
          return false;
        }
      }
      return true;
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue Fill([This] EcmaValue thisValue, EcmaValue value, EcmaValue start, EcmaValue end) {
      TypedArray array = thisValue.GetUnderlyingObject<TypedArray>();
      Guard.BufferNotDetached(array);
      value = value.ToNumber();
      int len = array.Length;
      int from = ArrayHelper.GetBoundIndex(start, len, 0);
      int until = ArrayHelper.GetBoundIndex(end, len, len);
      Guard.BufferNotDetached(array);
      for (int i = from; i < until; i++) {
        array.SetOrThrow(i, value);
      }
      return thisValue;
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue Filter([This] EcmaValue thisValue, EcmaValue callback, EcmaValue thisArg) {
      TypedArray array = thisValue.GetUnderlyingObject<TypedArray>();
      Guard.BufferNotDetached(array);
      Guard.ArgumentIsCallable(callback);
      List<EcmaValue> filtered = new List<EcmaValue>();
      for (int i = 0, length = array.Length; i < length; i++) {
        EcmaValue value = array.GetValueFromBuffer(i);
        if (callback.Call(thisArg, value, i, thisValue)) {
          filtered.Add(value);
        }
      }
      TypedArray target = TypedArray.SpeciesCreate(array, filtered.Count);
      for (int i = 0; i < filtered.Count; i++) {
        target.SetOrThrow(i, filtered[i]);
      }
      return target;
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue Find([This] EcmaValue thisValue, EcmaValue predicate, EcmaValue thisArg) {
      TypedArray array = thisValue.GetUnderlyingObject<TypedArray>();
      Guard.BufferNotDetached(array);
      Guard.ArgumentIsCallable(predicate);
      for (int i = 0, length = array.Length; i < length; i++) {
        EcmaValue value = array.GetValueFromBuffer(i);
        if (predicate.Call(thisArg, value, i, thisValue)) {
          return value;
        }
      }
      return default;
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue FindIndex([This] EcmaValue thisValue, EcmaValue predicate, EcmaValue thisArg) {
      TypedArray array = thisValue.GetUnderlyingObject<TypedArray>();
      Guard.BufferNotDetached(array);
      Guard.ArgumentIsCallable(predicate);
      for (int i = 0, length = array.Length; i < length; i++) {
        if (predicate.Call(thisArg, array.GetValueFromBuffer(i), i, thisValue)) {
          return i;
        }
      }
      return -1;
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue ForEach([This] EcmaValue thisValue, EcmaValue callback, EcmaValue thisArg) {
      TypedArray array = thisValue.GetUnderlyingObject<TypedArray>();
      Guard.BufferNotDetached(array);
      Guard.ArgumentIsCallable(callback);
      for (int i = 0, length = array.Length; i < length; i++) {
        callback.Call(thisArg, array.GetValueFromBuffer(i), i, thisValue);
      }
      return default;
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue Includes([This] EcmaValue thisValue, EcmaValue searchElement, EcmaValue? fromIndex) {
      TypedArray array = thisValue.GetUnderlyingObject<TypedArray>();
      Guard.BufferNotDetached(array);
      int length = array.Length;
      if (length == 0) {
        return false;
      }
      int from = 0;
      if (fromIndex.HasValue) {
        EcmaValue fromValue = fromIndex.Value.ToNumber();
        if (fromValue >= length) {
          return false;
        }
        from = fromValue.ToInt32();
        if (from < 0) {
          from = Math.Max(0, from + length);
        }
      }
      for (int i = from; i < length; i++) {
        if (array.GetValueFromBuffer(i).Equals(searchElement, EcmaValueComparison.SameValueZero)) {
          return true;
        }
      }
      return false;
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue IndexOf([This] EcmaValue thisValue, EcmaValue searchElement, EcmaValue? fromIndex) {
      TypedArray array = thisValue.GetUnderlyingObject<TypedArray>();
      Guard.BufferNotDetached(array);
      int length = array.Length;
      if (length == 0) {
        return -1;
      }
      int from = 0;
      if (fromIndex.HasValue) {
        EcmaValue fromValue = fromIndex.Value.ToNumber();
        if (fromValue >= length) {
          return -1;
        }
        from = fromValue.ToInt32();
        if (from < 0) {
          from = Math.Max(0, from + length);
        }
      }
      for (int i = from; i < length; i++) {
        if (array.GetValueFromBuffer(i).Equals(searchElement, EcmaValueComparison.Strict)) {
          return i;
        }
      }
      return -1;
    }

    [IntrinsicMember]
    public static EcmaValue Join([This] EcmaValue thisValue, EcmaValue separater) {
      TypedArray array = thisValue.GetUnderlyingObject<TypedArray>();
      Guard.BufferNotDetached(array);
      string sep = separater.Type == EcmaValueType.Undefined ? "," : separater.ToStringOrThrow();
      int length = array.Length;
      if (length == 0) {
        return String.Empty;
      }
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < length; i++) {
        if (i > 0) {
          sb.Append(sep);
        }
        EcmaValue item = array.GetValueFromBuffer(i);
        sb.Append(item.ToStringOrThrow());
      }
      return sb.ToString();
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue LastIndexOf([This] EcmaValue thisValue, EcmaValue searchElement, EcmaValue? fromIndex) {
      TypedArray array = thisValue.GetUnderlyingObject<TypedArray>();
      Guard.BufferNotDetached(array);
      int length = array.Length;
      if (length == 0) {
        return -1;
      }
      int from = length - 1;
      if (fromIndex.HasValue) {
        EcmaValue fromValue = fromIndex.Value.ToNumber();
        if (fromValue == EcmaValue.NegativeInfinity) {
          return -1;
        }
        if (fromValue >= length) {
          from = length - 1;
        } else {
          from = fromValue.ToInt32();
          if (from < 0) {
            from += length;
          }
        }
      }
      for (int i = from; i >= 0; i--) {
        if (array.GetValueFromBuffer(i).Equals(searchElement, EcmaValueComparison.Strict)) {
          return i;
        }
      }
      return -1;
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue Map([This] EcmaValue thisValue, EcmaValue callback, EcmaValue thisArg) {
      TypedArray array = thisValue.GetUnderlyingObject<TypedArray>();
      Guard.BufferNotDetached(array);
      Guard.ArgumentIsCallable(callback);
      int len = array.Length;
      RuntimeObject target = TypedArray.SpeciesCreate(array, len);
      for (int i = 0; i < len; i++) {
        EcmaValue value = array.GetValueFromBuffer(i);
        target.SetOrThrow(i, callback.Call(thisArg, value, i, thisValue));
      }
      return target;
    }

    [IntrinsicMember]
    public static EcmaValue Reverse([This] EcmaValue thisValue) {
      TypedArray array = thisValue.GetUnderlyingObject<TypedArray>();
      Guard.BufferNotDetached(array);
      int len = array.Length;
      int middle = len >> 1;
      int lower = 0;
      int upper = len - 1;
      while (lower != middle) {
        EcmaValue lowerValue = array.GetValueFromBuffer(lower);
        EcmaValue upperValue = array.GetValueFromBuffer(upper);
        array.SetOrThrow(lower, upperValue);
        array.SetOrThrow(upper, lowerValue);
        lower++;
        upper--;
      }
      return thisValue;
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue Reduce([This] EcmaValue thisValue, EcmaValue callback, EcmaValue? initialValue) {
      TypedArray array = thisValue.GetUnderlyingObject<TypedArray>();
      Guard.BufferNotDetached(array);
      Guard.ArgumentIsCallable(callback);
      int length = array.Length;
      if (length == 0 && !initialValue.HasValue) {
        throw new EcmaTypeErrorException(InternalString.Error.ReduceEmptyArray);
      }
      int i = 0;
      EcmaValue value = initialValue.HasValue ? initialValue.Value : array.GetValueFromBuffer(i++);
      for (; i < length; i++) {
        value = callback.Call(EcmaValue.Undefined, value, array.GetValueFromBuffer(i), i, thisValue);
      }
      return value;
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue ReduceRight([This] EcmaValue thisValue, EcmaValue callback, EcmaValue? initialValue) {
      TypedArray array = thisValue.GetUnderlyingObject<TypedArray>();
      Guard.BufferNotDetached(array);
      Guard.ArgumentIsCallable(callback);
      int length = array.Length;
      if (length == 0 && !initialValue.HasValue) {
        throw new EcmaTypeErrorException(InternalString.Error.ReduceEmptyArray);
      }
      int i = length - 1;
      EcmaValue value = initialValue.HasValue ? initialValue.Value : array.GetValueFromBuffer(i--);
      for (; i >= 0; i--) {
        value = callback.Call(EcmaValue.Undefined, value, array.GetValueFromBuffer(i), i, thisValue);
      }
      return value;
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue Set([This] EcmaValue thisValue, EcmaValue source, EcmaValue offset) {
      TypedArray array = thisValue.GetUnderlyingObject<TypedArray>();
      Guard.BufferNotDetached(array);
      offset = offset.ToInteger();
      if (offset < 0) {
        throw new EcmaRangeErrorException(InternalString.Error.BufferOffsetOutOfBound, offset);
      }
      int targetOffset = offset.ToInt32();
      Guard.BufferNotDetached(array);
      if (source.GetUnderlyingObject() is TypedArray srcArray) {
        Guard.BufferNotDetached(srcArray);
        if (srcArray.Length + offset > array.Length) {
          throw new EcmaRangeErrorException(InternalString.Error.SourceTooLarge);
        }
        long srcByteIndex = srcArray.ByteOffset;
        long dstByteIndex = array.GetByteOffset(targetOffset);
        ArrayBuffer srcBuffer = srcArray.Buffer;
        ArrayBuffer dstBuffer = array.Buffer;
        if (srcBuffer == dstBuffer) {
          srcBuffer = srcBuffer.Clone(srcByteIndex, srcArray.ByteLength, srcArray.Realm.GetRuntimeObject(WellKnownObject.ArrayBuffer));
          srcByteIndex = 0;
        }
        if (array.ArrayKind == srcArray.ArrayKind) {
          ArrayBuffer.CopyBytes(srcBuffer, srcByteIndex, dstBuffer, dstByteIndex, array.ElementSize * srcArray.Length);
        } else {
          for (int i = targetOffset, j = 0, until = srcArray.Length; j < until; i++, j++) {
            array.SetValueInBuffer(i, srcArray.GetValueFromBuffer(j));
          }
        }
      } else {
        RuntimeObject srcObj = source.ToObject();
        long count = srcObj[WellKnownProperty.Length].ToLength();
        if (count + offset > array.Length) {
          throw new EcmaRangeErrorException(InternalString.Error.SourceTooLarge);
        }
        long index = 0;
        for (int i = targetOffset; index < count; i++, index++) {
          Guard.BufferNotDetached(array);
          array.SetValueInBuffer(i, srcObj[index].ToNumber());
        }
      }
      return default;
    }

    [IntrinsicMember]
    public static EcmaValue Slice([This] EcmaValue thisValue, EcmaValue start, EcmaValue end) {
      TypedArray array = thisValue.GetUnderlyingObject<TypedArray>();
      Guard.BufferNotDetached(array);
      int len = array.Length;
      int from = ArrayHelper.GetBoundIndex(start, len, 0);
      int until = ArrayHelper.GetBoundIndex(end, len, len);
      int count = Math.Max(until - from, 0);
      TypedArray target = TypedArray.SpeciesCreate(array, count);
      if (target.ArrayKind != array.ArrayKind) {
        for (int i = 0; i < count; i++, from++) {
          target.SetOrThrow(i, array.GetValueFromBuffer(from));
        }
      } else if (count > 0) {
        Guard.BufferNotDetached(array);
        ArrayBuffer.CopyBytes(array.Buffer, array.ByteOffset + from * array.ElementSize, target.Buffer, target.ByteOffset, count * array.ElementSize);
      }
      return target;
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue Some([This] EcmaValue thisValue, EcmaValue callback, EcmaValue thisArg) {
      TypedArray array = thisValue.GetUnderlyingObject<TypedArray>();
      Guard.BufferNotDetached(array);
      Guard.ArgumentIsCallable(callback);
      for (int i = 0, len = array.Length; i < len; i++) {
        EcmaValue value = array.GetValueFromBuffer(i);
        if (callback.Call(thisArg, value, i, thisValue)) {
          return true;
        }
      }
      return false;
    }

    [IntrinsicMember]
    public static EcmaValue Sort([This] EcmaValue thisValue, EcmaValue callback) {
      TypedArray array = thisValue.GetUnderlyingObject<TypedArray>();
      Guard.BufferNotDetached(array);
      if (callback != default) {
        Guard.ArgumentIsCallable(callback);
        array.Sort(callback.ToObject());
      } else {
        array.Sort();
      }
      return thisValue;
    }

    [IntrinsicMember]
    public static EcmaValue Subarray([This] EcmaValue thisValue, EcmaValue start, EcmaValue end) {
      TypedArray array = thisValue.GetUnderlyingObject<TypedArray>();
      int len = array.Length;
      int from = ArrayHelper.GetBoundIndex(start, len, 0);
      int until = ArrayHelper.GetBoundIndex(end, len, len);
      int count = Math.Max(until - from, 0);
      return TypedArray.SpeciesCreate(array, array.Buffer, array.GetByteOffset(from), count);
    }

    [IntrinsicMember]
    public static EcmaValue ToLocaleString([This] EcmaValue thisValue) {
      TypedArray array = thisValue.GetUnderlyingObject<TypedArray>();
      Guard.BufferNotDetached(array);
      long length = array.Length;
      if (length == 0) {
        return String.Empty;
      }
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < length; i++) {
        if (i > 0) {
          sb.Append(",");
        }
        EcmaValue item = array.GetValueFromBuffer(i);
        sb.Append(item.Invoke("toLocaleString").ToStringOrThrow());
      }
      return sb.ToString();
    }

    [IntrinsicMember]
    [AliasOf(WellKnownObject.ArrayPrototype, "toString")]
    public static EcmaValue ToString([This] EcmaValue thisValue) {
      throw new NotImplementedException();
    }
  }

  #region Derived prototype objects
  [IntrinsicObject(WellKnownObject.Float32ArrayPrototype, Prototype = WellKnownObject.TypedArrayPrototype)]
  internal static class Float32TypedArrayPrototype {
    [IntrinsicMember("BYTES_PER_ELEMENT", EcmaPropertyAttributes.None)]
    public const int BytesPerElement = sizeof(float);
  }

  [IntrinsicObject(WellKnownObject.Float64ArrayPrototype, Prototype = WellKnownObject.TypedArrayPrototype)]
  internal static class Float64TypedArrayPrototype {
    [IntrinsicMember("BYTES_PER_ELEMENT", EcmaPropertyAttributes.None)]
    public const int BytesPerElement = sizeof(double);
  }

  [IntrinsicObject(WellKnownObject.Int8ArrayPrototype, Prototype = WellKnownObject.TypedArrayPrototype)]
  internal static class Int8ArrayPrototype {
    [IntrinsicMember("BYTES_PER_ELEMENT", EcmaPropertyAttributes.None)]
    public const int BytesPerElement = sizeof(sbyte);
  }

  [IntrinsicObject(WellKnownObject.Int16ArrayPrototype, Prototype = WellKnownObject.TypedArrayPrototype)]
  internal static class Int16ArrayPrototype {
    [IntrinsicMember("BYTES_PER_ELEMENT", EcmaPropertyAttributes.None)]
    public const int BytesPerElement = sizeof(short);
  }

  [IntrinsicObject(WellKnownObject.Int32ArrayPrototype, Prototype = WellKnownObject.TypedArrayPrototype)]
  internal static class Int32ArrayPrototype {
    [IntrinsicMember("BYTES_PER_ELEMENT", EcmaPropertyAttributes.None)]
    public const int BytesPerElement = sizeof(int);
  }

  [IntrinsicObject(WellKnownObject.Uint8ArrayPrototype, Prototype = WellKnownObject.TypedArrayPrototype)]
  internal static class Uint8ArrayPrototype {
    [IntrinsicMember("BYTES_PER_ELEMENT", EcmaPropertyAttributes.None)]
    public const int BytesPerElement = sizeof(byte);
  }

  [IntrinsicObject(WellKnownObject.Uint8ClampedArrayPrototype, Prototype = WellKnownObject.TypedArrayPrototype)]
  internal static class Uint8ClampedArrayPrototype {
    [IntrinsicMember("BYTES_PER_ELEMENT", EcmaPropertyAttributes.None)]
    public const int BytesPerElement = sizeof(byte);
  }

  [IntrinsicObject(WellKnownObject.Uint16ArrayPrototype, Prototype = WellKnownObject.TypedArrayPrototype)]
  internal static class Uint16ArrayPrototype {
    [IntrinsicMember("BYTES_PER_ELEMENT", EcmaPropertyAttributes.None)]
    public const int BytesPerElement = sizeof(ushort);
  }

  [IntrinsicObject(WellKnownObject.Uint32ArrayPrototype, Prototype = WellKnownObject.TypedArrayPrototype)]
  internal static class Uint32ArrayPrototype {
    [IntrinsicMember("BYTES_PER_ELEMENT", EcmaPropertyAttributes.None)]
    public const int BytesPerElement = sizeof(uint);
  }
  #endregion
}
