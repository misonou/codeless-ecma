namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.RegExpStringIteratorPrototype, Prototype = WellKnownObject.IteratorPrototype)]
  internal static class RegExpStringIteratorPrototype {
    [IntrinsicMember(WellKnownSymbol.ToStringTag, EcmaPropertyAttributes.Configurable)]
    public static string ToStringTag = InternalString.ObjectTag.RegExpStringIterator;

    [IntrinsicMember]
    public static EcmaValue Next([This] EcmaValue thisValue) {
      EcmaRegExpStringEnumerator iterator = thisValue.GetUnderlyingObject<EcmaRegExpStringEnumerator>();
      return iterator.Next();
    }
  }
}
