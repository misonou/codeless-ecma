using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Codeless.Ecma.Intl.Utilities {
  internal static class CldrTimeZoneNames {
    private static readonly XDocument timeZones = CldrUtility.LoadXml("Codeless.Ecma.Intl.Data.timeZoneNames.xml.gz");
    private static readonly XDocument metaZones = CldrUtility.LoadXml("Codeless.Ecma.Intl.Data.metaZones.xml.gz");

    public static string Resolve(string locale, string ianaTimeZone, DateTime date, TimeZoneNameStyle style) {
      if (ianaTimeZone == "UTC" || ianaTimeZone == "Etc/UTC") {
        return GetMetaZoneName(locale, "Etc/UTC", style, false);
      }
      TimeZoneInfo info = TimeZoneConverter.TZConvert.GetTimeZoneInfo(ianaTimeZone);
      XElement metaZone = metaZones.XPathSelectElement(String.Format("/supplementalData/metaZones/metazoneInfo/timezone[@type = '{0}']", ianaTimeZone));
      if (metaZone != null) {
        IEnumerable<XElement> child = metaZone.Elements("usesMetazone");
        XElement usesMetazone = child.First(v => (v.Attribute("from") == null || date >= DateTime.Parse(v.Attribute("from").Value)) && (v.Attribute("to") == null || date < DateTime.Parse(v.Attribute("to").Value)));
        return GetMetaZoneName(locale, usesMetazone.Attribute("mzone").Value, style, info.IsDaylightSavingTime(date));
      }
      string hourFormat = GetHourFormat(locale);
      string format = hourFormat.Split(';')[info.BaseUtcOffset >= TimeSpan.Zero ? 0 : 1];
      return Regex.Replace(format, "H+|m+", m => {
        if (m.Value[0] == 'm') {
          return info.BaseUtcOffset.Minutes.ToString("00", CultureInfo.InvariantCulture);
        }
        return info.BaseUtcOffset.Hours.ToString(m.Length == 2 ? "00" : "0", CultureInfo.InvariantCulture);
      });
    }

    private static string GetMetaZoneName(string locale, string metaZone, TimeZoneNameStyle style, bool isDaylightSaving) {
      XElement metazoneNames = timeZones.XPathSelectElement(String.Format("/root/timeZoneNames[@locale = '{0}']/*[(local-name() = 'metazone' or local-name() = 'zone') and @type = '{1}']", locale, metaZone));
      if (metazoneNames != null) {
        if (style == TimeZoneNameStyle.Short) {
          metazoneNames = metazoneNames.Element("short") ?? metazoneNames.Element("long");
        } else {
          metazoneNames = metazoneNames.Element("long");
        }
        XElement name = metazoneNames.Element(isDaylightSaving ? "daylight" : "standard");
        if (name != null) {
          return name.Value;
        }
      }
      return CldrUtility.GetParentPatterns(locale, s => GetMetaZoneName(s, metaZone, style, isDaylightSaving));
    }

    private static string GetHourFormat(string locale) {
      XElement hourFormat = timeZones.XPathSelectElement(String.Format("/root/timeZoneNames[@locale = '{0}']/hourFormat", locale));
      if (hourFormat != null) {
        return hourFormat.Value;
      }
      return CldrUtility.GetParentPatterns(locale, GetHourFormat);
    }
  }
}
