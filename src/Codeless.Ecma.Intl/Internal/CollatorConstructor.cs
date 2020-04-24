using Codeless.Ecma.Intl.Utilities;
using Codeless.Ecma.Runtime;
using System.Collections.Generic;
using System.Linq;

namespace Codeless.Ecma.Intl.Internal {
  [IntrinsicObject(IntlObjectKind.Collator)]
  internal static class CollatorConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.AlwaysConstruct, Global = false, ObjectType = typeof(Collator), Prototype = IntlObjectKind.CollatorPrototype)]
    public static EcmaValue Collator([This] EcmaValue thisValue, EcmaValue locales, EcmaValue options) {
      Collator collator = thisValue.GetUnderlyingObject<Collator>();
      ICollection<string> requestedLocales = IntlUtility.CanonicalizeLocaleList(locales);
      collator.Init(requestedLocales, new CollatorOptions(options));
      return thisValue;
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue SupportedLocalesOf(EcmaValue locales, EcmaValue options) {
      ICollection<string> requestedLocales = IntlUtility.CanonicalizeLocaleList(locales);
      List<string> result = IntlUtility.GetSupportedLocales(new[] { "en" }, requestedLocales, new CollatorOptions(options));
      return new EcmaArray(result.Select(v => (EcmaValue)v).ToArray());
    }
  }
}
