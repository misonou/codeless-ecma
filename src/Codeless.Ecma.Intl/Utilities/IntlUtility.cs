using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TimeZoneConverter;

namespace Codeless.Ecma.Intl.Utilities {
  public static class IntlUtility {
    private static readonly ICollection<string> systemLocales = CultureInfo.GetCultures(CultureTypes.SpecificCultures).Select(v => CanonicalizeLanguageTag(v.Name)).ToArray();
    private static readonly Dictionary<string, string> ianaTimezones = new Dictionary<string, string>();
    private static readonly Dictionary<string, string> ianaTimezonesLinks = new Dictionary<string, string>();

    internal static readonly Dictionary<string, string> SupportedUnits = new Dictionary<string, string> {
      ["acre"] = "area-acre",
      ["bit"] = "digital-bit",
      ["byte"] = "digital-byte",
      ["celsius"] = "temperature-celsius",
      ["centimeter"] = "length-centimeter",
      ["day"] = "duration-day",
      ["degree"] = "angle-degree",
      ["fahrenheit"] = "temperature-fahrenheit",
      ["fluid-ounce"] = "volume-fluid-ounce",
      ["foot"] = "length-foot",
      ["gallon"] = "volume-gallon",
      ["gigabit"] = "digital-gigabit",
      ["gigabyte"] = "digital-gigabyte",
      ["gram"] = "mass-gram",
      ["hectare"] = "area-hectare",
      ["hour"] = "duration-hour",
      ["inch"] = "length-inch",
      ["kilobit"] = "digital-kilobit",
      ["kilobyte"] = "digital-kilobyte",
      ["kilogram"] = "mass-kilogram",
      ["kilometer"] = "length-kilometer",
      ["liter"] = "volume-liter",
      ["megabit"] = "digital-megabit",
      ["megabyte"] = "digital-megabyte",
      ["meter"] = "length-meter",
      ["mile"] = "length-mile",
      ["mile-scandinavian"] = "length-mile-scandinavian",
      ["milliliter"] = "volume-milliliter",
      ["millimeter"] = "length-millimeter",
      ["millisecond"] = "duration-millisecond",
      ["minute"] = "duration-minute",
      ["month"] = "duration-month",
      ["ounce"] = "mass-ounce",
      ["percent"] = "concentr-percent",
      ["petabyte"] = "digital-petabyte",
      ["pound"] = "mass-pound",
      ["second"] = "duration-second",
      ["stone"] = "mass-stone",
      ["terabit"] = "digital-terabit",
      ["terabyte"] = "digital-terabyte",
      ["week"] = "duration-week",
      ["yard"] = "length-yard",
      ["year"] = "duration-year",
    };

    internal static readonly Dictionary<string, Calendar> SupportedCalendars = new Dictionary<string, Calendar> {
      ["gregory"] = new GregorianCalendar(),
      ["chinese"] = new ChineseLunisolarCalendar(),
      ["japanese"] = new ExtendedJapaneseCalendar(),
      ["islamic-umalqura"] = new UmAlQuraCalendar(),
      ["buddhist"] = new ThaiBuddhistCalendar(),
      ["dangi"] = new KoreanLunisolarCalendar(),
      ["roc"] = new TaiwanCalendar()
    };

    internal static readonly Dictionary<string, int> SupportedCollations = new Dictionary<string, int> {
      ["standard"] = 0,
      ["phonebk"] = 0x00010407, // de-De
      ["pinyin"] = 0x00030404,  // zh
      ["trad"] = 0x0000040A,    // es
      ["stroke"] = 0
    };

    internal static readonly Dictionary<string, string> SupportedNumberingSystems = new Dictionary<string, string> {
      ["latn"] = "en-US"
    };

    internal static readonly Dictionary<string, string[]> SupportedValues = new Dictionary<string, string[]> {
      ["ca"] = SupportedCalendars.Keys.ToArray(),
      ["nu"] = SupportedNumberingSystems.Keys.ToArray(),
      ["co"] = SupportedCollations.Keys.ToArray(),
      ["kn"] = new[] { "" },
      ["kf"] = new[] { "upper", "lower", "false" },
      ["hc"] = new[] { "h11", "h12", "h23", "h24" }
    };

    internal static ICollection<string> SystemLocales => systemLocales;

    static IntlUtility() {
      foreach (string tz in TZConvert.KnownIanaTimeZoneNames) {
        ianaTimezones.Add(tz.ToLowerInvariant(), tz);
      }
      using (Stream compressedStream = typeof(TZConvert).Assembly.GetManifestResourceStream("TimeZoneConverter.Data.Aliases.csv.gz"))
      using (GZipStream stream = new GZipStream(compressedStream, CompressionMode.Decompress))
      using (StreamReader reader = new StreamReader(stream)) {
        string line;
        while ((line = reader.ReadLine()) != null) {
          string[] key = line.Split(',');
          foreach (string value in key[1].Split()) {
            ianaTimezonesLinks.Add(value.ToLowerInvariant(), key[0]);
          }
        }
      }
    }

    public static bool IsWellFormedCurrencyCode(string currency) {
      Guard.ArgumentNotNull(currency, "currency");
      return Regex.IsMatch(currency, "[a-zA-Z]{3}");
    }

    public static bool IsStructurallyValidLanguageTag(string locale) {
      return BcpLanguageTag.IsValid(locale);
    }

    public static bool IsValidTimeZoneName(string timezone) {
      Guard.ArgumentNotNull(timezone, "timezone");
      return ianaTimezones.ContainsKey(timezone.ToLowerInvariant());
    }

    public static bool IsWellFormedUnitIdentifier(string unit) {
      Guard.ArgumentNotNull(unit, "unit");
      unit = unit.ToLowerInvariant();
      if (SupportedUnits.ContainsKey(unit)) {
        return true;
      }
      int pos = unit.IndexOf("-per-");
      if (pos > 0) {
        return SupportedUnits.ContainsKey(unit.Substring(0, pos)) && SupportedUnits.ContainsKey(unit.Substring(pos + 5));
      }
      return false;
    }

    public static string CanonicalizeLanguageTag(string locale) {
      BcpLanguageTag tag = BcpLanguageTag.Parse(locale);
      if (tag.SingletonSubtags.Count > 0) {
        if (tag.SingletonSubtags.Count > 1 || !tag.SingletonSubtags.ContainsKey("u")) {
          BcpLanguageTagBuilder builder = new BcpLanguageTagBuilder(tag);
          builder.SingletonSubtags.Clear();
          builder.SingletonSubtags["u"] = tag.SingletonSubtags["u"];
          return builder.Canonicalize().ToString();
        }
      }
      return tag.Canonicalize().ToString();
    }

    public static string CanonicalizeLanguageTag(EcmaValue value) {
      if (value.Type != EcmaValueType.String && value.Type != EcmaValueType.Object) {
        throw new EcmaTypeErrorException("Language ID should be string or object");
      }
      if (value.GetUnderlyingObject() is Locale locale) {
        return locale.LocaleString;
      }
      string tag = value.ToStringOrThrow();
      if (!IsStructurallyValidLanguageTag(tag)) {
        throw new EcmaRangeErrorException("Invalid language tag: {0}", tag);
      }
      return CanonicalizeLanguageTag(tag);
    }

    public static ICollection<string> CanonicalizeLocaleList(EcmaValue locales) {
      if (locales == default) {
        return new List<string>();
      }
      if (locales.Type == EcmaValueType.String) {
        string s = locales.ToString();
        if (!IsStructurallyValidLanguageTag(s)) {
          throw new EcmaRangeErrorException("Invalid language tag: {0}", s);
        }
        return new List<string> { CanonicalizeLanguageTag(s) };
      }
      HashSet<string> list = new HashSet<string>();
      RuntimeObject obj = locales.ToObject();
      if (obj is Locale locale) {
        return new List<string> { locale.LocaleString };
      }
      long len = obj[WellKnownProperty.Length].ToLength();
      for (long i = 0; i < len; i++) {
        if (obj.HasProperty(i)) {
          list.Add(CanonicalizeLanguageTag(obj[i]));
        }
      }
      return list;
    }

    public static ICollection<string> CanonicalizeLocaleList(ICollection<string> locales) {
      Guard.ArgumentNotNull(locales, "locales");
      HashSet<string> list = new HashSet<string>();
      foreach (string value in locales) {
        if (!IsStructurallyValidLanguageTag(value)) {
          throw new EcmaRangeErrorException("Invalid language tag: {0}", value);
        }
        list.Add(CanonicalizeLanguageTag(value));
      }
      return list;
    }

    public static string CanonicalizeTimeZoneName(string timezone) {
      Guard.ArgumentNotNull(timezone, "timezone");
      string normalized;
      if (!ianaTimezonesLinks.TryGetValue(timezone.ToLowerInvariant(), out normalized) &&
          !ianaTimezones.TryGetValue(timezone.ToLowerInvariant(), out normalized)) {
        throw new EcmaRangeErrorException("Invalid timezone: {0}", timezone);
      }
      if (normalized == "Etc/UTC" || normalized == "Etc/GMT") {
        return "UTC";
      }
      return normalized;
    }

    public static string GetBestAvailableLocale(ICollection<string> availableLocales, ICollection<string> requestedLocales, LocaleMatcher lookupMatcher, out string extension) {
      Guard.ArgumentNotNull(availableLocales, "availableLocales");
      Guard.ArgumentNotNull(requestedLocales, "requestedLocales");
      ICollection<string> candidates = requestedLocales.Count == 0 ? new[] { IntlContext.DefaultLocale } : requestedLocales;
      foreach (string locale in candidates) {
        string normalized = RemoveUnicodeExtensions(locale);
        string matched = GetBestAvailableLocale(availableLocales, normalized);
        extension = locale.Substring(normalized.Length);
        if (matched != null) {
          return matched;
        }
        if (lookupMatcher == LocaleMatcher.BestFit) {
          BcpLanguageTagBuilder candidate = new BcpLanguageTagBuilder(BcpLanguageTag.Parse(normalized).Maximize());
          matched = GetBestAvailableLocale(availableLocales, candidate.ToString());
          if (matched == null && candidate.Script != "") {
            candidate.Script = "";
            matched = GetBestAvailableLocale(availableLocales, candidate.ToString());
          }
          if (matched != null) {
            return matched;
          }
        }
      }
      if (requestedLocales.Count > 0) {
        return GetBestAvailableLocale(availableLocales, new string[0], lookupMatcher, out extension);
      }
      extension = "";
      return availableLocales.First();
    }

    public static string GetBestAvailableLocale(ICollection<string> availableLocales, string locale) {
      Guard.ArgumentNotNull(availableLocales, "availableLocales");
      Guard.ArgumentNotNull(locale, "locale");
      while (true) {
        if (availableLocales.Contains(locale)) {
          return locale;
        }
        int pos = locale.LastIndexOf('-');
        if (pos < 0) {
          return null;
        }
        if (pos >= 2 && locale[pos - 2] == '-') {
          pos -= 2;
        }
        locale = locale.Substring(0, pos);
      }
    }

    public static string RemoveUnicodeExtensions(string locale) {
      Guard.ArgumentNotNull(locale, "locale");
      int len = locale.Length;
      int pos = locale.LastIndexOf('-');
      while (pos > 0) {
        if (pos >= 2 && locale[pos - 2] == '-') {
          pos -= 2;
          len = pos;
        }
        pos = locale.LastIndexOf('-', pos - 1);
      }
      return locale.Substring(0, len);
    }

    public static string GetUnicodeExtensionValue(string extension, string key) {
      Guard.ArgumentNotNull(extension, "extension");
      Guard.ArgumentNotNull(key, "key");
      int pos = extension.IndexOf("-" + key + "-");
      if (pos < 0) {
        pos = extension.IndexOf("-" + key);
        return pos >= 0 && pos + 3 == extension.Length ? "" : null;
      }
      int start = pos + 4;
      int end = start;
      while (true) {
        pos = extension.IndexOf('-', end);
        if (pos < 0) {
          return extension.Substring(start);
        }
        if (pos - end == 2) {
          return extension.Substring(start, end - start);
        }
        end = pos + 1;
      }
    }

    public static List<string> GetSupportedLocales(ICollection<string> availableLocales, ICollection<string> requestedLocales, IntlProviderOptions options) {
      if (options.LocaleMatcher == LocaleMatcher.BestFit) {
        return BestFitSupportedLocales(availableLocales, requestedLocales);
      }
      return LookupSupportedLocales(availableLocales, requestedLocales);
    }

    public static List<string> LookupSupportedLocales(ICollection<string> availableLocales, ICollection<string> requestedLocales) {
      Guard.ArgumentNotNull(availableLocales, "availableLocales");
      Guard.ArgumentNotNull(requestedLocales, "requestedLocales");
      List<string> result = new List<string>();
      foreach (string locale in requestedLocales) {
        string matched = GetBestAvailableLocale(availableLocales, RemoveUnicodeExtensions(locale));
        if (matched != null) {
          result.Add(matched);
        }
      }
      return result;
    }

    public static List<string> BestFitSupportedLocales(ICollection<string> availableLocales, ICollection<string> requestedLocales) {
      return LookupSupportedLocales(availableLocales, requestedLocales);
    }

    public static string GetDefaultExtensionValue(string key) {
      Guard.ArgumentNotNull(key, "key");
      switch (key) {
        case "hc":
          return IntlProviderOptions.ToStringValue(CldrUtility.GetDefaultHourCycle(IntlContext.RegionCode));
        case "ca":
          foreach (string type in CldrUtility.GetPreferredCalenderTypes(IntlContext.RegionCode)) {
            if (SupportedCalendars.ContainsKey(type)) {
              return type;
            }
          }
          break;
      }
      if (SupportedValues.ContainsKey(key)) {
        return SupportedValues[key][0];
      }
      return null;
    }

    public static string GetRegionCodeFromTimeZone(string ianaTimeZone) {
      Guard.ArgumentNotNull(ianaTimeZone, "ianaTimeZone");
      foreach (KeyValuePair<string, string> e in (IDictionary<string, string>)typeof(TZConvert).GetField("WindowsMap", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null)) {
        if (e.Value.Equals(ianaTimeZone, StringComparison.OrdinalIgnoreCase)) {
          return e.Key.Split('|')[0];
        }
      }
      return "001";
    }

    public static string GetTimeZoneFromRegionCode(string regionCode) {
      Guard.ArgumentNotNull(regionCode, "regionCode");
      if (regionCode == "001") {
        throw new ArgumentOutOfRangeException("Region code cannot be 001", "regionCode");
      }
      foreach (KeyValuePair<string, string> e in (IDictionary<string, string>)typeof(TZConvert).GetField("WindowsMap", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null)) {
        if (e.Key.Split('|')[0].Equals(regionCode, StringComparison.OrdinalIgnoreCase)) {
          return e.Value;
        }
      }
      return null;
    }
  }
}
