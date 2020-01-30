namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.SetIteratorPrototype, Prototype = WellKnownObject.IteratorPrototype)]
  internal static class SetIteratorPrototype {
    [IntrinsicMember(WellKnownSymbol.ToStringTag, EcmaPropertyAttributes.Configurable)]
    public static string ToStringTag = InternalString.ObjectTag.SetIterator;

    [IntrinsicMember]
    public static EcmaValue Next([This] EcmaValue thisValue) {
      EcmaSetIterator iterator = thisValue.GetUnderlyingObject<EcmaSetIterator>();
      return iterator.Next();
    }
  }
}
