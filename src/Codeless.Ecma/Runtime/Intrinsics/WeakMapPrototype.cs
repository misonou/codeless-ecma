using System;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.WeakMapPrototype)]
  internal static class WeakMapPrototype {
    [IntrinsicMember(WellKnownSymbol.ToStringTag, EcmaPropertyAttributes.Configurable)]
    public const string ToStringTag = InternalString.ObjectTag.WeakMap;

    [IntrinsicMember]
    public static EcmaValue Delete([This] EcmaValue thisValue, EcmaValue key) {
      EcmaWeakMap map = thisValue.GetUnderlyingObject<EcmaWeakMap>();
      if (key.Type != EcmaValueType.Object) {
        return false;
      }
      return map.Delete(key.ToObject());
    }

    [IntrinsicMember]
    public static EcmaValue Get([This] EcmaValue thisValue, EcmaValue key) {
      EcmaWeakMap map = thisValue.GetUnderlyingObject<EcmaWeakMap>();
      if (key.Type != EcmaValueType.Object) {
        return default;
      }
      return map.Get(key.ToObject());
    }

    [IntrinsicMember]
    public static EcmaValue Has([This] EcmaValue thisValue, EcmaValue key) {
      EcmaWeakMap map = thisValue.GetUnderlyingObject<EcmaWeakMap>();
      if (key.Type != EcmaValueType.Object) {
        return false;
      }
      return map.Has(key.ToObject());
    }

    [IntrinsicMember]
    public static EcmaValue Set([This] EcmaValue thisValue, EcmaValue key, EcmaValue value) {
      EcmaWeakMap map = thisValue.GetUnderlyingObject<EcmaWeakMap>();
      Guard.ArgumentIsObject(key, "Invalid value used as weak map key");
      map.Set(key.ToObject(), value);
      return thisValue;
    }
  }
}
