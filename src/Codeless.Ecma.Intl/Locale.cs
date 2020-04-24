using Codeless.Ecma.Intl.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.Intl {
  public class Locale : IntlProvider<LocaleOptions> {
    private string localeString;

    public Locale()
      : base(IntlModule.LocalePrototype) { }

    public Locale(string locale)
      : base(IntlModule.LocalePrototype, locale) { }

    public Locale(string locale, LocaleOptions options)
      : base(IntlModule.LocalePrototype, locale, options) { }

    public Locale(Locale locale)
      : base(IntlModule.LocalePrototype, locale?.LocaleString) { }

    public Locale(Locale locale, LocaleOptions options)
      : base(IntlModule.LocalePrototype, locale?.LocaleString, options) { }

    public Locale(BcpLanguageTag tag)
      : base(IntlModule.LocalePrototype) {
      Guard.ArgumentNotNull(tag, "tag");
      Init(new[] { tag.Canonicalize().ToString() }, new LocaleOptions());
    }

    public string LocaleString { 
      get {
        EnsureInitialized();
        return localeString;
      }
    }

    protected override ICollection<string> AvailableLocales => new string[0];

    protected override void InitInternal(ICollection<string> collection, LocaleOptions options) {
      if (collection.Count != 1) {
        throw new InvalidOperationException();
      }
      BcpLanguageTagBuilder builder = new BcpLanguageTagBuilder(collection.First());
      string language = options.Language;
      if (language != null) {
        try {
          builder.Language = language;
        } catch (FormatException) {
          throw new EcmaRangeErrorException("Incorrect locale information provided");
        }
      }
      string script = options.Script;
      if (script != null) {
        try {
          builder.Script = script;
        } catch (FormatException) {
          throw new EcmaRangeErrorException("Incorrect locale information provided");
        }
      }
      string region = options.Region;
      if (region != null) {
        try {
          builder.Region = region;
        } catch (FormatException) {
          throw new EcmaRangeErrorException("Incorrect locale information provided");
        }
      }
      string calendar = options.Calendar;
      if (calendar != null) {
        try {
          builder.UExtensions["ca"] = calendar;
        } catch (FormatException) {
          throw new EcmaRangeErrorException("Incorrect locale information provided");
        }
      }
      string collation = options.Collation;
      if (collation != null) {
        try {
          builder.UExtensions["co"] = collation;
        } catch (FormatException) {
          throw new EcmaRangeErrorException("Incorrect locale information provided");
        }
      }
      string hourCycle = options.HourCycle;
      if (hourCycle != null) {
        IntlProviderOptions.ParseEnum<HourCycle>(hourCycle);
        builder.UExtensions["hc"] = hourCycle;
      }
      string caseFirst = options.CaseFirst;
      if (caseFirst != null) {
        IntlProviderOptions.ParseEnum<CollatorCaseFirst>(caseFirst);
        builder.UExtensions["kf"] = caseFirst;
      }
      bool? numeric = options.Numeric;
      if (numeric != null) {
        builder.UExtensions["kn"] = numeric.Value ? "true" : "false";
      }
      string NumberingSystem = options.NumberingSystem;
      if (NumberingSystem != null) {
        try {
          builder.UExtensions["nu"] = NumberingSystem;
        } catch (FormatException) {
          throw new EcmaRangeErrorException("Incorrect locale information provided");
        }
      }
      this.localeString = builder.Canonicalize().ToString();
    }
  
    public Locale Minimize() {
      return new Locale(BcpLanguageTag.Parse(this.LocaleString).Minimize());
    }

    public Locale Maximize() {
      return new Locale(BcpLanguageTag.Parse(this.LocaleString).Maximize());
    }
  }
}
