using System;
using System.Collections.Generic;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.MapPrototype)]
  internal static class MapPrototype {
    [IntrinsicMember(WellKnownSymbol.ToStringTag, EcmaPropertyAttributes.Configurable)]
    public const string ToStringTag = InternalString.ObjectTag.Map;

    [IntrinsicMember]
    public static EcmaValue Clear([This] EcmaValue thisValue) {
      EcmaMap map = thisValue.GetUnderlyingObject<EcmaMap>();
      map.Clear();
      return default;
    }

    [IntrinsicMember]
    public static EcmaValue Delete([This] EcmaValue thisValue, EcmaValue key) {
      EcmaMap map = thisValue.GetUnderlyingObject<EcmaMap>();
      return map.Delete(key);
    }

    [IntrinsicMember]
    [IntrinsicMember(WellKnownSymbol.Iterator)]
    public static EcmaValue Entries([This] EcmaValue thisValue) {
      EcmaMap map = thisValue.GetUnderlyingObject<EcmaMap>();
      return map.Entries();
    }

    [IntrinsicMember]
    public static EcmaValue ForEach([This] EcmaValue thisValue, EcmaValue callback, EcmaValue thisArg) {
      EcmaMap map = thisValue.GetUnderlyingObject<EcmaMap>();
      Guard.ArgumentIsCallable(callback);
      IEnumerator<KeyValuePair<EcmaValue, EcmaValue>> iterator = map.GetEnumerator();
      while (iterator.MoveNext()) {
        KeyValuePair<EcmaValue, EcmaValue> entry = iterator.Current;
        callback.Call(thisArg, entry.Value, entry.Key);
      }
      return default;
    }

    [IntrinsicMember]
    public static EcmaValue Get([This] EcmaValue thisValue, EcmaValue key) {
      EcmaMap map = thisValue.GetUnderlyingObject<EcmaMap>();
      return map.Get(key);
    }

    [IntrinsicMember]
    public static EcmaValue Has([This] EcmaValue thisValue, EcmaValue key) {
      EcmaMap map = thisValue.GetUnderlyingObject<EcmaMap>();
      return map.Has(key);
    }

    [IntrinsicMember]
    public static EcmaValue Keys([This] EcmaValue thisValue) {
      EcmaMap map = thisValue.GetUnderlyingObject<EcmaMap>();
      return map.Keys();
    }

    [IntrinsicMember]
    public static EcmaValue Set([This] EcmaValue thisValue, EcmaValue key, EcmaValue value) {
      EcmaMap map = thisValue.GetUnderlyingObject<EcmaMap>();
      map.Set(key, value);
      return thisValue;
    }

    [IntrinsicMember(EcmaPropertyAttributes.Configurable, Getter = true)]
    public static EcmaValue Size([This] EcmaValue thisValue) {
      EcmaMap map = thisValue.GetUnderlyingObject<EcmaMap>();
      return map.Size;
    }

    [IntrinsicMember]
    public static EcmaValue Values([This] EcmaValue thisValue) {
      EcmaMap map = thisValue.GetUnderlyingObject<EcmaMap>();
      return map.Values();
    }
  }
}
