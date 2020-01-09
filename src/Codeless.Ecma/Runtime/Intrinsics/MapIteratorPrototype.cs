namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.MapIteratorPrototype, Prototype = WellKnownObject.IteratorPrototype)]
  internal static class MapIteratorPrototype {
    [IntrinsicMember(WellKnownSymbol.ToStringTag, EcmaPropertyAttributes.Configurable)]
    public static string ToStringTag = InternalString.ObjectTag.MapIterator;

    [IntrinsicMember]
    public static EcmaValue Next([This] EcmaValue thisValue) {
      EcmaIterator iterator = thisValue.GetUnderlyingObject<EcmaIterator>();
      return iterator.Next();
    }
  }
}
