using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Codeless.Ecma.Intl.Utilities {
  internal static class CldrUtility {
    private static readonly ConcurrentDictionary<string, XDocument> loadedDocuments = new ConcurrentDictionary<string, XDocument>();
    private static Dictionary<string, string> parentLocales;

    public const string InheritedValuePlaceholder = "↑↑↑";

    public static XDocument LoadXml(string resourceName) {
      return loadedDocuments.GetOrAdd(resourceName, _ => {
        using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)) {
          using (GZipStream gZip = new GZipStream(stream, CompressionMode.Decompress)) {
            return XDocument.Load(new StreamReader(gZip));
          }
        }
      });
    }

    public static bool IsAlias(XElement element, out string use) {
      Guard.ArgumentNotNull(element, "element");
      XAttribute aliasOf = element.Attribute("use");
      if (aliasOf != null) {
        use = aliasOf.Value;
        return true;
      }
      use = null;
      return false;
    }

    public static string GetParentLocale(string locale) {
      Guard.ArgumentNotNull(locale, "locale");
      locale = BcpLanguageTag.Parse(locale).Canonicalize().ToString();
      if (CldrUtility.parentLocales == null) {
        Dictionary<string, string> parentLocales = new Dictionary<string, string>();
        XDocument doc = LoadXml("Codeless.Ecma.Intl.Data.supplementalData.xml.gz");
        foreach (XElement elm in doc.XPathSelectElements("/supplementalData/parentLocales/parentLocale")) {
          string parent = elm.Attribute("parent").Value;
          foreach (string child in elm.Attribute("locales").Value.Split(' ')) {
            parentLocales[child] = parent;
          }
        }
        CldrUtility.parentLocales = parentLocales;
      }
      if (CldrUtility.parentLocales.TryGetValue(locale, out string p)) {
        return p;
      }
      int pos = locale.LastIndexOf('-');
      if (pos > 0) {
        return locale.Substring(0, pos);
      }
      return locale == "root" ? null : "root";
    }

    public static T GetParentPatterns<T>(string locale, Func<string, T> factory, string fallback = null) {
      Guard.ArgumentNotNull(locale, "locale");
      Guard.ArgumentNotNull(factory, "factory");
      string parentLocale = GetParentLocale(locale);
      if (parentLocale != null) {
        return factory(parentLocale);
      }
      throw new InvalidOperationException(String.Format("Unable to find data for locale {0}", locale));
    }

    public static T GetParentPatterns<T>(string locale, string type, Func<string, string, T> factory, string fallback = null) {
      Guard.ArgumentNotNull(locale, "locale");
      Guard.ArgumentNotNull(type, "type");
      Guard.ArgumentNotNull(factory, "factory");
      int pos;
      if ((pos = type.LastIndexOf('-')) > 0) {
        return factory(locale, type.Substring(0, pos));
      }
      if (fallback != null && type != fallback) {
        return factory(locale, fallback);
      }
      string parentLocale = GetParentLocale(locale);
      if (parentLocale != null) {
        return factory(parentLocale, type);
      }
      throw new InvalidOperationException(String.Format("Unable to find data for locale {0} and type {1}", locale, type));
    }

    public static string GetDefaultLanguage(string territory) {
      Guard.ArgumentNotNull(territory, "territory");
      XDocument supplementalData = LoadXml("Codeless.Ecma.Intl.Data.supplementalData.xml.gz");
      XElement languagePopulation = supplementalData.XPathSelectElements(String.Format("/supplementalData/territoryInfo/territory[@type = '{0}']/languagePopulation", territory)).FirstOrDefault();
      if (languagePopulation == null) {
        throw new ArgumentOutOfRangeException("territoryCode");
      }
      return IntlUtility.CanonicalizeLanguageTag(languagePopulation.Attribute("type").Value);
    }

    public static HourCycle GetDefaultHourCycle(string territory) {
      Guard.ArgumentNotNull(territory, "territory");
      XDocument supplementalData = LoadXml("Codeless.Ecma.Intl.Data.supplementalData.xml.gz");
      IEnumerable<XElement> hours = supplementalData.XPathSelectElements("/supplementalData/timeData/hours");
      XElement child = hours.FirstOrDefault(v => v.Attribute("regions").Value.Split(' ').Contains(territory)) ??
                       hours.First(v => v.Attribute("regions").Value == "001");
      switch (child.Attribute("preferred").Value) {
        case "H":
          return HourCycle.Hour23;
        case "h":
          return HourCycle.Hour12;
        case "K":
          return HourCycle.Hour11;
        case "k":
          return HourCycle.Hour24;
      }
      return HourCycle.Hour23;
    }

    public static string[] GetPreferredCalenderTypes(string territory) {
      Guard.ArgumentNotNull(territory, "territory");
      XDocument supplementalData = LoadXml("Codeless.Ecma.Intl.Data.supplementalData.xml.gz");
      IEnumerable<XElement> calendarPreferences = supplementalData.XPathSelectElements("/supplementalData/calendarPreferenceData/calendarPreference");
      XElement child = calendarPreferences.FirstOrDefault(v => v.Attribute("territories").Value.Split(' ').Contains(territory)) ??
                       calendarPreferences.First(v => v.Attribute("territories").Value == "001");
      return child.Attribute("ordering").Value.Split(' ');
    }

    public static void CopyPatternFromParent<T>(IList<T> dict, IList<T> parent) {
      Guard.ArgumentNotNull(dict, "dict");
      Guard.ArgumentNotNull(parent, "parent");
      for (int i = dict.Count - 1; i >= 0; i--) {
        if (dict[i] == null) {
          dict[i] = parent[i];
        }
      }
    }

    public static void CopyPatternFromParent<TKey, TValue>(Dictionary<TKey, TValue> dict, IDictionary<TKey, TValue> parent) {
      Guard.ArgumentNotNull(dict, "dict");
      Guard.ArgumentNotNull(parent, "parent");
      foreach (KeyValuePair<TKey, TValue> e in parent) {
        if (!dict.TryGetValue(e.Key, out TValue value) || value == null) {
          dict[e.Key] = e.Value;
        }
      }
    }
  }
}
