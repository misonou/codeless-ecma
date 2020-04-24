using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Codeless.Ecma.Intl.Utilities {
  internal class CldrNumberFormat {
    private static readonly XDocument xDocument = CldrUtility.LoadXml("Codeless.Ecma.Intl.Data.numbers.xml.gz");
    private static readonly ICollection<string> availableLocales = xDocument.XPathSelectElements("/root/numbers").Select(v => v.Attribute("locale").Value).ToList().AsReadOnly();
    private static readonly ConcurrentDictionary<string, CldrNumberFormat> instances = new ConcurrentDictionary<string, CldrNumberFormat>();
    private static readonly ConcurrentDictionary<string, FormattedString> parsedPatterns = new ConcurrentDictionary<string, FormattedString>();

    private readonly string locale;
    private readonly string numberSystem;
    private readonly CldrPluralRules pluralRules;
    private readonly XElement[] elements;
    private CldrNumberFormat parent;
    private NumberFormatInfo formatProvider;
    private NumberFormatPattern decimalNotation;
    private NumberFormatPattern scientificNotation;
    private NumberFormatPattern percentFormats;

    private CldrNumberFormat(string locale, string numberSystem) {
      this.locale = locale;
      this.numberSystem = numberSystem;
      this.pluralRules = CldrPluralRules.Resolve(PluralRuleType.Cardinal, locale);
      this.elements = xDocument.XPathSelectElements(String.Format("/root/numbers[@locale = '{0}']/*[local-name() = 'currencies' or @numberSystem = '{1}']", locale, numberSystem)).ToArray();
    }

    public static ICollection<string> AvailableLocales => availableLocales;

    public CldrNumberFormat Parent {
      get {
        if (this.parent == null && locale != "root") {
          this.parent = GetFormat(CldrUtility.GetParentLocale(locale), numberSystem);
        }
        return this.parent;
      }
    }

    public NumberFormatInfo FormatProvider {
      get {
        if (formatProvider == null) {
          XElement symbols = elements.FirstOrDefault(v => v.Name.LocalName == "symbols");
          if (symbols == null) {
            formatProvider = this.Parent.FormatProvider;
          } else if (CldrUtility.IsAlias(symbols, out string numberSystem)) {
            formatProvider = GetFormat(locale, numberSystem).FormatProvider;
          } else {
            Dictionary<string, string> ht = new Dictionary<string, string>();
            foreach (XElement child in symbols.Elements()) {
              ht[child.Name.LocalName] = child.Value;
            }
            NumberFormatInfo parent = symbols.Attribute("inherits") != null ? this.Parent.FormatProvider : null;
            NumberFormatInfo info = new NumberFormatInfo();
            info.NumberDecimalSeparator = ht["decimal"] ?? parent.NumberDecimalSeparator;
            info.NumberGroupSeparator = ht["group"] ?? parent.NumberGroupSeparator;
            info.PercentSymbol = ht["percentSign"] ?? parent.PercentSymbol;
            info.PositiveSign = ht["plusSign"] ?? parent.PositiveSign;
            info.NegativeSign = ht["minusSign"] ?? parent.NegativeSign;
            info.PerMilleSymbol = ht["perMille"] ?? parent.PerMilleSymbol;
            info.NaNSymbol = ht["nan"] ?? parent.NaNSymbol;
            info.PositiveInfinitySymbol = ht["infinity"] ?? parent.PositiveInfinitySymbol;
            info.NegativeInfinitySymbol = ht["infinity"] ?? parent.NegativeInfinitySymbol;
            formatProvider = info;
          }
        }
        return formatProvider;
      }
    }

    public static CldrNumberFormat GetFormat(string locale, string numberSystem) {
      string key = String.Concat(locale, "/", numberSystem);
      return instances.GetOrAdd(key, _ => new CldrNumberFormat(locale, numberSystem));
    }

    public NumberFormatPattern GetDecimalNotationFormat() {
      if (decimalNotation == null) {
        XElement pattern = elements.FirstOrDefault(v => v.Name.LocalName == "decimalFormats");
        if (pattern == null) {
          decimalNotation = this.Parent.GetDecimalNotationFormat();
        } else if (CldrUtility.IsAlias(pattern, out string numberSystem)) {
          decimalNotation = GetFormat(locale, numberSystem).GetDecimalNotationFormat();
        } else {
          pattern = pattern.XPathSelectElement("decimalFormatLength[not(@type)]/decimalFormat/pattern");
          if (pattern == null) {
            decimalNotation = this.Parent.GetDecimalNotationFormat();
          } else {
            decimalNotation = ParseNumberFormatPattern(pattern.Value);
          }
        }
      }
      return decimalNotation;
    }

    public NumberFormatPattern GetScientificNotationFormat() {
      if (scientificNotation == null) {
        XElement pattern = elements.FirstOrDefault(v => v.Name.LocalName == "scientificFormats");
        if (pattern == null) {
          scientificNotation = this.Parent.GetScientificNotationFormat();
        } else if (CldrUtility.IsAlias(pattern, out string numberSystem)) {
          scientificNotation = GetFormat(locale, numberSystem).GetScientificNotationFormat();
        } else {
          pattern = pattern.XPathSelectElement("scientificFormatLength/scientificFormat/pattern");
          if (pattern == null) {
            scientificNotation = this.Parent.GetScientificNotationFormat();
          } else {
            scientificNotation = ParseNumberFormatPattern(pattern.Value);
          }
        }
      }
      return scientificNotation;
    }

    public NumberFormatPattern GetPercentStyleFormat() {
      if (percentFormats == null) {
        XElement pattern = elements.FirstOrDefault(v => v.Name.LocalName == "percentFormats");
        if (pattern == null) {
          percentFormats = this.Parent.GetPercentStyleFormat();
        } else if (CldrUtility.IsAlias(pattern, out string numberSystem)) {
          percentFormats = GetFormat(locale, numberSystem).GetPercentStyleFormat();
        } else {
          pattern = pattern.XPathSelectElement("percentFormatLength/percentFormat/pattern");
          if (pattern == null) {
            percentFormats = this.Parent.GetPercentStyleFormat();
          } else {
            percentFormats = ParseNumberFormatPattern(pattern.Value);
          }
        }
      }
      return percentFormats;
    }

    public ReadOnlyDictionary<PluralCategories, ReadOnlyDictionary<int, NumberFormatPattern>> GetCompactNotationFormats(NumberCompactDisplayFormat style) {
      XElement decimalFormats = elements.FirstOrDefault(v => v.Name.LocalName == "decimalFormats");
      if (decimalFormats == null) {
        return this.Parent.GetCompactNotationFormats(style);
      }
      if (CldrUtility.IsAlias(decimalFormats, out string numberSystem)) {
        return GetFormat(locale, numberSystem).GetCompactNotationFormats(style);
      }
      Dictionary<PluralCategories, Dictionary<int, NumberFormatPattern>> dict = new Dictionary<PluralCategories, Dictionary<int, NumberFormatPattern>>();
      foreach (PluralCategories kind in pluralRules.EnumerateCategories()) {
        dict[kind] = new Dictionary<int, NumberFormatPattern>();
      }
      XElement decimalFormat = decimalFormats.XPathSelectElement(String.Format("decimalFormatLength[@type = '{0}']/decimalFormat", IntlProviderOptions.ToStringValue(style)));
      foreach (XElement pattern in decimalFormat.XPathSelectElements("pattern")) {
        PluralCategories count = IntlProviderOptions.ParseEnum<PluralCategories>(pattern.Attribute("count").Value);
        dict[count][pattern.Attribute("type").Value.Length - 1] = ParseNumberFormatPattern(pattern.Value);
      }
      if (decimalFormat.Attribute("inherits") != null) {
        ReadOnlyDictionary<PluralCategories, ReadOnlyDictionary<int, NumberFormatPattern>> parent = this.Parent.GetCompactNotationFormats(style);
        foreach (KeyValuePair<PluralCategories, ReadOnlyDictionary<int, NumberFormatPattern>> e in parent) {
          CldrUtility.CopyPatternFromParent(dict[e.Key], e.Value);
        }
      }
      return new ReadOnlyDictionary<PluralCategories, ReadOnlyDictionary<int, NumberFormatPattern>>(dict.ToDictionary(v => v.Key, v => new ReadOnlyDictionary<int, NumberFormatPattern>(v.Value)));
    }

    public ReadOnlyDictionary<PluralCategories, NumberFormatPattern> GetCurrencyStyleFormat(string code, CurrencyDisplayFormat format, CurrencySignFormat sign) {
      Dictionary<PluralCategories, NumberFormatPattern> dict = new Dictionary<PluralCategories, NumberFormatPattern>();
      NumberFormatPattern pattern = GetCurrencyStyleFormat(sign);
      FormattedString[] src = new[] { pattern.PositivePattern, pattern.NegativePattern, pattern.ZeroPattern };
      FormattedString[] dst = new FormattedString[3];
      foreach (KeyValuePair<PluralCategories, string> e in GetCurrencyName(code, format)) {
        Array.Clear(dst, 0, 3);
        for (int i = 0; i < 3; i++) {
          if (src[i] != dst[0]) {
            FormattedPart[] parts = src[i].GetParts();
            int index = Array.FindIndex(parts, v => v.Type == FormattedPartType.Currency);
            parts[index] = new FormattedPart(FormattedPartType.Currency, e.Value);
            dst[i] = new FormattedString(parts);
          }
        }
        dict.Add(e.Key, new NumberFormatPattern(dst[0], dst[1] ?? dst[0], dst[2] ?? dst[0]));
      }
      return new ReadOnlyDictionary<PluralCategories, NumberFormatPattern>(dict);
    }

    public ReadOnlyDictionary<PluralCategories, NumberFormatPattern> GetUnitStyleFormat(string unit, UnitDisplayFormat format) {
      IDictionary<PluralCategories, string> pattern = GetUnitSubFormat(unit, format);
      if (pattern == null) {
        int pos = unit.IndexOf("-per-");
        if (pos <= 0) {
          throw new ArgumentOutOfRangeException("unit");
        }
        string nUnit = unit.Substring(0, pos);
        string dUnit = unit.Substring(pos + 5);
        string perUnitPattern = GetUnitSubFormat(unit, format, true)[PluralCategories.Other];
        ReadOnlyDictionary<PluralCategories, string> nUnitPattern = GetUnitSubFormat(nUnit, format);
        if (perUnitPattern != null) {
          pattern = nUnitPattern.ToDictionary(v => v.Key, v => String.Format(perUnitPattern, v.Value));
        } else {
          string compoundUnitPattern = GetUnitSubFormat("per", format)[PluralCategories.Other];
          string dUnitPattern = GetUnitSubFormat(dUnit, format)[pluralRules.Match(1)];
          pattern = nUnitPattern.ToDictionary(v => v.Key, v => String.Format(compoundUnitPattern, v.Value, dUnitPattern.Replace("{0}", "").Trim()));
        }
      }
      return new ReadOnlyDictionary<PluralCategories, NumberFormatPattern>(pattern.ToDictionary(v => v.Key, v => new NumberFormatPattern(FormattedString.Parse(v.Value))));
    }

    private ReadOnlyDictionary<PluralCategories, string> GetUnitSubFormat(string unit, UnitDisplayFormat format, bool perUnit = false) {
      XDocument units = CldrUtility.LoadXml("Codeless.Ecma.Intl.Data.units.xml.gz");
      XElement unitLength = units.XPathSelectElement(String.Format("/root/units[@locale = '{0}']/unitLength[@type = '{1}']", locale, IntlProviderOptions.ToStringValue(format)));
      if (unitLength == null) {
        return this.Parent.GetUnitSubFormat(unit, format);
      }
      if (CldrUtility.IsAlias(unitLength, out string type)) {
        return GetUnitSubFormat(unit, IntlProviderOptions.ParseEnum<UnitDisplayFormat>(type), perUnit);
      }
      bool hasInheritedValues = unitLength.Attribute("inherits") != null;
      if (unit == "per") {
        XElement perCompountUnit = unitLength.XPathSelectElement("compoundUnit[@type = 'per']/compoundUnitPattern");
        if (perCompountUnit == null) {
          return hasInheritedValues ? this.Parent.GetUnitSubFormat(unit, format) : null;
        }
        return CreateSingleResult(perCompountUnit.Value);
      }
      XElement unitElm = unitLength.XPathSelectElement(String.Format("unit[substring-after(@type, '-') = '{0}']", unit));
      if (unitElm == null) {
        return hasInheritedValues ? this.Parent.GetUnitSubFormat(unit, format) : null;
      }
      if (perUnit) {
        XElement perUnitPattern = unitElm.XPathSelectElement("perUnitPattern");
        if (perUnitPattern == null) {
          return hasInheritedValues ? this.Parent.GetUnitSubFormat(unit, format) : null;
        }
        return CreateSingleResult(perUnitPattern.Value);
      }
      Dictionary<PluralCategories, string> dict = new Dictionary<PluralCategories, string>();
      foreach (XElement unitPattern in unitElm.XPathSelectElements("unitPattern")) {
        dict[IntlProviderOptions.ParseEnum<PluralCategories>(unitPattern.Attribute("count").Value)] = unitPattern.Value;
      }
      if (unitElm.Attribute("inherits") != null) {
        ReadOnlyDictionary<PluralCategories, string> parent = this.Parent.GetUnitSubFormat(unit, format);
        CldrUtility.CopyPatternFromParent(dict, parent);
      }
      return new ReadOnlyDictionary<PluralCategories, string>(dict);
    }

    private ReadOnlyDictionary<PluralCategories, string> GetCurrencyName(string code, CurrencyDisplayFormat format) {
      if (format == CurrencyDisplayFormat.Code) {
        return CreateSingleResult(code);
      }
      XElement currencies = elements.FirstOrDefault(v => v.Name.LocalName == "currencies");
      XElement symbol = currencies?.XPathSelectElement(String.Format("currency[@type = '{0}']/symbol[@alt = 'narrow']/symbol", code));
      if (symbol == null) {
        return locale == "root" ? CreateSingleResult(code) : this.Parent.GetCurrencyName(code, format);
      }
      return CreateSingleResult(symbol.Value);
    }

    private NumberFormatPattern GetCurrencyStyleFormat(CurrencySignFormat sign) {
      XElement currencyFormats = elements.FirstOrDefault(v => v.Name.LocalName == "currencyFormats");
      if (currencyFormats == null) {
        return this.Parent.GetCurrencyStyleFormat(sign);
      }
      if (CldrUtility.IsAlias(currencyFormats, out string numberSystem)) {
        return GetFormat(locale, numberSystem).GetCurrencyStyleFormat(sign);
      }
      XElement currencyFormat = currencyFormats.XPathSelectElement(String.Format("currencyFormatLength[not(@type)]/currencyFormat[@type = '{0}']", IntlProviderOptions.ToStringValue(sign)));
      if (currencyFormat == null) {
        return this.Parent.GetCurrencyStyleFormat(sign);
      }
      return ParseNumberFormatPattern(currencyFormat.Value);
    }

    private static NumberFormatPattern ParseNumberFormatPattern(string pattern) {
      int pos = pattern.IndexOf(';');
      if (pos < 0) {
        return new NumberFormatPattern(ParseNumberFormatSubPattern(pattern));
      }
      return new NumberFormatPattern(ParseNumberFormatSubPattern(pattern.Substring(0, pos)), ParseNumberFormatSubPattern(pattern.Substring(pos + 1)));
    }

    private static FormattedString ParseNumberFormatSubPattern(string pattern) {
      if (parsedPatterns.TryGetValue(pattern, out FormattedString cached)) {
        return cached;
      }
      List<FormattedPart> parts = new List<FormattedPart>();
      StringBuilder sb = new StringBuilder();
      foreach (Match m in Regex.Matches(pattern, "([0-9E‰%@#.,*+-]+)|'[^']*'|.")) {
        if (m.Groups[1].Success) {
          if (sb.Length > 0) {
            parts.Add(new FormattedPart(FormattedPartType.Literal, sb.ToString()));
            sb.Remove(0, sb.Length);
          }
          parts.Add(new FormattedPart(FormattedPartType.Placeholder, m.Value));
        } else if (m.Value == "¤") {
          parts.Add(new FormattedPart(FormattedPartType.Currency, m.Value));
        } else {
          sb.Append(m.Value[0] != '\'' ? m.Value : m.Value[1] == '\'' ? "'" : m.Value.Substring(1, m.Length - 2));
        }
      }
      if (sb.Length > 0) {
        parts.Add(new FormattedPart(FormattedPartType.Literal, sb.ToString()));
      }
      FormattedString parsed = new FormattedString(parts);
      return parsedPatterns.GetOrAdd(pattern, parsed);
    }

    private static ReadOnlyDictionary<PluralCategories, string> CreateSingleResult(string code) {
      return new ReadOnlyDictionary<PluralCategories, string>(new Dictionary<PluralCategories, string> { [PluralCategories.Other] = code });
    }
  }
}
