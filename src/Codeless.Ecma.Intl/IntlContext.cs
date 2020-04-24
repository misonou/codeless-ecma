using Codeless.Ecma.Intl.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeZoneConverter;

namespace Codeless.Ecma.Intl {
  public static class IntlContext {
    private static string defaultLocale;
    private static string defaultTimeZone;
    private static string regionCode;

    public static string DefaultLocale {
      get {
        if (defaultLocale == null) {
          Initialize();
        }
        return defaultLocale;
      }
      set {
        if (!IntlUtility.IsStructurallyValidLanguageTag(value)) {
          throw new FormatException(String.Format("'{0}' is not a valid BCP-47 language tag", value));
        }
        defaultLocale = IntlUtility.CanonicalizeLanguageTag(value);
      }
    }

    public static string DefaultTimeZone {
      get {
        if (defaultTimeZone == null) {
          Initialize();
        }
        return defaultTimeZone;
      }
      set {
        if (!IntlUtility.IsValidTimeZoneName(value)) {
          throw new FormatException(String.Format("'{0}' is not a valid IANA time zone name", value));
        }
        defaultTimeZone = IntlUtility.CanonicalizeTimeZoneName(value);
      }
    }

    public static string RegionCode {
      get {
        if (regionCode == null) {
          Initialize();
        }
        return regionCode;
      }
      set {
        regionCode = value;
      }
    }

    private static void Initialize() {
      lock (typeof(IntlContext)) {
        bool isTimeZoneAutoDetected = defaultTimeZone == null;
        if (defaultTimeZone == null) {
          TimeZoneInfo localTimeZone = TimeZoneInfo.Local;
          if (IntlUtility.IsValidTimeZoneName(localTimeZone.Id)) {
            defaultTimeZone = IntlUtility.CanonicalizeTimeZoneName(localTimeZone.Id);
          } else {
            if (Environment.OSVersion.Platform != PlatformID.Unix && TZConvert.TryWindowsToIana(localTimeZone.StandardName, out string result)) {
              defaultTimeZone = IntlUtility.CanonicalizeTimeZoneName(result);
            }
            if (defaultTimeZone == null) {
              TimeZoneInfo[] systemIanaTimezones = TimeZoneInfo.GetSystemTimeZones().Where(v => IntlUtility.IsValidTimeZoneName(v.Id)).ToArray();
              TimeZoneInfo matched = systemIanaTimezones.FirstOrDefault(v => v.HasSameRules(localTimeZone)) ?? systemIanaTimezones.FirstOrDefault(v => v.BaseUtcOffset == localTimeZone.BaseUtcOffset);
              if (matched != null) {
                defaultTimeZone = IntlUtility.CanonicalizeTimeZoneName(matched.Id);
              }
            }
          }
          if (defaultTimeZone == null) {
            defaultTimeZone = "UTC";
          }
        }
        if (regionCode == null) {
          regionCode = RegionInfo.CurrentRegion.TwoLetterISORegionName;
          if (regionCode == "IV") {
            regionCode = IntlUtility.GetRegionCodeFromTimeZone(defaultTimeZone);
          }
        }
        if (defaultLocale == null) {
          string systemLocale = CultureInfo.CurrentUICulture.Name;
          if (String.IsNullOrEmpty(systemLocale)) {
            defaultLocale = CldrUtility.GetDefaultLanguage(regionCode);
          } else {
            defaultLocale = IntlUtility.CanonicalizeLanguageTag(systemLocale);
          }
        }
        if (isTimeZoneAutoDetected && IntlUtility.GetRegionCodeFromTimeZone(defaultTimeZone) == "001" && regionCode != "001") {
          string timeZone = IntlUtility.GetTimeZoneFromRegionCode(regionCode);
          if (timeZone != null && TZConvert.IanaToWindows(timeZone) == TZConvert.IanaToWindows(defaultTimeZone)) {
            defaultTimeZone = timeZone;
          }
        }
      }
    }
  }
}
