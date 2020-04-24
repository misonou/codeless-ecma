using Codeless.Ecma.Intl.Utilities;
using Codeless.Ecma.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.Intl {
  public enum RelativeTimeUnit {
    [StringValue("year")]
    Year,
    [StringValue("quarter")]
    Quarter,
    [StringValue("month")]
    Month,
    [StringValue("week")]
    Week,
    [StringValue("day")]
    Day,
    [StringValue("hour")]
    Hour,
    [StringValue("minute")]
    Minute,
    [StringValue("second")]
    Second
  }

  public class RelativeTimeFormat : IntlProvider<RelativeTimeFormatOptions> {
    private static readonly string[] relevantExtensionKeys = new[] { "nu" };
    private static readonly Dictionary<string, string> singularForms = new Dictionary<string, string> {
      ["seconds"] = "second",
      ["minutes"] = "minute",
      ["hours"] = "hour",
      ["days"] = "day",
      ["weeks"] = "week",
      ["months"] = "month",
      ["quarters"] = "quarter",
      ["years"] = "year"
    };

    public RelativeTimeFormat()
      : base(IntlModule.RelativeTimeFormatPrototype) { }

    public RelativeTimeFormat(string locale)
      : base(IntlModule.RelativeTimeFormatPrototype, locale) { }

    public RelativeTimeFormat(string locale, RelativeTimeFormatOptions options)
      : base(IntlModule.RelativeTimeFormatPrototype, locale, options) { }

    public RelativeTimeFormat(ICollection<string> locale)
      : base(IntlModule.RelativeTimeFormatPrototype, locale) { }

    public RelativeTimeFormat(ICollection<string> locale, RelativeTimeFormatOptions options)
      : base(IntlModule.RelativeTimeFormatPrototype, locale, options) { }

    public string Locale { get; private set; }
    public string NumberingSystem { get; private set; }
    public RelativeTimeStyle Style { get; private set; }
    public RelativeTimeNumericFormat Numeric { get; private set; }
    public NumberFormat NumberFormat { get; private set; }
    public EcmaValue BoundFormat { get; private set; }

    protected override ICollection<string> AvailableLocales => CldrRelativeTimeFormat.AvailableLocales;

    protected override void InitInternal(ICollection<string> locales, RelativeTimeFormatOptions options) {
      LocaleMatcher matcher = options.LocaleMatcher;
      Hashtable ht = new Hashtable();
      ht["nu"] = options.NumberingSystem;
      this.Locale = ResolveLocale(locales, matcher, relevantExtensionKeys, ht, out ht);
      this.NumberingSystem = (string)ht["nu"];
      this.Style = options.Style;
      this.Numeric = options.Numeric;
      this.NumberFormat = new NumberFormat(this.Locale);
      this.BoundFormat = Literal.FunctionLiteral(this.FormatInternal);
    }

    public FormattedString Format(EcmaValue value, EcmaValue unit) {
      return Format(value, unit, out _);
    }

    public FormattedString Format(EcmaValue value, EcmaValue unit, out string[] units) {
      EcmaValue number = value.ToNumber();
      string unitStr = unit.ToStringOrThrow();
      if (number.IsNaN || !number.IsFinite) {
        throw new EcmaRangeErrorException("Value need to be finite number for Intl.RelativeTimeFormat.prototype.format()");
      }
      if (singularForms.TryGetValue(unitStr, out string singular)) {
        unitStr = singular;
      }
      double doubleValue = number.ToDouble();
      RelativeTimeUnit parsedUnit = IntlProviderOptions.ParseEnum<RelativeTimeUnit>(unitStr);
      return Format(doubleValue, parsedUnit, out units);
    }

    public FormattedString Format(double value, RelativeTimeUnit parsedUnit) {
      return Format(value, parsedUnit, out _);
    }

    public FormattedString Format(double value, RelativeTimeUnit parsedUnit, out string[] units) {
      EnsureInitialized();
      CldrRelativeTimeFormat formatter = CldrRelativeTimeFormat.Resolve(this.Locale, parsedUnit, this.Style);
      return formatter.Format(value, this.NumberFormat, this.Numeric, out units);
    }

    [IntrinsicMember]
    private EcmaValue FormatInternal(EcmaValue value, EcmaValue unit) {
      return Format(value, unit, out _).ToString();
    }
  }
}
