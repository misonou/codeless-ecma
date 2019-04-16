using System;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.WeakSetPrototype)]
  internal static class WeakSetPrototype {
    [IntrinsicMember(WellKnownSymbol.ToStringTag, EcmaPropertyAttributes.Configurable)]
    public const string ToStringTag = InternalString.ObjectTag.WeakSet;

    [IntrinsicMember]
    public static EcmaValue Add([This] EcmaValue thisValue, EcmaValue item) {
      EcmaWeakSet map = thisValue.GetUnderlyingObject<EcmaWeakSet>();
      Guard.ArgumentIsObject(item, "Invalid value used in weak set");
      map.Add(item.ToObject());
      return thisValue;
    }

    [IntrinsicMember]
    public static EcmaValue Delete([This] EcmaValue thisValue, EcmaValue item) {
      EcmaWeakSet map = thisValue.GetUnderlyingObject<EcmaWeakSet>();
      if (item.Type != EcmaValueType.Object) {
        return false;
      }
      return map.Delete(item.ToObject());
    }

    [IntrinsicMember]
    public static EcmaValue Has([This] EcmaValue thisValue, EcmaValue item) {
      EcmaWeakSet map = thisValue.GetUnderlyingObject<EcmaWeakSet>();
      if (item.Type != EcmaValueType.Object) {
        return false;
      }
      return map.Has(item.ToObject());
    }
  }
}
