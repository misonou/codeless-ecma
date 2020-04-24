using Codeless.Ecma.Intl.Utilities;
using Codeless.Ecma.Runtime;
using System.Collections.Generic;
using System.Linq;

namespace Codeless.Ecma.Intl.Internal {
  [IntrinsicObject(IntlObjectKind.RelativeTimeFormat)]
  internal static class RelativeTimeFormatConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.AlwaysConstruct, Global = false, ObjectType = typeof(RelativeTimeFormat), Prototype = IntlObjectKind.RelativeTimeFormatPrototype)]
    public static EcmaValue NumberFormat([This] EcmaValue thisValue, EcmaValue locales, EcmaValue options) {
      RelativeTimeFormat formatter = thisValue.GetUnderlyingObject<RelativeTimeFormat>();
      ICollection<string> requestedLocales = IntlUtility.CanonicalizeLocaleList(locales);
      formatter.Init(requestedLocales, new RelativeTimeFormatOptions(options));
      return thisValue;
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue SupportedLocalesOf(EcmaValue locales, EcmaValue options) {
      ICollection<string> requestedLocales = IntlUtility.CanonicalizeLocaleList(locales);
      List<string> result = IntlUtility.GetSupportedLocales(CldrRelativeTimeFormat.AvailableLocales, requestedLocales, new RelativeTimeFormatOptions(options));
      return new EcmaArray(result.Select(v => (EcmaValue)v).ToArray());
    }
  }
}
