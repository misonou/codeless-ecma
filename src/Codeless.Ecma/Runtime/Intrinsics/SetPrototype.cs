using System;
using System.Collections.Generic;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.SetPrototype)]
  internal static class SetPrototype {
    [IntrinsicMember(WellKnownSymbol.ToStringTag, EcmaPropertyAttributes.Configurable)]
    public const string ToStringTag = InternalString.ObjectTag.Set;

    [IntrinsicMember]
    public static EcmaValue Add([This] EcmaValue thisValue, EcmaValue value) {
      EcmaSet map = thisValue.GetUnderlyingObject<EcmaSet>();
      map.Add(value);
      return thisValue;
    }

    [IntrinsicMember]
    public static EcmaValue Clear([This] EcmaValue thisValue) {
      EcmaSet map = thisValue.GetUnderlyingObject<EcmaSet>();
      map.Clear();
      return default;
    }

    [IntrinsicMember]
    public static EcmaValue Delete([This] EcmaValue thisValue, EcmaValue value) {
      EcmaSet map = thisValue.GetUnderlyingObject<EcmaSet>();
      return map.Delete(value);
    }

    [IntrinsicMember]
    public static EcmaValue Entries([This] EcmaValue thisValue) {
      EcmaSet map = thisValue.GetUnderlyingObject<EcmaSet>();
      return map.Entries();
    }

    [IntrinsicMember]
    public static EcmaValue ForEach([This] EcmaValue thisValue, EcmaValue callback, EcmaValue thisArg) {
      EcmaSet map = thisValue.GetUnderlyingObject<EcmaSet>();
      Guard.ArgumentIsCallable(callback);
      IEnumerator<KeyValuePair<EcmaValue, EcmaValue>> iterator = map.GetEnumerator();
      while (iterator.MoveNext()) {
        KeyValuePair<EcmaValue, EcmaValue> entry = iterator.Current;
        callback.Call(thisArg, entry.Value, entry.Key);
      }
      return default;
    }

    [IntrinsicMember]
    public static EcmaValue Has([This] EcmaValue thisValue, EcmaValue value) {
      EcmaSet map = thisValue.GetUnderlyingObject<EcmaSet>();
      return map.Has(value);
    }

    [IntrinsicMember]
    public static EcmaValue Keys([This] EcmaValue thisValue) {
      EcmaSet map = thisValue.GetUnderlyingObject<EcmaSet>();
      return map.Keys();
    }

    [IntrinsicMember(Getter = true)]
    public static EcmaValue Size([This] EcmaValue thisValue) {
      EcmaSet map = thisValue.GetUnderlyingObject<EcmaSet>();
      return map.Size;
    }

    [IntrinsicMember]
    [IntrinsicMember(WellKnownSymbol.Iterator)]
    public static EcmaValue Values([This] EcmaValue thisValue) {
      EcmaSet map = thisValue.GetUnderlyingObject<EcmaSet>();
      return map.Values();
    }
  }
}
