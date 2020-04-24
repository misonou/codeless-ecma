using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Codeless.Ecma.Intl.Utilities {
  internal class CldrRelativeTimeFormat {
    private static readonly XDocument xDocument = CldrUtility.LoadXml("Codeless.Ecma.Intl.Data.dateFields.xml.gz");
    private static readonly ICollection<string> availableLocales = xDocument.XPathSelectElements("/root/fields").Select(v => v.Attribute("locale").Value).ToList().AsReadOnly();
    private static readonly ConcurrentDictionary<string, CldrRelativeTimeFormat> resolvedPatterns = new ConcurrentDictionary<string, CldrRelativeTimeFormat>();

    private readonly Dictionary<int, FormattedString> relative = new Dictionary<int, FormattedString>();
    private readonly Dictionary<PluralCategories, FormattedString> past = new Dictionary<PluralCategories, FormattedString>();
    private readonly Dictionary<PluralCategories, FormattedString> future = new Dictionary<PluralCategories, FormattedString>();
    private readonly RelativeTimeUnit unit;
    private readonly string locale;

    private CldrRelativeTimeFormat(string locale, string type) {
      int pos = type.IndexOf('-');
      this.unit = IntlProviderOptions.ParseEnum<RelativeTimeUnit>(pos < 0 ? type : type.Substring(0, pos));
      this.locale = locale;
    }

    public static ICollection<string> AvailableLocales => availableLocales;

    public FormattedString Format(double value, NumberFormat formatter, RelativeTimeNumericFormat numeric, out string[] units) {
      FormattedString pattern;
      if (numeric == RelativeTimeNumericFormat.Auto) {
        double intValue = Math.Floor(value);
        if (intValue == value && intValue >= Int32.MinValue && intValue <= Int32.MaxValue) {
          if (relative.TryGetValue((int)intValue, out pattern)) {
            units = new string[1];
            return pattern;
          }
        }
      }
      CldrPluralRules pluralRules = CldrPluralRules.Resolve(PluralRuleType.Cardinal, locale);
      PluralCategories key = pluralRules.Match(value);
      if ((value < 0 ? past : future).TryGetValue(key, out pattern)) {
        FormattedString numParts = formatter.Format(Math.Abs(value));
        FormattedPart[] parts = pattern.GetParts();
        int index = Array.FindIndex(parts, v => v.Type == FormattedPartType.Placeholder);
        string unitStr = IntlProviderOptions.ToStringValue(unit);
        if (numParts.PartCount > 1) {
          List<FormattedPart> parts1 = new List<FormattedPart>(parts);
          parts1.RemoveAt(index);
          parts1.InsertRange(index, numParts);
          units = new string[parts1.Count];
          for (int j = 0, len2 = numParts.PartCount; j < len2; j++) {
            units[index + j] = unitStr;
          }
          return new FormattedString(parts1);
        } else {
          units = new string[parts.Length];
          units[index] = unitStr;
          parts[index] = numParts[0];
          return new FormattedString(parts);
        }
      }
      units = new string[0];
      return FormattedString.Empty;
    }

    public static CldrRelativeTimeFormat Resolve(string locale, RelativeTimeUnit unit, RelativeTimeStyle style) {
      return Resolve(locale, GetLdmlFieldType(unit, style));
    }

    public static CldrRelativeTimeFormat Resolve(string locale, string type) {
      string key = locale + "/" + type;
      if (resolvedPatterns.TryGetValue(key, out CldrRelativeTimeFormat cached)) {
        return cached;
      }
      CldrRelativeTimeFormat formatter = new CldrRelativeTimeFormat(locale, type);
      XElement field = xDocument.XPathSelectElement(String.Format("/root/fields[@locale = '{0}']/field[@type = '{1}']", locale, type));
      if (field == null) {
        throw new InvalidOperationException("Unknown locale or type");
      }
      if (CldrUtility.IsAlias(field, out string use)) {
        return resolvedPatterns.GetOrAdd(key, Resolve(locale, use));
      }
      bool hasInheritedValues = field.Attribute("inherits") != null;
      foreach (XElement relative in field.Elements("relative")) {
        int amount = Int32.Parse(relative.Attribute("type").Value);
        formatter.relative[amount] = FormattedString.Parse(relative.Value);
      }
      foreach (XElement relativeTime in field.Elements("relativeTime")) {
        Dictionary<PluralCategories, FormattedString> dict = relativeTime.Attribute("type").Value == "future" ? formatter.future : formatter.past;
        foreach (XElement child in relativeTime.Elements()) {
          PluralCategories category = IntlProviderOptions.ParseEnum<PluralCategories>(child.Attribute("count").Value);
          dict[category] = FormattedString.Parse(child.Value);
        }
        hasInheritedValues |= relativeTime.Attribute("inherits") != null;
      }
      if (hasInheritedValues) {
        CldrRelativeTimeFormat parent = CldrUtility.GetParentPatterns(locale, type, Resolve);
        CldrUtility.CopyPatternFromParent(formatter.relative, parent.relative);
        CldrUtility.CopyPatternFromParent(formatter.future, parent.future);
        CldrUtility.CopyPatternFromParent(formatter.past, parent.past);
      }
      return resolvedPatterns.GetOrAdd(key, formatter);
    }

    private static string GetLdmlFieldType(RelativeTimeUnit unit, RelativeTimeStyle style) {
      string str = IntlProviderOptions.ToStringValue(unit);
      if (style == RelativeTimeStyle.Long) {
        return str;
      }
      return str + "-" + IntlProviderOptions.ToStringValue(style);
    }
  }
}
