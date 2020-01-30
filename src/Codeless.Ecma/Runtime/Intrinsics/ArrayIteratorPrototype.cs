using System;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.ArrayIteratorPrototype, Prototype = WellKnownObject.IteratorPrototype)]
  internal static class ArrayIteratorPrototype {
    [IntrinsicMember(WellKnownSymbol.ToStringTag, EcmaPropertyAttributes.Configurable)]
    public static string ToStringTag = InternalString.ObjectTag.ArrayIterator;

    [IntrinsicMember]
    public static EcmaValue Next([This] EcmaValue thisValue) {
      EcmaArrayIterator iterator = thisValue.GetUnderlyingObject<EcmaArrayIterator>();
      return iterator.Next();
    }
  }
}
