using Codeless.Ecma.Intl.Internal;
using Codeless.Ecma.Intl.Utilities;

namespace Codeless.Ecma.Intl {
  public enum DateTimeFormatMatcher {
    [StringValue("basic")]
    Basic,
    [StringValue("best fit")]
    BestFit
  }

  public enum HourCycle {
    Unspecified,
    [StringValue("h11")]
    Hour11,
    [StringValue("h12")]
    Hour12,
    [StringValue("h23")]
    Hour23,
    [StringValue("h24")]
    Hour24
  }

  public enum NumericDateTimePartStyle {
    Unspecified,
    [StringValue("2-digit")]
    TwoDigit,
    [StringValue("numeric")]
    Numeric
  }

  public enum WeekdayStyle {
    Unspecified,
    [StringValue("narrow")]
    Narrow = 3,
    [StringValue("short")]
    Short,
    [StringValue("long")]
    Long
  }

  public enum EraStyle {
    Unspecified,
    [StringValue("narrow")]
    Narrow = 3,
    [StringValue("short")]
    Short,
    [StringValue("long")]
    Long
  }

  public enum TimeZoneNameStyle {
    Unspecified,
    [StringValue("short")]
    Short = 4,
    [StringValue("long")]
    Long
  }

  public enum MonthStyle {
    Unspecified,
    [StringValue("2-digit")]
    TwoDigit,
    [StringValue("numeric")]
    Numeric,
    [StringValue("narrow")]
    Narrow,
    [StringValue("short")]
    Short,
    [StringValue("long")]
    Long
  }

  public class DateTimeFormatOptions : IntlProviderOptions {
    public DateTimeFormatOptions() { }

    public DateTimeFormatOptions(EcmaValue value)
      : base(value) { }

    public HourCycle HourCycle {
      get => GetOption(PropertyKey.HourCycle, HourCycle.Unspecified);
      set => SetOption(PropertyKey.HourCycle, value);
    }

    public DateTimeFormatMatcher FormatMatcher {
      get => GetOption(PropertyKey.FormatMatcher, DateTimeFormatMatcher.BestFit);
      set => SetOption(PropertyKey.FormatMatcher, value);
    }

    public WeekdayStyle Weekday {
      get => GetOption(PropertyKey.Weekday, WeekdayStyle.Unspecified);
      set => SetOption(PropertyKey.Weekday, value);
    }

    public EraStyle Era {
      get => GetOption(PropertyKey.Era, EraStyle.Unspecified);
      set => SetOption(PropertyKey.Era, value);
    }

    public NumericDateTimePartStyle Year {
      get => GetOption(PropertyKey.Year, NumericDateTimePartStyle.Unspecified);
      set => SetOption(PropertyKey.Year, value);
    }

    public MonthStyle Month {
      get => GetOption(PropertyKey.Month, MonthStyle.Unspecified);
      set => SetOption(PropertyKey.Month, value);
    }

    public NumericDateTimePartStyle Day {
      get => GetOption(PropertyKey.Day, NumericDateTimePartStyle.Unspecified);
      set => SetOption(PropertyKey.Day, value);
    }

    public NumericDateTimePartStyle Hour {
      get => GetOption(PropertyKey.Hour, NumericDateTimePartStyle.Unspecified);
      set => SetOption(PropertyKey.Hour, value);
    }

    public NumericDateTimePartStyle Minute {
      get => GetOption(PropertyKey.Minute, NumericDateTimePartStyle.Unspecified);
      set => SetOption(PropertyKey.Minute, value);
    }

    public NumericDateTimePartStyle Second {
      get => GetOption(PropertyKey.Second, NumericDateTimePartStyle.Unspecified);
      set => SetOption(PropertyKey.Second, value);
    }

    public TimeZoneNameStyle TimeZoneName {
      get => GetOption(PropertyKey.TimeZoneName, TimeZoneNameStyle.Unspecified);
      set => SetOption(PropertyKey.TimeZoneName, value);
    }

    public bool? Hour12 {
      get => GetOption(PropertyKey.Hour12, BooleanNull);
      set => SetOption(PropertyKey.Hour12, value);
    }

    public string TimeZone {
      get => GetOption(PropertyKey.TimeZone, (string)null);
      set => SetOption(PropertyKey.TimeZone, value);
    }

    public string NumberingSystem {
      get => GetOption(PropertyKey.NumberingSystem, (string)null);
      set => SetOption(PropertyKey.NumberingSystem, value);
    }

    public string Calendar {
      get => GetOption(PropertyKey.Calendar, (string)null);
      set => SetOption(PropertyKey.Calendar, value);
    }
  }
}
