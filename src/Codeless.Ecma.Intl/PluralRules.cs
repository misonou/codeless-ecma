using Codeless.Ecma.Intl.Utilities;
using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;

namespace Codeless.Ecma.Intl {
  [Flags]
  public enum PluralCategories {
    [StringValue("zero")]
    Zero = 1,
    [StringValue("one")]
    One = 2,
    [StringValue("two")]
    Two = 4,
    [StringValue("few")]
    Few = 8,
    [StringValue("many")]
    Many = 16,
    [StringValue("other")]
    Other = 32
  }

  public class PluralRules : IntlProvider<PluralRulesOptions> {
    private CldrPluralRules resolver;

    public PluralRules()
      : base(IntlModule.PluralRulesPrototype) { }

    public PluralRules(string locale)
      : base(IntlModule.PluralRulesPrototype, locale) { }

    public PluralRules(string locale, PluralRulesOptions options)
      : base(IntlModule.PluralRulesPrototype, locale, options) { }

    public PluralRules(ICollection<string> locale)
      : base(IntlModule.PluralRulesPrototype, locale) { }

    public PluralRules(ICollection<string> locale, PluralRulesOptions options)
      : base(IntlModule.PluralRulesPrototype, locale, options) { }

    public string Locale { get; private set; }
    public PluralCategories PluralCategories { get; private set; }
    public PluralRuleType Type { get; private set; }
    public NumberFormatDigitOptions Digits { get; private set; }

    protected override ICollection<string> AvailableLocales => CldrPluralRules.AvailableLocales;

    protected override void InitInternal(ICollection<string> locales, PluralRulesOptions options) {
      LocaleMatcher matcher = options.LocaleMatcher;
      this.Type = options.Type;
      this.Digits = options.Digits;
      this.Locale = ResolveLocale(locales, matcher);
      this.resolver = CldrPluralRules.Resolve(this.Type, this.Locale);
      this.PluralCategories = resolver.PluralCategories;
    }

    public PluralCategories ResolveCategory(double number) {
      EnsureInitialized();
      return resolver.Match(number);
    }

    public EcmaValue Select(EcmaValue value) {
      return IntlProviderOptions.ToStringValue(ResolveCategory(value.ToNumber().ToDouble()));
    }
  }
}
