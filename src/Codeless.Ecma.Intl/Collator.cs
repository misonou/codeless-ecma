using Codeless.Ecma.Intl.Utilities;
using Codeless.Ecma.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Codeless.Ecma.Intl {
  public class Collator : IntlProvider<CollatorOptions> {
    private static readonly string[] relevantExtensionKeys = new[] { "co", "kn", "kf" };

    private CompareInfo comparer;

    public Collator()
      : base(IntlModule.CollatorPrototype) { }

    public Collator(string locale)
      : base(IntlModule.CollatorPrototype, locale) { }

    public Collator(string locale, CollatorOptions options)
      : base(IntlModule.CollatorPrototype, locale, options) { }

    public Collator(ICollection<string> locale)
      : base(IntlModule.CollatorPrototype, locale) { }

    public Collator(ICollection<string> locale, CollatorOptions options)
      : base(IntlModule.CollatorPrototype, locale, options) { }

    public string Locale { get; private set; }
    public string Collation { get; private set; }
    public bool IgnorePunctuation { get; private set; }
    public bool Numeric { get; private set; }
    public CollatorUsage Usage { get; private set; }
    public CollatorCaseFirst CaseFirst { get; private set; }
    public CollatorSensitivity Sensitivity { get; private set; }
    public EcmaValue BoundCompare { get; private set; }

    protected override ICollection<string> AvailableLocales => IntlUtility.SystemLocales;

    protected override void InitInternal(ICollection<string> locales, CollatorOptions options) {
      this.Usage = options.Usage;
      LocaleMatcher matcher = options.LocaleMatcher;
      Hashtable ht = new Hashtable();
      ht["kn"] = options.Numeric == true ? "" : null;
      ht["kf"] = IntlProviderOptions.ToStringValue(options.CaseFirst);

      this.Locale = ResolveLocale(locales, matcher, relevantExtensionKeys, ht, out ht);
      this.Collation = (string)ht["co"];
      this.Numeric = String.Empty.Equals(ht["kn"]);
      this.CaseFirst = IntlProviderOptions.ParseEnum<CollatorCaseFirst>((string)ht["kf"]);
      this.Sensitivity = options.Sensitivity;
      if (this.Sensitivity == CollatorSensitivity.Unspecified) {
        if (this.Usage == CollatorUsage.Sort) {
          this.Sensitivity = CollatorSensitivity.Variant;
        } else {
          this.Sensitivity = CollatorSensitivity.Variant;
        }
      }
      this.IgnorePunctuation = options.IgnorePunctuation;
      this.BoundCompare = Literal.FunctionLiteral(this.Compare);
      this.comparer = CultureInfo.GetCultureInfo(this.Locale).CompareInfo;
    }

    [IntrinsicMember]
    public EcmaValue Compare(EcmaValue x, EcmaValue y) {
      EnsureInitialized();
      return comparer.Compare(x.ToStringOrThrow(), y.ToStringOrThrow());
    }
  }
}
