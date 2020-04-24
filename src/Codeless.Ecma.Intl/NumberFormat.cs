using Codeless.Ecma.Intl.Utilities;
using Codeless.Ecma.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Codeless.Ecma.Intl {
  public class NumberFormat : IntlProvider<NumberFormatOptions> {
    private static readonly NumberFormatPattern defaultPattern = new NumberFormatPattern(new FormattedString(new[] { new FormattedPart(FormattedPartType.Placeholder, "0") }));
    private static readonly string[] relevantExtensionKeys = new[] { "nu" };

    private CldrNumberFormat formatProvider;
    private CldrPluralRules pluralRules;
    private Dictionary<string, FormattedPartType> symbolType;
    private Dictionary<PluralCategories, NumberFormatPattern> styleFormats;
    private ReadOnlyDictionary<PluralCategories, ReadOnlyDictionary<int, NumberFormatPattern>> compactNotations;
    private NumberFormatPattern notationFormats;

    public NumberFormat()
      : base(IntlModule.NumberFormatPrototype) { }

    public NumberFormat(string locale)
      : base(IntlModule.NumberFormatPrototype, locale) { }

    public NumberFormat(string locale, NumberFormatOptions options)
      : base(IntlModule.NumberFormatPrototype, locale, options) { }

    public NumberFormat(ICollection<string> locale)
      : base(IntlModule.NumberFormatPrototype, locale) { }

    public NumberFormat(ICollection<string> locale, NumberFormatOptions options)
      : base(IntlModule.NumberFormatPrototype, locale, options) { }

    public string Locale { get; private set; }
    public string NumberingSystem { get; private set; }
    public string Currency { get; private set; }
    public string Unit { get; private set; }
    public bool UseGrouping { get; private set; }
    public NumberStyle Style { get; private set; }
    public NumberNotation Notation { get; private set; }
    public NumberFormatDigitOptions Digits { get; private set; }
    public NumberCompactDisplayFormat CompactDisplay { get; private set; }
    public CurrencyDisplayFormat CurrencyDisplay { get; private set; }
    public CurrencySignFormat CurrencySign { get; private set; }
    public UnitDisplayFormat UnitDisplay { get; private set; }
    public SignDisplayFormat SignDisplay { get; private set; }
    public EcmaValue BoundFormat { get; private set; }

    protected override ICollection<string> AvailableLocales => CldrNumberFormat.AvailableLocales;

    protected override void InitInternal(ICollection<string> locales, NumberFormatOptions options) {
      Hashtable ht = new Hashtable();
      LocaleMatcher localeMatcher = options.LocaleMatcher;
      ht["nu"] = options.NumberingSystem;
      this.Locale = ResolveLocale(locales, localeMatcher, relevantExtensionKeys, ht, out ht);
      this.NumberingSystem = (string)ht["nu"];
      this.Style = options.Style;

      string currency = options.Currency;
      if (currency != null) {
        if (!IntlUtility.IsWellFormedCurrencyCode(currency)) {
          throw new EcmaRangeErrorException("Invalid currency codes: {0}", currency);
        }
        this.Currency = currency;
      }
      this.CurrencyDisplay = options.CurrencyDisplay;
      this.CurrencySign = options.CurrencySign;

      this.Unit = options.Unit;
      if (this.Unit != null) {
        if (!IntlUtility.IsWellFormedUnitIdentifier(this.Unit)) {
          throw new EcmaRangeErrorException("Invalid unit identifier: {0}", this.Unit);
        }
      }
      this.UnitDisplay = options.UnitDisplay;

      NumberFormatInfo numberFormat = CultureInfo.GetCultureInfo(this.Locale).NumberFormat;
      int defaultMinFractionDigits, defaultMaxFractionDigits;
      if (this.Style == NumberStyle.Currency) {
        if (currency == null) {
          throw new EcmaTypeErrorException("Currency code is required with currency style");
        }
        defaultMinFractionDigits = numberFormat.CurrencyDecimalDigits;
        defaultMaxFractionDigits = numberFormat.CurrencyDecimalDigits;
      } else {
        if (this.Style == NumberStyle.Unit && this.Unit == null) {
          throw new EcmaTypeErrorException("Unit is required with unit style");
        }
        defaultMinFractionDigits = 0;
        defaultMaxFractionDigits = this.Style == NumberStyle.Percent ? 0 : 3;
      }
      this.Notation = options.Notation;
      this.Digits = options.GetNumberFormatDigitOptions(defaultMinFractionDigits, defaultMaxFractionDigits, this.Notation);

      NumberCompactDisplayFormat compactDisplay = options.CompactDisplay;
      if (this.Notation == NumberNotation.Compact) {
        this.CompactDisplay = compactDisplay;
      }
      this.UseGrouping = options.UseGrouping;
      this.SignDisplay = options.SignDisplay;
      this.BoundFormat = Literal.FunctionLiteral(this.FormatInternal);
      this.formatProvider = CldrNumberFormat.GetFormat(this.Locale, this.NumberingSystem);
      this.pluralRules = CldrPluralRules.Resolve(PluralRuleType.Cardinal, this.Locale);

      NumberFormatInfo formatter = formatProvider.FormatProvider;
      this.symbolType = new Dictionary<string, FormattedPartType> {
        [formatter.PositiveSign] = FormattedPartType.PlusSign,
        [formatter.NegativeSign] = FormattedPartType.MinusSign,
        [formatter.NumberGroupSeparator] = FormattedPartType.Group,
        [formatter.NumberDecimalSeparator] = FormattedPartType.Decimal,
        [formatter.PercentSymbol] = FormattedPartType.PercentSign,
        ["E"] = FormattedPartType.ExponentSeparator
      };
      this.styleFormats = GetStyleFormat().ToDictionary(v => v.Key, v => v.Value);
      if (this.Notation == NumberNotation.Compact) {
        this.compactNotations = formatProvider.GetCompactNotationFormats(this.CompactDisplay);
      } else if (this.Notation == NumberNotation.Scientific) {
        this.notationFormats = formatProvider.GetScientificNotationFormat().ToDisplayFormat(this);
      } else {
        this.notationFormats = formatProvider.GetDecimalNotationFormat().ToDisplayFormat(this);
      }
    }

    public FormattedString Format(EcmaValue value) {
      EnsureInitialized();
      NumberFormatInfo formatter = formatProvider.FormatProvider;
      value = value.ToNumber();
      if (value.IsNaN || !value.IsFinite) {
        string str = value.ToDouble().ToString(formatter);
        return new FormattedString(new[] { new FormattedPart(value.IsNaN ? FormattedPartType.NaN : FormattedPartType.Infinity, str) });
      }

      double doubleValue = value.ToDouble();
      if (this.SignDisplay == SignDisplayFormat.Never) {
        doubleValue = Math.Abs(doubleValue);
      }
      if (this.Style == NumberStyle.Percent) {
        doubleValue *= 100;
      }
      PluralCategories pluralCount = pluralRules.Match(doubleValue);
      NumberFormatPattern notationFormats = this.Notation == NumberNotation.Compact ? GetCompactNotationPattern(doubleValue, pluralCount, out doubleValue) : this.notationFormats;
      NumberFormatPattern styleFormats = this.styleFormats.Count == 1 ? this.styleFormats[PluralCategories.Other] : this.styleFormats[pluralCount];
      FormattedString notationFormat = null;
      FormattedString styleFormat = null;
      switch (doubleValue.CompareTo(0)) {
        case -1:
          notationFormat = notationFormats.NegativePattern;
          styleFormat = styleFormats.NegativePattern;
          break;
        case 0:
          notationFormat = notationFormats.ZeroPattern;
          styleFormat = styleFormats.ZeroPattern;
          break;
        case 1:
          notationFormat = notationFormats.PositivePattern;
          styleFormat = styleFormats.PositivePattern;
          break;
      }
      if (this.Digits.RoundingType == RoundingType.SignificantDigits) {
        doubleValue = RoundToSignificantDigits(doubleValue, this.Digits.MaximumSignificantDigits);
      }

      // format double value with corresponding notation (scienific, compact, ...)
      List<FormattedPart> numParts = new List<FormattedPart>(notationFormat);
      List<FormattedPart> subParts = new List<FormattedPart>();
      int index = numParts.FindIndex(v => v.Type == FormattedPartType.Placeholder);
      string formatted = doubleValue.ToString(numParts[index].Value, formatter);

      FormattedPartType integerType = FormattedPartType.Integer;
      RoundingType roundingType = this.Digits.RoundingType;
      int maxDigits = roundingType == RoundingType.SignificantDigits ? this.Digits.MaximumSignificantDigits : -1;
      int numOfDigits = 0;
      foreach (Match m in Regex.Matches(formatted, "([0-9]+)|.")) {
        FormattedPartType type = FormattedPartType.Literal;
        string str = m.Value;
        if (m.Groups[1].Success) {
          type = integerType;
          if (roundingType == RoundingType.SignificantDigits) {
            if (type != FormattedPartType.ExponentInteger) {
              numOfDigits += numOfDigits == 0 ? str.TrimStart('0').Length : str.Length;
            }
            if (type == FormattedPartType.Fraction && numOfDigits > maxDigits) {
              str = str.Substring(0, str.Length - numOfDigits + maxDigits);
            }
          }
        } else if (symbolType.TryGetValue(str, out type)) {
          if (type == FormattedPartType.Decimal) {
            integerType = FormattedPartType.Fraction;
          } else if (type == FormattedPartType.ExponentSeparator) {
            integerType = FormattedPartType.ExponentInteger;
          }
        }
        subParts.Add(new FormattedPart(type, str));
      }
      numParts.RemoveAt(index);
      numParts.InsertRange(index, subParts);

      // format as curreny or unit with the formatted number
      List<FormattedPart> finalParts = new List<FormattedPart>(styleFormat);
      index = finalParts.FindIndex(v => v.Type == FormattedPartType.Placeholder);
      finalParts.RemoveAt(index);
      finalParts.InsertRange(index, numParts);
      return new FormattedString(finalParts);
    }

    private NumberFormatPattern GetCompactNotationPattern(double doubleValue, PluralCategories pluralCount, out double roundedValue) {
      ReadOnlyDictionary<int, NumberFormatPattern> dict = this.compactNotations[pluralCount];
      int exponent = (int)Math.Floor(Math.Log10(Math.Abs(doubleValue)));
      NumberFormatPattern pattern;
      if (dict.TryGetValue(exponent, out NumberFormatPattern notationFormats)) {
        pattern = notationFormats;
      } else {
        pattern = exponent < dict.Keys.First() ? dict.Values.First() : dict.Values.Last();
      }
      int roundExp = exponent - pattern.PositivePattern.ToString().Count(v => v == '0') + 1;
      roundedValue = doubleValue / Math.Pow(10, roundExp);
      return pattern.ToDisplayFormat(this);
    }

    private IDictionary<PluralCategories, NumberFormatPattern> GetStyleFormat() {
      switch (this.Style) {
        case NumberStyle.Percent:
          return new Dictionary<PluralCategories, NumberFormatPattern> { [PluralCategories.Other] = formatProvider.GetPercentStyleFormat() };
        case NumberStyle.Unit:
          return formatProvider.GetUnitStyleFormat(this.Unit, this.UnitDisplay);
        case NumberStyle.Currency:
          return formatProvider.GetCurrencyStyleFormat(this.Currency, this.CurrencyDisplay, this.CurrencySign);
      }
      return new Dictionary<PluralCategories, NumberFormatPattern> { [PluralCategories.Other] = defaultPattern };
    }

    [IntrinsicMember]
    private EcmaValue FormatInternal(EcmaValue value) {
      return Format(value).ToString();
    }

    private static double RoundToSignificantDigits(double d, int digits) {
      if (d == 0) {
        return 0;
      }
      double scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(d))) + 1);
      return scale * Math.Round(d / scale, digits);
    }
  }
}
