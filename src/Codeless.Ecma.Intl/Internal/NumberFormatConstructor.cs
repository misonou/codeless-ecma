using Codeless.Ecma.Intl.Utilities;
using Codeless.Ecma.Runtime;
using System.Collections.Generic;
using System.Linq;

namespace Codeless.Ecma.Intl.Internal {
  [IntrinsicObject(IntlObjectKind.NumberFormat)]
  internal static class NumberFormatConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.AlwaysConstruct, Global = false, ObjectType = typeof(NumberFormat), Prototype = IntlObjectKind.NumberFormatPrototype)]
    public static EcmaValue NumberFormat([This] EcmaValue thisValue, EcmaValue locales, EcmaValue options) {
      NumberFormat formatter = thisValue.GetUnderlyingObject<NumberFormat>();
      ICollection<string> requestedLocales = IntlUtility.CanonicalizeLocaleList(locales);
      formatter.Init(requestedLocales, new NumberFormatOptions(options));
      return thisValue;
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue SupportedLocalesOf(EcmaValue locales, EcmaValue options) {
      ICollection<string> requestedLocales = IntlUtility.CanonicalizeLocaleList(locales);
      List<string> result = IntlUtility.GetSupportedLocales(new[] { "en" }, requestedLocales, new NumberFormatOptions(options));
      return new EcmaArray(result.Select(v => (EcmaValue)v).ToArray());
    }
  }
}
