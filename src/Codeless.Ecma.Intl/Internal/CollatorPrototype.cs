using Codeless.Ecma.Runtime;
using System;

namespace Codeless.Ecma.Intl.Internal {
  [IntrinsicObject(IntlObjectKind.CollatorPrototype)]
  internal static class CollatorPrototype {
    [IntrinsicMember(WellKnownSymbol.ToStringTag, EcmaPropertyAttributes.Configurable)]
    public const string ToStringTag = "Object";

    [IntrinsicMember(Getter = true)]
    public static EcmaValue Compare([This] EcmaValue thisValue) {
      Collator collator = thisValue.GetUnderlyingObject<Collator>();
      return collator.BoundCompare;
    }

    [IntrinsicMember]
    public static EcmaValue ResolvedOptions([This] EcmaValue thisValue) {
      Collator collator = thisValue.GetUnderlyingObject<Collator>();
      EcmaObject obj = new EcmaObject();
      obj.CreateDataPropertyOrThrow(PropertyKey.Locale, collator.Locale);
      obj.CreateDataPropertyOrThrow(PropertyKey.Usage, IntlProviderOptions.ToStringValue(collator.Usage));
      obj.CreateDataPropertyOrThrow(PropertyKey.Sensitivity, IntlProviderOptions.ToStringValue(collator.Sensitivity));
      obj.CreateDataPropertyOrThrow(PropertyKey.IgnorePunctuation, collator.IgnorePunctuation);
      obj.CreateDataPropertyOrThrow(PropertyKey.Collation, collator.Collation);
      obj.CreateDataPropertyOrThrow(PropertyKey.Numeric, collator.Numeric);
      obj.CreateDataPropertyOrThrow(PropertyKey.CaseFirst, IntlProviderOptions.ToStringValue(collator.CaseFirst));
      return obj;
    }
  }
}
