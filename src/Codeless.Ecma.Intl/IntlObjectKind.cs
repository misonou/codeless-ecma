using Codeless.Ecma.Runtime;

namespace Codeless.Ecma.Intl {
  [IntrinsicObjectEnum((int)MaxValue)]
  public enum IntlObjectKind {
    Intl,
    Locale,
    LocalePrototype,
    Collator,
    CollatorPrototype,
    NumberFormat,
    NumberFormatPrototype,
    DateTimeFormat,
    DateTimeFormatPrototype,
    PluralRules,
    PluralRulesPrototype,
    RelativeTimeFormat,
    RelativeTimeFormatPrototype,
    ListFormat,
    ListFormatPrototype,
    DisplayNames,
    DisplayNamesPrototype,
    MaxValue
  }
}
