using System;
using System.Collections.Generic;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.ArrayConstructor)]
  internal static class ArrayConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.AlwaysConstruct, ObjectType = typeof(EcmaArray))]
    public static EcmaValue Array([This] EcmaValue thisValue, params EcmaValue[] args) {
      EcmaArray array = thisValue.GetUnderlyingObject<EcmaArray>();
      if (args.Length == 1 && args[0].Type == EcmaValueType.Number) {
        uint length = args[0].ToUInt32();
        if (args[0] != length) {
          throw new EcmaRangeErrorException(InternalString.Error.InvalidArrayLength);
        }
        array.Length = length;
      } else {
        array.Init(args);
      }
      return thisValue;
    }

    [IntrinsicMember(WellKnownSymbol.Species, Getter = true)]
    public static EcmaValue Species([This] EcmaValue thisValue) {
      return thisValue;
    }

    [IntrinsicMember]
    public static EcmaValue IsArray(EcmaValue value) {
      return EcmaArray.IsArray(value);
    }

    [IntrinsicMember(FunctionLength = 0)]
    public static EcmaValue Of([This] EcmaValue thisValue, params EcmaValue[] elements) {
      if (!thisValue.IsCallable || !thisValue.ToObject().IsConstructor) {
        return new EcmaArray(elements);
      }
      RuntimeObject arr = thisValue.Construct(elements.Length).ToObject();
      for (long i = 0, len = elements.Length; i < len; i++) {
        arr.CreateDataPropertyOrThrow(i, elements[i]);
      }
      arr.SetOrThrow(WellKnownProperty.Length, elements.Length);
      return arr;
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue From([This] EcmaValue thisValue, EcmaValue arrayLike, EcmaValue mapFn, EcmaValue thisArg) {
      RuntimeObject items = arrayLike.ToObject();
      if (mapFn != default) {
        Guard.ArgumentIsCallable(mapFn);
      }
      bool usingIterator = items.GetMethod(WellKnownSymbol.Iterator) != null;
      long initialLen = usingIterator ? 0 : arrayLike[WellKnownProperty.Length].ToLength();
      RuntimeObject arr;
      if (thisValue.IsCallable && thisValue.ToObject().IsConstructor) {
        arr = thisValue.Construct(usingIterator ? EcmaValue.EmptyArray : new EcmaValue[] { initialLen }).ToObject();
      } else {
        arr = new EcmaArray(initialLen);
      }

      if (usingIterator) {
        foreach (EcmaValue value in items.GetIterator()) {
          ArrayPrototype.ThrowIfLengthExceeded(initialLen + 1);
          EcmaValue value1 = value;
          if (mapFn != default) {
            value1 = mapFn.Call(thisArg, value, initialLen);
          }
          arr.CreateDataPropertyOrThrow(initialLen++, value1);
        }
      } else {
        for (long i = 0; i < initialLen; i++) {
          EcmaValue value = items[i];
          if (mapFn != default) {
            value = mapFn.Call(thisArg, value, i);
          }
          arr.CreateDataPropertyOrThrow(i, value);
        }
      }
      arr.SetOrThrow(WellKnownProperty.Length, initialLen);
      return arr;
    }
  }
}
