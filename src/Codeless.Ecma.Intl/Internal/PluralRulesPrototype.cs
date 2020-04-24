using Codeless.Ecma.Intl.Utilities;
using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Codeless.Ecma.Intl.Internal {
  [IntrinsicObject(IntlObjectKind.PluralRulesPrototype)]
  internal static class PluralRulesPrototype {
    [IntrinsicMember(WellKnownSymbol.ToStringTag, EcmaPropertyAttributes.Configurable)]
    public const string ToStringTag = "Object";

    [IntrinsicMember]
    public static EcmaValue Select([This] EcmaValue thisValue, EcmaValue value) {
      PluralRules rules = thisValue.GetUnderlyingObject<PluralRules>();
      return rules.Select(value);
    }

    [IntrinsicMember]
    public static EcmaValue ResolvedOptions([This] EcmaValue thisValue) {
      PluralRules rules = thisValue.GetUnderlyingObject<PluralRules>();
      List<EcmaValue> categories = new List<EcmaValue>();
      foreach (KeyValuePair<string, PluralCategories> e in StringValueMap<PluralCategories>.Default.Forward) {
        if ((rules.PluralCategories & e.Value) != 0){
          categories.Add(e.Key);
        }
      }
      EcmaObject obj = new EcmaObject();
      obj.CreateDataPropertyOrThrow(PropertyKey.Locale, rules.Locale);
      obj.CreateDataPropertyOrThrow(PropertyKey.PluralCategories, new EcmaArray(categories));
      obj.CreateDataPropertyOrThrow(PropertyKey.Type, IntlProviderOptions.ToStringValue(rules.Type));
      if (rules.Digits.UseSignificantDigits) {
        obj.CreateDataPropertyOrThrow(PropertyKey.MinimumSignificantDigits, rules.Digits.MinimumSignificantDigits);
        obj.CreateDataPropertyOrThrow(PropertyKey.MaximumSignificantDigits, rules.Digits.MaximumSignificantDigits);
      } else {
        obj.CreateDataPropertyOrThrow(PropertyKey.MinimumIntegerDigits, rules.Digits.MinimumSignificantDigits);
        obj.CreateDataPropertyOrThrow(PropertyKey.MinimumFractionDigits, rules.Digits.MinimumFractionDigits);
        obj.CreateDataPropertyOrThrow(PropertyKey.MaximumFractionDigits, rules.Digits.MaximumFractionDigits);
      }
      return obj;
    }
  }
}
