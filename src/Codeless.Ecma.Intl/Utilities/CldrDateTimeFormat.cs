using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace Codeless.Ecma.Intl.Utilities {
  [DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
  internal class CldrDateTimeFormat {
    public CldrDateTimeFormat(CldrCalendarInfo info, DateTimePartStyles style, string pattern, List<string> patternId) {
      Guard.ArgumentNotNull(info, "info");
      Guard.ArgumentNotNull(pattern, "pattern");
      Guard.ArgumentNotNull(patternId, "patternId");
      this.Styles = style;
      this.CalendarInfo = info;
      this.Pattern = pattern;
      this.PatternId = patternId.AsReadOnly();
    }

    public CldrCalendarInfo CalendarInfo { get; }

    public DateTimePartStyles Styles { get; }

    public IList<string> PatternId { get; }

    public string Pattern { get; }

    public string[] MonthNames {
      get { return this.CalendarInfo.GetMonthNames(this.Styles.Month); }
    }

    public string[] EraNames {
      get { return this.CalendarInfo.GetEraNames(this.Styles.Era); }
    }

    public string[] DayPeriodNames {
      get { return this.CalendarInfo.DayPeriodNames; }
    }

    public string[] WeekdayNames {
      get { return this.CalendarInfo.GetWeekdayNames(this.Styles.Weekday); }
    }

    public FormattedString GetDefaultRangePattern() {
      return this.CalendarInfo.DefaultRangePattern;
    }

    public FormattedString GetRangePattern(FormattedPartType greatestDifference) {
      return this.CalendarInfo.GetRangePattern(greatestDifference, this.PatternId);
    }

    private string GetDebuggerDisplay() {
      return Regex.Replace(Pattern, "{([^}]+)}", m => {
        switch (m.Value) {
          case "{weekday}":
            return this.Styles.Weekday == WeekdayStyle.Long ? "EEE" : this.Styles.Weekday == WeekdayStyle.Short ? "EE" : "E";
          case "{era}":
            return this.Styles.Era == EraStyle.Long ? "GGG" : this.Styles.Era == EraStyle.Short ? "GG" : "G";
          case "{year}":
            return this.Styles.Year == NumericDateTimePartStyle.TwoDigit ? "yy" : "yyyy";
          case "{month}":
            return this.Styles.Month == MonthStyle.Long ? "MMMM" : this.Styles.Month == MonthStyle.Short ? "MMM" : this.Styles.Month == MonthStyle.TwoDigit ? "MM" : "M";
          case "{day}":
            return this.Styles.Day == NumericDateTimePartStyle.TwoDigit ? "dd" : "d";
          case "{hour}":
            return this.Styles.Hour == NumericDateTimePartStyle.TwoDigit ? "hh" : "h";
          case "{minute}":
            return this.Styles.Minute == NumericDateTimePartStyle.TwoDigit ? "mm" : "m";
          case "{second}":
            return this.Styles.Second == NumericDateTimePartStyle.TwoDigit ? "ss" : "s";
          case "{timeZoneName}":
            return this.Styles.TimeZoneName == TimeZoneNameStyle.Long ? "zzzz" : "z";
          case "{ampm}":
            return "a";
        }
        return m.Value;
      });
    }
  }
}
