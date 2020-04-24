using Codeless.Ecma.Intl.Utilities;
using Codeless.Ecma.Runtime;
using Codeless.Ecma.Runtime.Intrinsics;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using TimeZoneConverter;

namespace Codeless.Ecma.Intl {
  public class DateTimeFormat : IntlProvider<DateTimeFormatOptions> {
    private const long MinTime = -8640000000000000;
    private const long MaxTime = 8640000000000000;

    private static readonly ConcurrentDictionary<string, FormattedString> parsedPatterns = new ConcurrentDictionary<string, FormattedString>();
    private static readonly DateTime unixEpochUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly string[] relevantExtensionKeys = new[] { "ca", "nu", "hc" };

    private CldrDateTimeFormat format;
    private Calendar calendar;
    private string[] monthNames;
    private string[] dayNames;
    private string[] dayPeriodNames;
    private string[] eraNames;

    public DateTimeFormat()
      : base(IntlModule.DateTimeFormatPrototype) { }

    public DateTimeFormat(string locale)
      : base(IntlModule.DateTimeFormatPrototype, locale) { }

    public DateTimeFormat(string locale, DateTimeFormatOptions options)
      : base(IntlModule.DateTimeFormatPrototype, locale, options) { }

    public DateTimeFormat(ICollection<string> locale)
      : base(IntlModule.DateTimeFormatPrototype, locale) { }

    public DateTimeFormat(ICollection<string> locale, DateTimeFormatOptions options)
      : base(IntlModule.DateTimeFormatPrototype, locale, options) { }

    public string Locale { get; private set; }
    public string Calendar { get; private set; }
    public string NumberingSystem { get; private set; }
    public string TimeZone { get; private set; }
    public bool Hour12 { get; private set; }
    public HourCycle HourCycle { get; private set; }
    public DateTimeFormatMatcher FormatMatcher { get; private set; }
    public DateTimePartStyles DateTimePartStyles { get; private set; }
    public EcmaValue BoundFormat { get; private set; }

    protected override ICollection<string> AvailableLocales => CldrCalendarInfo.AvailableLocales;

    protected override void InitInternal(ICollection<string> locales, DateTimeFormatOptions options) {
      Hashtable ht = new Hashtable();
      LocaleMatcher matcher = options.LocaleMatcher;
      ht["ca"] = options.Calendar;
      ht["nu"] = options.NumberingSystem;

      bool? hour12 = options.Hour12;
      HourCycle hourCycle = options.HourCycle;
      if (hour12 != default) {
        hourCycle = HourCycle.Unspecified;
      }
      ht["hc"] = IntlProviderOptions.ToStringValue(hourCycle);
      this.Locale = ResolveLocale(locales, matcher, relevantExtensionKeys, ht, out ht);
      this.Calendar = (string)ht["ca"];
      this.NumberingSystem = (string)ht["nu"];

      string tz = options.TimeZone;
      if (tz != null) {
        if (!IntlUtility.IsValidTimeZoneName(tz)) {
          throw new EcmaRangeErrorException("Invalid time zone specified: {0}", tz);
        }
        this.TimeZone = IntlUtility.CanonicalizeTimeZoneName(tz);
      } else {
        this.TimeZone = IntlContext.DefaultTimeZone;
      }
      this.FormatMatcher = options.FormatMatcher;

      DateTimePartStyles styles = new DateTimePartStyles(options);
      HourCycle finalHourCycle = IntlProviderOptions.ParseEnum<HourCycle>((string)ht["hc"]);
      this.HourCycle = hour12 == default ? finalHourCycle : NormalizeHourCycle(finalHourCycle, hour12.Value);
      this.Hour12 = this.HourCycle == HourCycle.Hour11 || this.HourCycle == HourCycle.Hour12;

      this.format = GetBestFormat(styles);
      this.calendar = IntlUtility.SupportedCalendars[this.Calendar];
      this.DateTimePartStyles = format.Styles;
      this.BoundFormat = Literal.FunctionLiteral(this.FormatInternal);
    }

    public FormattedString Format(EcmaValue date) {
      EnsureInitialized();
      date = date == default ? DateConstructor.Now() : date.ToNumber();
      if (date < MinTime || date > MaxTime) {
        throw new EcmaRangeErrorException("Invalid time value");
      }
      TimeZoneInfo timezone = TZConvert.GetTimeZoneInfo(this.TimeZone);
      DateTime dt = unixEpochUtc.AddMilliseconds(date.ToInt64() + timezone.BaseUtcOffset.TotalMilliseconds);
      return FormatDateTime(dt, format.Pattern);
    }

    public FormattedString FormatRange(EcmaValue startDate, EcmaValue endDate) {
      return FormatRange(startDate, endDate, out _);
    }

    public FormattedString FormatRange(EcmaValue startDate, EcmaValue endDate, out string[] annotations) {
      EnsureInitialized();
      if (startDate == default || endDate == default) {
        throw new EcmaTypeErrorException("Invalid time value");
      }
      EcmaValue x = startDate.ToNumber();
      EcmaValue y = endDate.ToNumber();
      if (x > y) {
        throw new EcmaRangeErrorException("Invalid time value");
      }
      if (x < MinTime || x > MaxTime) {
        throw new EcmaRangeErrorException("Invalid time value");
      }
      if (y < MinTime || y > MaxTime) {
        throw new EcmaRangeErrorException("Invalid time value");
      }
      TimeZoneInfo timezone = TZConvert.GetTimeZoneInfo(this.TimeZone);
      DateTime dt1 = unixEpochUtc.AddMilliseconds(x.ToInt64() + timezone.BaseUtcOffset.TotalMilliseconds);
      DateTime dt2 = unixEpochUtc.AddMilliseconds(y.ToInt64() + timezone.BaseUtcOffset.TotalMilliseconds);
      bool dateFieldsPracticallyEqual = true;
      bool patternContainsLargerDateField = false;
      FormattedString pattern = null;
      foreach (FormattedPartType part in new[] { FormattedPartType.Era, FormattedPartType.Year, FormattedPartType.Month, FormattedPartType.Day, FormattedPartType.DayPeriod, FormattedPartType.Hour, FormattedPartType.Minute, FormattedPartType.Second }) {
        int v1 = GetPartValue(part, dt1);
        int v2 = GetPartValue(part, dt2);
        if (v1 != v2) {
          dateFieldsPracticallyEqual = false;
        }
        FormattedString rp = format.GetRangePattern(part);
        if (pattern != null && rp == null) {
          patternContainsLargerDateField = true;
        }
        pattern = rp ?? pattern;
        if (!dateFieldsPracticallyEqual || patternContainsLargerDateField) {
          break;
        }
      }
      if (dateFieldsPracticallyEqual) {
        FormattedString str = FormatDateTime(dt1, format.Pattern);
        annotations = Enumerable.Range(0, str.PartCount).Select(v => "shared").ToArray();
        return str;
      }
      if (pattern == null) {
        pattern = format.GetDefaultRangePattern();
      }
      List<FormattedPart> parts = new List<FormattedPart>(pattern);
      List<string> sources = new List<string>();
      for (int i = parts.Count - 1; i >= 0; i--) {
        FormattedString placeable = null;
        string source = null;
        switch (parts[i].Type) {
          case FormattedPartType.SharedPlaceholder:
            placeable = FormatDateTime(dt1, parts[i].Value);
            source = "shared";
            break;
          case FormattedPartType.StartRangePlaceholder:
            placeable = FormatDateTime(dt1, parts[i].Value);
            source = "startRange";
            break;
          case FormattedPartType.EndRangePlaceholder:
            placeable = FormatDateTime(dt2, parts[i].Value);
            source = "endRange";
            break;
          case FormattedPartType.Placeholder:
            bool isStartRange = parts[i].Value == "{0}";
            placeable = FormatDateTime(isStartRange ? dt1 : dt2, format.Pattern);
            source = isStartRange ? "startRange" : "endRange";
            break;
          default:
            sources.Insert(0, "shared");
            break;
        }
        if (placeable != null) {
          parts.RemoveAt(i);
          parts.InsertRange(i, placeable);
          sources.InsertRange(0, Enumerable.Range(0, placeable.PartCount).Select(_ => source));
        }
      }
      annotations = sources.ToArray();
      return new FormattedString(parts);
    }

    [IntrinsicMember]
    private EcmaValue FormatInternal(EcmaValue value) {
      return Format(value).ToString();
    }

    private FormattedString FormatDateTime(DateTime dt, string patternStr) {
      DateTimePartStyles styles = this.DateTimePartStyles;
      FormattedString pattern = parsedPatterns.GetOrAdd(patternStr, FormattedString.Parse);
      List<FormattedPart> parts = new List<FormattedPart>(pattern);
      if (monthNames == null) {
        monthNames = format.MonthNames;
        dayNames = format.WeekdayNames;
        dayPeriodNames = format.DayPeriodNames;
        eraNames = format.EraNames;
      }
      for (int i = parts.Count - 1; i >= 0; i--) {
        if (parts[i].Type == FormattedPartType.Placeholder) {
          FormattedPart[] placeable = null;
          switch (parts[i].Value) {
            case "{weekday}":
              placeable = new[] { new FormattedPart(FormattedPartType.Weekday, dayNames[GetPartValue(FormattedPartType.Weekday, dt)]) };
              break;
            case "{era}":
              placeable = new[] { new FormattedPart(FormattedPartType.Era, eraNames[GetPartValue(FormattedPartType.Era, dt)]) };
              break;
            case "{year}":
              int year = GetPartValue(FormattedPartType.Year, dt);
              placeable = FormatNumber(FormattedPartType.Year, styles.Year == NumericDateTimePartStyle.TwoDigit ? year % 100 : year, this.NumberingSystem, styles.Year == NumericDateTimePartStyle.TwoDigit ? "00" : "0");
              break;
            case "{month}":
              int month = GetPartValue(FormattedPartType.Month, dt);
              placeable = styles.Month == MonthStyle.Numeric || styles.Month == MonthStyle.TwoDigit
                ? FormatNumber(FormattedPartType.Month, month, this.NumberingSystem, styles.Month == MonthStyle.TwoDigit ? "00" : "0")
                : new[] { new FormattedPart(FormattedPartType.Month, monthNames[month]) };
              break;
            case "{day}":
              placeable = FormatNumber(FormattedPartType.Day, GetPartValue(FormattedPartType.Day, dt), this.NumberingSystem, styles.Day == NumericDateTimePartStyle.TwoDigit ? "00" : "0");
              break;
            case "{hour}":
              placeable = FormatNumber(FormattedPartType.Hour, GetPartValue(FormattedPartType.Hour, dt), this.NumberingSystem, styles.Hour == NumericDateTimePartStyle.TwoDigit ? "00" : "0");
              break;
            case "{minute}":
              placeable = FormatNumber(FormattedPartType.Minute, GetPartValue(FormattedPartType.Minute, dt), this.NumberingSystem, styles.Minute == NumericDateTimePartStyle.TwoDigit ? "00" : "0");
              break;
            case "{second}":
              placeable = FormatNumber(FormattedPartType.Second, GetPartValue(FormattedPartType.Second, dt), this.NumberingSystem, styles.Second == NumericDateTimePartStyle.TwoDigit ? "00" : "0");
              break;
            case "{ampm}":
              placeable = new[] { new FormattedPart(FormattedPartType.DayPeriod, dayPeriodNames[GetPartValue(FormattedPartType.DayPeriod, dt)]) };
              break;
            case "{timeZoneName}":
              placeable = new[] { new FormattedPart(FormattedPartType.TimeZoneName, CldrTimeZoneNames.Resolve(this.Locale, this.TimeZone, dt, styles.TimeZoneName)) };
              break;
          }
          if (placeable != null) {
            parts.RemoveAt(i);
            parts.InsertRange(i, placeable);
          }
        }
      }
      return new FormattedString(parts);
    }

    private CldrDateTimeFormat GetBestFormat(DateTimePartStyles options) {
      string locale = IntlUtility.RemoveUnicodeExtensions(this.Locale);
      ReadOnlyCollection<CldrDateTimeFormat> formats;
      if (options.IsDateOnly) {
        formats = CldrCalendarInfo.Resolve(locale, this.Calendar).GetAvailableDateFormats();
      } else if (options.IsTimeOnly) {
        formats = CldrCalendarInfo.Resolve(locale, "generic").GetAvailableTimeFormats();
      } else {
        formats = CldrCalendarInfo.Resolve(locale, this.Calendar).GetAvailableDateTimeFormats();
      }
      bool isDateOnly = options.IsDateOnly;
      bool isHour12 = this.Hour12;
      int bestScore = Int32.MinValue;
      CldrDateTimeFormat bestFormat = null;
      foreach (CldrDateTimeFormat format in formats) {
        if (isDateOnly || isHour12 == format.Styles.IsHour12) {
          int score = format.Styles.Match(options);
          if (score > bestScore) {
            bestScore = score;
            bestFormat = format;
          }
        }
      }
      return bestFormat;
    }

    private int GetPartValue(FormattedPartType type, DateTime dt) {
      switch (type) {
        case FormattedPartType.Weekday:
          return (int)calendar.GetDayOfWeek(dt);
        case FormattedPartType.Era:
          return calendar.GetEra(dt);
        case FormattedPartType.Year:
          return calendar.GetYear(dt);
        case FormattedPartType.Month:
          return calendar.GetMonth(dt);
        case FormattedPartType.Day:
          return calendar.GetDayOfMonth(dt);
        case FormattedPartType.Hour:
          return GetHourInHourCycle(dt.Hour, this.HourCycle);
        case FormattedPartType.Minute:
          return dt.Minute;
        case FormattedPartType.Second:
          return dt.Second;
        case FormattedPartType.DayPeriod:
          return dt.Hour / 12;
      }
      throw new ArgumentOutOfRangeException("type");
    }

    private static FormattedPart[] FormatNumber(FormattedPartType type, int value, string nu, string format = null) {
      return new[] { new FormattedPart(type, value.ToString(format, CultureInfo.InvariantCulture)) };
    }

    private static int GetHourInHourCycle(int value, HourCycle hc) {
      if (hc == HourCycle.Hour11 || hc == HourCycle.Hour12) {
        value = value % 12;
      }
      if (value == 0) {
        if (hc == HourCycle.Hour12) {
          value = 12;
        } else if (hc == HourCycle.Hour24) {
          value = 24;
        }
      }
      return value;
    }

    private static HourCycle NormalizeHourCycle(HourCycle hc, bool hour12) {
      return hour12
         ? hc == HourCycle.Hour11 || hc == HourCycle.Hour23 ? HourCycle.Hour11 : HourCycle.Hour12
         : hc == HourCycle.Hour11 || hc == HourCycle.Hour23 ? HourCycle.Hour23 : HourCycle.Hour24;
    }
  }
}
