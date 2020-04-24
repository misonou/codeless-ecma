using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Codeless.Ecma.Intl.Utilities {
  internal class CldrCalendarInfo {
    private static readonly XDocument xDocument = CldrUtility.LoadXml("Codeless.Ecma.Intl.Data.calendars.xml.gz");
    private static readonly ICollection<string> availableLocales = xDocument.XPathSelectElements("/root/calendars").Select(v => v.Attribute("locale").Value).ToList().AsReadOnly();
    private static readonly ConcurrentDictionary<string, CldrCalendarInfo> cache = new ConcurrentDictionary<string, CldrCalendarInfo>();
    private static readonly char[] requiredTimeComponents = new[] { 'h', 'H', 'k', 'K', 'm' };
    private static readonly char[] requiredDateComponents = new[] { 'y', 'M', 'd' };
    private static readonly char[] unsupportedTimeComponents = new[] { 'E', 'c' };
    private static readonly char[] unsupportedDateComponents = new[] { 'Q', 'w', 'W' };
    private static readonly Dictionary<string, string> labelKeysEra = new Dictionary<string, string> {
      ["eraNames"] = "long",
      ["eraAbbr"] = "short",
      ["eraNarrow"] = "narrow"
    };
    private static readonly Dictionary<string, string> labelKeysDayPeriod = new Dictionary<string, string> {
      ["wide"] = "long",
      ["abbreviated"] = "short",
      ["narrow"] = "narrow"
    };
    private static readonly Dictionary<string, string> labelKeysMonth = new Dictionary<string, string> {
      ["wide"] = "long",
      ["abbreviated"] = "short",
      ["narrow"] = "narrow"
    };
    private static readonly Dictionary<string, string> labelKeysWeekday = new Dictionary<string, string> {
      ["wide"] = "long",
      ["abbreviated"] = "short",
      ["short"] = "short",
      ["narrow"] = "narrow"
    };
    private static readonly Dictionary<FormattedPartType, string> partId = new Dictionary<FormattedPartType, string> {
      [FormattedPartType.Era] = "G",
      [FormattedPartType.Year] = "y",
      [FormattedPartType.Month] = "M",
      [FormattedPartType.Day] = "d",
      [FormattedPartType.DayPeriod] = "B",
      [FormattedPartType.Hour] = "h",
      [FormattedPartType.Minute] = "m",
      [FormattedPartType.Second] = "s",
    };

    private readonly ConcurrentDictionary<string, FormattedString> rangeFormats = new ConcurrentDictionary<string, FormattedString>();
    private readonly Dictionary<string, string[]>[] labels = new Dictionary<string, string[]>[4];
    private readonly string locale;
    private readonly string calendar;
    private readonly XElement root;
    private CldrCalendarInfo parent;
    private FormattedString defaultRangePattern;
    private Dictionary<string, string> combinePatterns;
    private ReadOnlyCollection<CldrDateTimeFormat> dateTimeFormats;
    private ReadOnlyCollection<CldrDateTimeFormat> dateFormats;
    private ReadOnlyCollection<CldrDateTimeFormat> timeFormats;

    private CldrCalendarInfo(string locale, string calendar) {
      Guard.ArgumentNotNull(locale, "locale");
      Guard.ArgumentNotNull(calendar, "calendar");
      this.locale = locale;
      this.calendar = calendar;
      this.root = xDocument.XPathSelectElement(String.Format("/root/calendars[@locale = '{0}']/calendar[@type = '{1}']", locale, calendar));
    }

    public static ICollection<string> AvailableLocales => availableLocales;

    public CldrCalendarInfo Parent {
      get {
        if (this.parent == null && locale != "root") {
          this.parent = Resolve(CldrUtility.GetParentLocale(locale), calendar);
        }
        return this.parent;
      }
    }

    public FormattedString DefaultRangePattern {
      get {
        if (this.defaultRangePattern == null) {
          XElement dateTimeFormats = root.XPathSelectElement("dateTimeFormats");
          if (CldrUtility.IsAlias(dateTimeFormats, out string calendar)) {
            this.defaultRangePattern = Resolve(locale, calendar).DefaultRangePattern;
          } else {
            XElement fallback = dateTimeFormats.XPathSelectElement("intervalFormats/intervalFormatFallback");
            if (fallback == null) {
              this.defaultRangePattern = this.Parent.DefaultRangePattern;
            } else {
              this.defaultRangePattern = FormattedString.Parse(fallback.Value);
            }
          }
        }
        return this.defaultRangePattern;
      }
    }

    public static CldrCalendarInfo Resolve(string locale, string calendar) {
      string key = String.Concat(locale, "/", calendar);
      return cache.GetOrAdd(key, new CldrCalendarInfo(locale, calendar));
    }

    public string[] DayPeriodNames {
      get { return (string[])GetDayPeriodNames()["long"].Clone(); }
    }

    public string[] GetMonthNames(MonthStyle style) {
      Dictionary<string, string[]> dictionary = GetMonthNames();
      switch (style) {
        case MonthStyle.Short:
          return (string[])dictionary["short"].Clone();
        case MonthStyle.Narrow:
          return (string[])dictionary["narrow"].Clone();
        default:
          return (string[])dictionary["long"].Clone();
      }
    }

    public string[] GetEraNames(EraStyle style) {
      Dictionary<string, string[]> dictionary = GetEraNames();
      switch (style) {
        case EraStyle.Short:
          return (string[])dictionary["short"].Clone();
        case EraStyle.Narrow:
          return (string[])dictionary["narrow"].Clone();
        default:
          return (string[])dictionary["long"].Clone();
      }
    }

    public string[] GetWeekdayNames(WeekdayStyle style) {
      Dictionary<string, string[]> dictionary = GetWeekdayNames();
      switch (style) {
        case WeekdayStyle.Short:
          return (string[])dictionary["short"].Clone();
        case WeekdayStyle.Narrow:
          return (string[])dictionary["narrow"].Clone();
        default:
          return (string[])dictionary["long"].Clone();
      }
    }

    public FormattedString GetRangePattern(FormattedPartType greatestDifference, ICollection<string> patterns) {
      Guard.ArgumentNotNull(patterns, "patterns");
      if (!partId.ContainsKey(greatestDifference)) {
        throw new ArgumentOutOfRangeException("greatestDifference");
      }
      if (patterns.Count == 0) {
        throw new ArgumentException("Collection should contain at least one element", "patternId");
      }
      XElement dateTimeFormats = root.XPathSelectElement("dateTimeFormats");
      if (CldrUtility.IsAlias(dateTimeFormats, out string calendar)) {
        return Resolve(locale, calendar).GetRangePattern(greatestDifference, patterns);
      }
      foreach (XElement child in dateTimeFormats.XPathSelectElements("intervalFormats/intervalFormatItem")) {
        string formatId = child.Attribute("id").Value;
        if (patterns.Contains(formatId)) {
          return GetRangePattern(formatId, partId[greatestDifference]);
        }
      }
      if (dateTimeFormats.Attribute("inherits") != null) {
        return this.GetGenericOrParent().GetRangePattern(greatestDifference, patterns);
      }
      return null;
    }

    public ReadOnlyCollection<CldrDateTimeFormat> GetAvailableDateTimeFormats() {
      if (this.dateTimeFormats == null) {
        ReadOnlyCollection<CldrDateTimeFormat> datePatterns = GetAvailableDateFormats();
        ReadOnlyCollection<CldrDateTimeFormat> timePatterns = Resolve(locale, "gregory").GetAvailableTimeFormats();
        Dictionary<string, string> combinePatterns = Resolve(locale, "generic").GetCombinePatterns();
        List<CldrDateTimeFormat> dateTimeFormats = new List<CldrDateTimeFormat>();
        foreach (CldrDateTimeFormat dateFormat in datePatterns) {
          string combinePattern;
          if (dateFormat.Styles.Weekday == WeekdayStyle.Long) {
            combinePattern = combinePatterns["full"];
          } else {
            switch (dateFormat.Styles.Month) {
              case MonthStyle.Long:
                combinePattern = combinePatterns["full"];
                break;
              case MonthStyle.Short:
                combinePattern = combinePatterns["long"];
                break;
              case MonthStyle.Narrow:
                combinePattern = combinePatterns["medium"];
                break;
              default:
                combinePattern = combinePatterns["short"];
                break;
            }
          }
          foreach (CldrDateTimeFormat timeFormat in timePatterns) {
            List<string> keys = new List<string>(dateFormat.PatternId);
            keys.AddRange(timeFormat.PatternId);
            DateTimePartStyles combinedStyle = new DateTimePartStyles(dateFormat.Styles, timeFormat.Styles);
            CldrDateTimeFormat combinedFormat = new CldrDateTimeFormat(this, combinedStyle, String.Format(combinePattern, timeFormat.Pattern, dateFormat.Pattern), keys);
            dateTimeFormats.Add(combinedFormat);
          }
        }
        this.dateTimeFormats = dateTimeFormats.AsReadOnly();
      }
      return this.dateTimeFormats;
    }

    public ReadOnlyCollection<CldrDateTimeFormat> GetAvailableDateFormats() {
      if (this.dateFormats == null) {
        XElement dateFormats = root.XPathSelectElement("dateFormats");
        if (CldrUtility.IsAlias(dateFormats, out string calendar)) {
          this.dateFormats = Resolve(locale, calendar).GetAvailableDateFormats();
        } else {
          Dictionary<string, XElement> patterns = new Dictionary<string, XElement>();
          foreach (XElement dateFormatLength in dateFormats.Elements("dateFormatLength")) {
            patterns[dateFormatLength.Attribute("type").Value] = dateFormatLength.XPathSelectElement("dateFormat/pattern");
          }
          foreach (XElement dateFormatItem in GetAvailableFormats()) {
            string id = dateFormatItem.Attribute("id").Value;
            if (id.IndexOfAny(requiredDateComponents) >= 0 && id.IndexOfAny(unsupportedDateComponents) < 0) {
              patterns[id] = dateFormatItem;
            }
          }
          Dictionary<DateTimePartStyles, CldrDateTimeFormat> formats = new Dictionary<DateTimePartStyles, CldrDateTimeFormat>();
          Dictionary<DateTimePartStyles, List<string>> patternKeys = new Dictionary<DateTimePartStyles, List<string>>();
          foreach (KeyValuePair<string, XElement> entry in patterns) {
            string pattern = ParseDateTimeFormat(entry.Value.Value, out DateTimePartStyles styles);
            if (!formats.TryGetValue(styles, out CldrDateTimeFormat format)) {
              List<string> patternIds = new List<string>();
              format = new CldrDateTimeFormat(this, styles, pattern, patternIds);
              formats.Add(styles, format);
              patternKeys.Add(styles, patternIds);
            }
            if (patternKeys.TryGetValue(styles, out List<string> keys)) {
              keys.Add(entry.Key);
            }
          }
          if (dateFormats.Attribute("inherits") != null) {
            ReadOnlyCollection<CldrDateTimeFormat> parentFormats = GetGenericOrParent().GetAvailableDateFormats();
            foreach (string key in new[] { "full", "long", "medium", "short" }) {
              if (!patterns.ContainsKey(key)) {
                CldrDateTimeFormat parent = parentFormats.FirstOrDefault(v => v.PatternId.Contains(key));
                if (parent != null) {
                  formats[parent.Styles] = parent;
                }
              }
            }
          }
          this.dateFormats = formats.Values.ToList().AsReadOnly();
        }
      }
      return this.dateFormats;
    }

    public ReadOnlyCollection<CldrDateTimeFormat> GetAvailableTimeFormats() {
      if (this.timeFormats == null) {
        XElement timeFormats = root.XPathSelectElement("timeFormats");
        if (CldrUtility.IsAlias(timeFormats, out string calendar)) {
          this.timeFormats = Resolve(locale, calendar).GetAvailableTimeFormats();
        } else {
          Dictionary<string, XElement> patterns = new Dictionary<string, XElement>();
          foreach (XElement timeFormatLength in timeFormats.Elements("timeFormatLength")) {
            patterns[timeFormatLength.Attribute("type").Value] = timeFormatLength.XPathSelectElement("timeFormat/pattern");
          }
          foreach (XElement dateFormatItem in GetAvailableFormats()) {
            string id = dateFormatItem.Attribute("id").Value;
            if (id.IndexOfAny(requiredTimeComponents) >= 0 && id.IndexOfAny(unsupportedTimeComponents) < 0) {
              patterns[id] = dateFormatItem;
            }
          }
          Dictionary<DateTimePartStyles, CldrDateTimeFormat> formats = new Dictionary<DateTimePartStyles, CldrDateTimeFormat>();
          Dictionary<DateTimePartStyles, List<string>> patternKeys = new Dictionary<DateTimePartStyles, List<string>>();
          foreach (KeyValuePair<string, XElement> entry in patterns) {
            string pattern = ParseDateTimeFormat(entry.Value.Value, out DateTimePartStyles styles);
            if (!formats.TryGetValue(styles, out CldrDateTimeFormat format)) {
              List<string> patternIds = new List<string>();
              format = new CldrDateTimeFormat(this, styles, pattern, patternIds);
              formats.Add(styles, format);
              patternKeys.Add(styles, patternIds);
            }
            if (patternKeys.TryGetValue(styles, out List<string> keys)) {
              keys.Add(entry.Key);
            }
          }
          if (timeFormats.Attribute("inherits") != null) {
            ReadOnlyCollection<CldrDateTimeFormat> parentFormats = GetGenericOrParent().GetAvailableDateFormats();
            foreach (string key in new[] { "full", "long", "medium", "short" }) {
              if (!patterns.ContainsKey(key)) {
                CldrDateTimeFormat parent = parentFormats.FirstOrDefault(v => v.PatternId.Contains(key));
                if (parent != null) {
                  formats[parent.Styles] = parent;
                }
              }
            }
          }
          this.timeFormats = formats.Values.ToList().AsReadOnly();
        }
      }
      return this.timeFormats;
    }

    private IEnumerable<XElement> GetAvailableFormats() {
      XElement dateTimeFormats = root.XPathSelectElement("dateTimeFormats");
      if (CldrUtility.IsAlias(dateTimeFormats, out string calendar)) {
        return Resolve(locale, calendar).GetAvailableFormats();
      }
      XElement availableFormats = dateTimeFormats.Element("availableFormats");
      if (availableFormats == null) {
        return this.Parent.GetAvailableFormats();
      }
      IEnumerable<XElement> dateFormatItems = availableFormats.Elements("dateFormatItem");
      if (availableFormats.Attribute("inherits") != null) {
        return dateFormatItems.Concat(this.Parent.GetAvailableFormats());
      }
      return dateFormatItems;
    }

    private Dictionary<string, string[]> GetMonthNames() {
      return GetLabels(0, "months", "monthContext[@type = 'format']/monthWidth", labelKeysMonth);
    }

    private Dictionary<string, string[]> GetWeekdayNames() {
      return GetLabels(1, "days", "dayContext[@type = 'format']/dayWidth", labelKeysWeekday);
    }

    private Dictionary<string, string[]> GetDayPeriodNames() {
      return GetLabels(2, "dayPeriods", "dayPeriodContext[@type = 'format']/dayPeriodWidth", labelKeysDayPeriod, new[] { "am", "pm" });
    }

    private Dictionary<string, string[]> GetEraNames() {
      return GetLabels(3, "eras", "*", labelKeysEra);
    }

    private Dictionary<string, string[]> GetLabels(int type, string sectionName, string path, Dictionary<string, string> keyMapping, string[] types = null) {
      if (this.labels[type] == null) {
        XElement section = root.XPathSelectElement(sectionName);
        if (section == null) {
          this.labels[type] = this.Parent.GetLabels(type, sectionName, path, keyMapping, types);
        } else if (CldrUtility.IsAlias(section, out string calendar)) {
          this.labels[type] = Resolve(locale, calendar).GetLabels(type, sectionName, path, keyMapping, types);
        } else {
          bool hasInheritedValues = false;
          Dictionary<string, string> copy = new Dictionary<string, string>();
          Dictionary<string, string[]> dict = new Dictionary<string, string[]>();
          foreach (XElement child in section.XPathSelectElements(path)) {
            string labelType = keyMapping[child.Attribute("type")?.Value ?? child.Name.LocalName];
            if (CldrUtility.IsAlias(child, out string use)) {
              copy[labelType] = keyMapping[use];
            } else {
              IEnumerable<XElement> elements = child.XPathSelectElements("*[not(@alt)]");
              string[] labels = (types == null ? elements : types.Select(v => elements.FirstOrDefault(w => w.Attribute("type").Value == v))).Select(v => v?.Value).ToArray();
              hasInheritedValues |= labels.Contains(null);
              dict[labelType] = labels;
            }
          }
          foreach (KeyValuePair<string, string> e in copy) {
            dict[e.Key] = dict[e.Value];
          }
          if (dict.Count < 3 || hasInheritedValues) {
            Dictionary<string, string[]> parent = this.Parent.GetLabels(type, sectionName, path, keyMapping, types);
            if (locale != "root") {
              if (dict.Count == 0) {
                dict = parent;
              } else {
                if (dict.Count < 3) {
                  CldrUtility.CopyPatternFromParent(dict, parent);
                }
              }
            }
            if (!dict.ContainsKey("narrow")) {
              dict["narrow"] = dict.ContainsKey("short") ? dict["short"] : dict["long"];
            }
            if (!dict.ContainsKey("short")) {
              dict["short"] = dict.ContainsKey("narrow") ? dict["narrow"] : dict["long"];
            }
            if (!dict.ContainsKey("long")) {
              dict["long"] = dict.ContainsKey("short") ? dict["short"] : dict["narrow"];
            }
            if (hasInheritedValues) {
              foreach (KeyValuePair<string, string[]> e in dict) {
                CldrUtility.CopyPatternFromParent(e.Value, parent[e.Key]);
              }
            }
          }
          this.labels[type] = dict;
        }
      }
      return this.labels[type];
    }

    private Dictionary<string, string> GetCombinePatterns() {
      if (this.combinePatterns == null) {
        XElement dateTimeFormats = root.XPathSelectElement("dateTimeFormats");
        if (CldrUtility.IsAlias(dateTimeFormats, out string calendar)) {
          this.combinePatterns = Resolve(locale, calendar).GetCombinePatterns();
        } else {
          Dictionary<string, string> combinePatterns = new Dictionary<string, string>();
          foreach (XElement child in dateTimeFormats.Elements("dateTimeFormatLength")) {
            combinePatterns[child.Attribute("type").Value] = Regex.Replace(child.XPathSelectElement("dateTimeFormat/pattern").Value, "'([^']*)'", m => {
              return m.Length == 2 ? "'" : m.Groups[1].Value;
            });
          }
          if (combinePatterns.Count < 4) {
            CldrUtility.CopyPatternFromParent(combinePatterns, this.Parent.GetCombinePatterns());
          }
          this.combinePatterns = combinePatterns;
        }
      }
      return this.combinePatterns;
    }

    private FormattedString GetRangePattern(string formatId, string id) {
      string key = String.Concat(formatId, "/", id);
      if (rangeFormats.TryGetValue(key, out FormattedString cached)) {
        return cached;
      }
      XElement dateTimeFormats = root.XPathSelectElement("dateTimeFormats");
      if (CldrUtility.IsAlias(dateTimeFormats, out string use)) {
        return rangeFormats.GetOrAdd(key, Resolve(locale, use).GetRangePattern(formatId, id));
      }
      XElement pattern = dateTimeFormats.XPathSelectElement(String.Format("intervalFormats/intervalFormatItem[@id = '{0}']/greatestDifference[@id = '{1}']", formatId, id));
      if (pattern == null) {
        return null;
      }

      string patternValue = pattern.Value;
      Match m = Regex.Match(patternValue, "(?<p>(?:'[^']*'|\\s|[^a-zA-Z]|(?<d>[yMLdGEcabBhHkKmszv])\\k<d>*)+)((?:'[^']*'|\\s|[^a-zA-Z])+)\\k<p>");
      if (!m.Success) {
        return new FormattedString(new[] { new FormattedPart(FormattedPartType.Literal, patternValue) });
      }
      List<FormattedPart> parts = new List<FormattedPart>();
      Group repeated = m.Groups["p"];
      if (repeated.Index > 0) {
        parts.Add(GetPlaceholderOrLiteral(patternValue.Substring(0, repeated.Index)));
      }
      string repeatedFormat = ParseDateTimeFormat(repeated.Value, out _);
      parts.Add(new FormattedPart(FormattedPartType.StartRangePlaceholder, repeatedFormat));
      parts.Add(GetPlaceholderOrLiteral(m.Groups[1].Value));
      parts.Add(new FormattedPart(FormattedPartType.EndRangePlaceholder, repeatedFormat));
      if (m.Index + m.Length < patternValue.Length) {
        parts.Add(GetPlaceholderOrLiteral(patternValue.Substring(m.Index + m.Length)));
      }
      return rangeFormats.GetOrAdd(key, new FormattedString(parts));
    }

    private CldrCalendarInfo GetGenericOrParent() {
      if (calendar == "generic") {
        return this.Parent;
      }
      return Resolve(locale, "generic");
    }

    private static FormattedPart GetPlaceholderOrLiteral(string format) {
      string pattern = ParseDateTimeFormat(format, out _);
      return new FormattedPart(pattern != format ? FormattedPartType.SharedPlaceholder : FormattedPartType.Literal, pattern);
    }

    private static string ParseDateTimeFormat(string format, out DateTimePartStyles style) {
      WeekdayStyle weekday = default;
      EraStyle era = default;
      MonthStyle month = default;
      NumericDateTimePartStyle year = default;
      NumericDateTimePartStyle day = default;
      NumericDateTimePartStyle hour = default;
      NumericDateTimePartStyle minute = default;
      NumericDateTimePartStyle second = default;
      TimeZoneNameStyle timeZoneName = default;
      bool isHour12 = false;
      string pattern = Regex.Replace(format, "('([^']*)'|(?<d>[yMLdGEcabBhHkKmszv])(\\k<d>)*)", m => {
        switch (m.Value[0]) {
          case 'y':
            year = m.Length == 2 ? NumericDateTimePartStyle.TwoDigit : NumericDateTimePartStyle.Numeric;
            return "{year}";
          case 'M':
          case 'L':
            switch (m.Length) {
              case 1:
                month = MonthStyle.Numeric;
                break;
              case 2:
                month = MonthStyle.TwoDigit;
                break;
              case 3:
                month = MonthStyle.Short;
                break;
              case 4:
                month = MonthStyle.Long;
                break;
              case 5:
                month = MonthStyle.Narrow;
                break;
            }
            return "{month}";
          case 'd':
            day = m.Length == 2 ? NumericDateTimePartStyle.TwoDigit : NumericDateTimePartStyle.Numeric;
            return "{day}";
          case 'E':
          case 'c':
            weekday = m.Length == 2 ? WeekdayStyle.Short : m.Length == 3 ? WeekdayStyle.Long : WeekdayStyle.Narrow;
            return "{weekday}";
          case 'G':
            era = EraStyle.Long;
            return "{era}";
          case 'a':
          case 'b':
          case 'B':
            isHour12 = true;
            return "{ampm}";
          case 'h':
          case 'H':
          case 'k':
          case 'K':
            hour = m.Length == 2 ? NumericDateTimePartStyle.TwoDigit : NumericDateTimePartStyle.Numeric;
            return "{hour}";
          case 'm':
            minute = m.Length == 2 ? NumericDateTimePartStyle.TwoDigit : NumericDateTimePartStyle.Numeric;
            return "{minute}";
          case 's':
            second = m.Length == 2 ? NumericDateTimePartStyle.TwoDigit : NumericDateTimePartStyle.Numeric;
            return "{second}";
          case 'z':
          case 'v':
            timeZoneName = m.Length == 1 ? TimeZoneNameStyle.Short : TimeZoneNameStyle.Long;
            return "{timeZoneName}";
        }
        return m.Length == 2 ? "'" : m.Groups[1].Value;
      });
      style = new DateTimePartStyles(weekday, era, year, month, day, hour, minute, second, timeZoneName, isHour12);
      return pattern;
    }
  }
}
