using Codeless.Ecma.Intl.Utilities;
using Codeless.Ecma.Runtime;
using System.Collections.Generic;
using System.Linq;

namespace Codeless.Ecma.Intl.Internal {
  [IntrinsicObject(IntlObjectKind.ListFormat)]
  internal static class ListFormatConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.AlwaysConstruct, Global = false, ObjectType = typeof(ListFormat), Prototype = IntlObjectKind.ListFormatPrototype)]
    public static EcmaValue NumberFormat([This] EcmaValue thisValue, EcmaValue locales, EcmaValue options) {
      ListFormat formatter = thisValue.GetUnderlyingObject<ListFormat>();
      ICollection<string> requestedLocales = IntlUtility.CanonicalizeLocaleList(locales);
      formatter.Init(requestedLocales, new ListFormatOptions(options));
      return thisValue;
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue SupportedLocalesOf(EcmaValue locales, EcmaValue options) {
      ICollection<string> requestedLocales = IntlUtility.CanonicalizeLocaleList(locales);
      List<string> result = IntlUtility.GetSupportedLocales(new[] { "en" }, requestedLocales, new ListFormatOptions(options));
      return new EcmaArray(result.Select(v => (EcmaValue)v).ToArray());
    }
  }
}
