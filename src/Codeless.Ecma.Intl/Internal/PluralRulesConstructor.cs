using Codeless.Ecma.Intl.Utilities;
using Codeless.Ecma.Runtime;
using System.Collections.Generic;
using System.Linq;

namespace Codeless.Ecma.Intl.Internal {
  [IntrinsicObject(IntlObjectKind.PluralRules)]
  internal static class PluralRulesConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.DenyCall, Global = false, ObjectType = typeof(PluralRules), Prototype = IntlObjectKind.PluralRulesPrototype)]
    public static EcmaValue PluralRules([This] EcmaValue thisValue, EcmaValue locales, EcmaValue options) {
      PluralRules rules = thisValue.GetUnderlyingObject<PluralRules>();
      ICollection<string> requestedLocales = IntlUtility.CanonicalizeLocaleList(locales);
      rules.Init(requestedLocales, new PluralRulesOptions(options));
      return thisValue;
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue SupportedLocalesOf(EcmaValue locales, EcmaValue options) {
      ICollection<string> requestedLocales = IntlUtility.CanonicalizeLocaleList(locales);
      List<string> result = IntlUtility.GetSupportedLocales(CldrPluralRules.AvailableLocales, requestedLocales, new PluralRulesOptions(options));
      return new EcmaArray(result.Select(v => (EcmaValue)v).ToArray());
    }
  }
}
