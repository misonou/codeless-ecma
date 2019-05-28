namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.RegExpStringIteratorPrototype)]
  internal static class RegExpStringIteratorPrototype {
    [IntrinsicMember(WellKnownSymbol.ToStringTag, EcmaPropertyAttributes.Configurable)]
    public static string ToStringTag = InternalString.ObjectTag.RegExpStringIterator;

    [IntrinsicMember]
    public static EcmaValue Next([This] EcmaValue thisValue) {
      EcmaIterator iterator = thisValue.GetUnderlyingObject<EcmaIterator>();
      return iterator.Next();
    }
  }
}
