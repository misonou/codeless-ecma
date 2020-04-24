using Codeless.Ecma.Intl.Utilities;
using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Codeless.Ecma.Intl.Internal {
  [IntrinsicObject(IntlObjectKind.Intl, Name = "Intl", Global = true)]
  internal static class IntlObject {
    [IntrinsicMember("Locale", EcmaPropertyAttributes.DefaultMethodProperty)]
    public const IntlObjectKind Locale = IntlObjectKind.Locale;

    [IntrinsicMember("Collator", EcmaPropertyAttributes.DefaultMethodProperty)]
    public const IntlObjectKind Collator = IntlObjectKind.Collator;

    [IntrinsicMember("NumberFormat", EcmaPropertyAttributes.DefaultMethodProperty)]
    public const IntlObjectKind NumberFormat = IntlObjectKind.NumberFormat;

    [IntrinsicMember("DateTimeFormat", EcmaPropertyAttributes.DefaultMethodProperty)]
    public const IntlObjectKind DateTimeFormat = IntlObjectKind.DateTimeFormat;

    [IntrinsicMember("PluralRules", EcmaPropertyAttributes.DefaultMethodProperty)]
    public const IntlObjectKind PluralRules = IntlObjectKind.PluralRules;

    [IntrinsicMember("ListFormat", EcmaPropertyAttributes.DefaultMethodProperty)]
    public const IntlObjectKind ListFormat = IntlObjectKind.ListFormat;

    [IntrinsicMember("RelativeTimeFormat", EcmaPropertyAttributes.DefaultMethodProperty)]
    public const IntlObjectKind RelativeTimeFormat = IntlObjectKind.RelativeTimeFormat;

    [IntrinsicMember]
    public static EcmaValue GetCanonicalLocales(EcmaValue locales) {
      ICollection<string> result = IntlUtility.CanonicalizeLocaleList(locales);
      return new EcmaArray(result.Select(v => (EcmaValue)v).ToArray());
    }
  }
}
