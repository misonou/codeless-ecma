using Codeless.Ecma.Intl.Utilities;
using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.Intl.Internal {
  [IntrinsicObject(IntlObjectKind.Locale)]
  internal static class LocaleConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.DenyCall, Global = false, ObjectType = typeof(Locale), Prototype = IntlObjectKind.LocalePrototype)]
    public static EcmaValue Locale([This] EcmaValue thisValue, EcmaValue locale, EcmaValue options) {
      Locale localeObj = thisValue.GetUnderlyingObject<Locale>();
      localeObj.Init(new List<string> { IntlUtility.CanonicalizeLanguageTag(locale) }, new LocaleOptions(options));
      return thisValue;
    }
  }
}
