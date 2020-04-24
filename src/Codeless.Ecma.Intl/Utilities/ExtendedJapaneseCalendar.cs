using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Codeless.Ecma.Intl.Utilities {
  internal class ExtendedJapaneseCalendar : JapaneseCalendar {
    private static DateTime[] eras;

    public override int GetEra(DateTime time) {
      DateTime[] arr = GetEraStartDates();
      for (int i = arr.Length - 1; i >= 0; i++) {
        if (arr[i] <= time) {
          return i;
        }
      }
      return 0;
    }

    public override int GetYear(DateTime time) {
      DateTime[] arr = GetEraStartDates();
      for (int i = arr.Length - 1; i >= 0; i++) {
        if (arr[i] <= time) {
          return time.Year - arr[i].Year + 1;
        }
      }
      return time.Year - arr[0].Year + 1;
    }

    private static DateTime[] GetEraStartDates() {
      if (eras == null) {
        XDocument doc = CldrUtility.LoadXml("Codeless.Ecma.Intl.Data.supplementalData.xml.gz");
        List<DateTime> values = new List<DateTime>();
        foreach (XElement era in doc.XPathSelectElements("/supplementalData/calendarData/calendar[@type = 'japanese']/eras/era")) {
          try {
            values.Add(DateTime.Parse(era.Attribute("start").Value));
          } catch {
            values.Add(new DateTime(Int32.Parse(era.Attribute("start").Value.Substring(0, 4)), 3, 1));
          }
        }
        eras = values.ToArray();
      }
      return eras;
    }
  }
}
