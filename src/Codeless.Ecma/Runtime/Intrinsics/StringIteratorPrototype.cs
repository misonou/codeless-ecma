namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.StringIteratorPrototype, Prototype = WellKnownObject.IteratorPrototype)]
  internal static class StringIteratorPrototype {
    [IntrinsicMember(WellKnownSymbol.ToStringTag, EcmaPropertyAttributes.Configurable)]
    public static string ToStringTag = InternalString.ObjectTag.StringIterator;

    [IntrinsicMember]
    public static EcmaValue Next([This] EcmaValue thisValue) {
      EcmaStringEnumerator iterator = thisValue.GetUnderlyingObject<EcmaStringEnumerator>();
      return iterator.Next();
    }
  }
}
