using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Codeless.Ecma {
  internal enum EcmaDateComponent {
    Year,
    Month,
    Date,
    Hours,
    Minutes,
    Seconds,
    Milliseconds,
    WeekDay
  }

  internal struct EcmaTimestamp {
    private static readonly DateTime UnixEpochUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly int[] DaysToMonth365 = new int[] { 0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334, 365 };
    private static readonly int[] DaysToMonth366 = new int[] { 0, 31, 60, 91, 121, 152, 182, 213, 244, 274, 305, 335, 366 };
    private static readonly int tz = (int)TimeZoneInfo.Local.BaseUtcOffset.TotalMilliseconds;

    private const string DefaultFormat = "{0} {1} {2:D2} {3:D4} {4:D2}:{5:D2}:{6:D2} GMT{7:+00;-00}{8:D2} ({9})";
    private const string DateFormat = "{0} {1} {2:D2} {3:D4}";
    private const string UTCFormat = "{0}, {2:D2} {1} {3:D4} {4:D2}:{5:D2}:{6:D2} GMT";
    private const string ISOFormat = "{0:D4}-{1:D2}-{3:D2}T{4:D2}:{5:D2}:{6:D2}Z";
    private const string ISOExtendFormat = "{0:+000000;-000000}-{1:D2}-{3:D2}T{4:D2}:{5:D2}:{6:D2}.{7:D3}Z";
    private const string TimeFormat = "{0:D2}:{1:D2}:{2:D2} GMT{3:+00;-00}{4:D2} ({5})";

    private const long MinTime = -8640000000000000;
    private const long MaxTime = 8640000000000000;
    private const int HoursPerDay = 24;
    private const int MinutesPerHour = 60;
    private const int SecondsPerMinute = 60;
    private const int msPerDay = 86400000;
    private const int msPerSecond = 1000;
    private const int msPerMinute = 60000;
    private const int msPerHour = 3600000;
    private const int DaysPerYear = 365;
    private const int DaysPer4Years = 1461;
    private const int DaysPer100Years = 36524;
    private const int DaysPer400Years = 146097;
    private const int DaysTo1970 = 719162;

    public static readonly EcmaTimestamp Invalid = new EcmaTimestamp(Int64.MinValue);
    public static readonly EcmaTimestamp LocalEpoch = new EcmaTimestamp(tz);

    private readonly long timestamp;

    public EcmaTimestamp(long timestamp) {
      this.timestamp = (timestamp < MinTime || timestamp > MaxTime) ? Int64.MinValue : timestamp;
    }

    public EcmaTimestamp(DateTime dt) {
      this.timestamp = FromNativeDateTime(dt).timestamp;
    }

    public static int TimezoneOffset {
      get { return tz; }
    }

    public bool IsValid {
      get { return timestamp != Int64.MinValue; }
    }

    public long Value {
      get { return timestamp; }
    }

    public int GetComponent(EcmaDateComponent part) {
      return GetComponent(ToLocal(timestamp), part);
    }

    public int GetComponentUtc(EcmaDateComponent part) {
      return GetComponent(timestamp, part);
    }

    public override string ToString() {
      return ToString(DateTimeFormatInfo.InvariantInfo);
    }

    public string ToString(DateTimeFormatInfo format) {
      if (!IsValid) {
        return "Invalid Date";
      }
      long t = ToLocal(this.timestamp);
      int y = 0, m = 0, d = 0, h = 0, n = 0, s = 0, ms = 0;
      GetDateComponents(t, 3, ref y, ref m, ref d);
      GetTimeComponents(t, 4, ref h, ref n, ref s, ref ms);
      return String.Format(DefaultFormat, format.AbbreviatedDayNames[GetComponent(t, EcmaDateComponent.WeekDay)], format.AbbreviatedMonthNames[m], d, y, h, n, s, tz / msPerHour, (tz % msPerHour) / msPerMinute, TimeZoneInfo.Local.StandardName);
    }

    public string ToISOString() {
      if (!IsValid) {
        throw new EcmaRangeErrorException("Invalid time value");
      }
      int y = 0, m = 0, d = 0, h = 0, n = 0, s = 0, ms = 0;
      GetDateComponents(timestamp, 3, ref y, ref m, ref d);
      GetTimeComponents(timestamp, 4, ref h, ref n, ref s, ref ms);
      return String.Format(y >= 0 && y < 10000 ? ISOFormat : ISOExtendFormat, y, m + 1, d, h, n, s, ms);
    }

    public string ToUTCString(DateTimeFormatInfo format) {
      if (!IsValid) {
        return "Invalid Date";
      }
      int y = 0, m = 0, d = 0, h = 0, n = 0, s = 0, ms = 0;
      GetDateComponents(timestamp, 3, ref y, ref m, ref d);
      GetTimeComponents(timestamp, 4, ref h, ref n, ref s, ref ms);
      return String.Format(UTCFormat, format.AbbreviatedDayNames[GetComponent(timestamp, EcmaDateComponent.WeekDay)], format.AbbreviatedMonthNames[m], d, y, h, n, s);
    }

    public string ToDateString(DateTimeFormatInfo format) {
      if (!IsValid) {
        return "Invalid Date";
      }
      long t = ToLocal(this.timestamp);
      int y = 0, m = 0, d = 0;
      GetDateComponents(t, 3, ref y, ref m, ref d);
      return String.Format(DateFormat, format.AbbreviatedDayNames[GetComponent(t, EcmaDateComponent.WeekDay)], format.AbbreviatedMonthNames[m], d, y);
    }

    public string ToTimeString(DateTimeFormatInfo format) {
      if (!IsValid) {
        return "Invalid Date";
      }
      int h = 0, n = 0, s = 0, ms = 0;
      GetTimeComponents(ToLocal(timestamp), 4, ref h, ref n, ref s, ref ms);
      return String.Format(TimeFormat, h, n, s, tz / msPerHour, (tz % msPerHour) / msPerMinute, TimeZoneInfo.Local.StandardName);
    }

    public DateTime ToNativeDateTime(DateTimeKind kind = DateTimeKind.Local) {
      DateTime d = UnixEpochUtc.AddMilliseconds(Value);
      if (kind == DateTimeKind.Local) {
        return d.ToLocalTime();
      }
      return d;
    }

    public static EcmaTimestamp FromNativeDateTime(DateTime d) {
      if (d.Kind == DateTimeKind.Utc) {
        return new EcmaTimestamp(Convert.ToInt64((d - UnixEpochUtc).TotalMilliseconds));
      }
      return new EcmaTimestamp(Convert.ToInt64((d.ToUniversalTime() - UnixEpochUtc).TotalMilliseconds));
    }

    public static long GetTimestamp(long timestamp, int start, params long[] args) {
      return ToUtc(GetTimestampUtc(ToLocal(timestamp), start, args));
    }

    public static long GetTimestampUtc(long timestamp, int start, params long[] args) {
      long datePart, timePart;

      long[] p = new long[7];
      Array.Copy(args, 0, p, start, args.Length);

      if (start >= 3) {
        datePart = timestamp / msPerDay * msPerDay;
      } else {
        if (start > 2 || args.Length < 3) {
          int y = 0, m = 0, d = 0;
          int part = start + args.Length - 3;
          GetDateComponents(timestamp, part, ref y, ref m, ref d);
          if (start > 0 || args.Length < 1) {
            p[0] = y;
          }
          if (start > 1 || args.Length < 2) {
            p[1] = m;
          }
          if (start > 2 || args.Length < 3) {
            p[2] = d;
          }
        }
        datePart = GetTimestampForDate(p[0], p[1], p[2]);
      }

      int end = start + args.Length;
      if (end < 4) {
        timePart = timestamp % msPerDay;
      } else {
        if (end < 7 || start > 3) {
          int h = 0, n = 0, s = 0, ms = 0;
          GetTimeComponents(timestamp, 4, ref h, ref n, ref s, ref ms);
          if (end < 4 || start > 3) {
            p[3] = h;
          }
          if (end < 5 || start > 4) {
            p[4] = n;
          }
          if (end < 6 || start > 5) {
            p[5] = s;
          }
          if (end < 7 || start > 6) {
            p[6] = ms;
          }
        }
        timePart = (p[3] * msPerHour) + (p[4] * msPerMinute) + (p[5] * msPerSecond) + p[6];
      }
      return datePart + timePart;
    }

    public static explicit operator long(EcmaTimestamp value) {
      return value.timestamp;
    }

    public static explicit operator EcmaTimestamp(long value) {
      return new EcmaTimestamp(value);
    }

    #region Helper methods
    private static long GetTimestampForDate(long y, long m, long d) {
      y = y + m / 12;
      m = m % 12;

      bool isLeapYear = (y % 4 == 0) && (y % 100 != 0 || y % 400 == 0);
      int[] arr = (isLeapYear ? DaysToMonth366 : DaysToMonth365);
      long day = (365 * (y - 1970) * ((y - 1969) / 4) - ((y - 1901) / 100) + ((y - 1601) / 400)) + (arr[m] + d - 1);
      return day * msPerDay;
    }

    private static int GetDateComponents(long t, int part, ref int y, ref int m, ref int d) {
      int numOf400y, numOf100y, numOf4y, numOf1y;

      d = (int)(t / msPerDay) + DaysTo1970;
      numOf400y = d / DaysPer400Years;
      d = d - numOf400y * DaysPer400Years;
      numOf100y = d / DaysPer100Years;
      if (numOf100y == 4) {
        numOf100y = 3;
      }
      d = d - numOf100y * DaysPer100Years;
      numOf4y = d / DaysPer4Years;
      d = d - numOf4y * DaysPer4Years;
      numOf1y = d / DaysPerYear;
      if (numOf1y == 4) {
        numOf1y = 3;
      }
      if (part == 0) {
        y = numOf400y * 400 + numOf100y * 100 + numOf4y * 4 + numOf1y + 1;
        return y;
      }
      if (part == 3) {
        y = numOf400y * 400 + numOf100y * 100 + numOf4y * 4 + numOf1y + 1;
      }
      d = d - numOf1y * DaysPerYear;
      bool isLeapYear = numOf1y == 3 && (numOf4y != 24 && numOf100y == 3);
      int[] arr = (isLeapYear ? DaysToMonth366 : DaysToMonth365);
      m = d >> 6;
      while (d >= arr[m]) {
        m++;
      }
      m--;
      if (part == 1) {
        return m;
      }
      d = d - arr[m] + 1;
      return d;
    }

    private static int GetTimeComponents(long t, int part, ref int h, ref int n, ref int s, ref int ms) {
      t = t % msPerDay;
      h = (int)(t / msPerHour);
      t -= h * msPerHour;
      n = (int)(t / msPerMinute);
      t -= n * msPerMinute;
      s = (int)(t / msPerSecond);
      ms = (int)(t - s * msPerSecond);
      return ms;
    }

    private static int GetComponent(long t, EcmaDateComponent part) {
      int y = 0, m = 0, d = 0;
      switch (part) {
        case EcmaDateComponent.Year:
          return GetDateComponents(t, 0, ref y, ref m, ref d);
        case EcmaDateComponent.Month:
          return GetDateComponents(t, 1, ref y, ref m, ref d);
        case EcmaDateComponent.Date:
          return GetDateComponents(t, 2, ref y, ref m, ref d);
        case EcmaDateComponent.Hours:
          return (int)(t / msPerHour) % HoursPerDay;
        case EcmaDateComponent.Minutes:
          return (int)((t / msPerMinute) % MinutesPerHour);
        case EcmaDateComponent.Seconds:
          return (int)((t / msPerSecond) % SecondsPerMinute);
        case EcmaDateComponent.Milliseconds:
          return (int)(t % msPerSecond);
        case EcmaDateComponent.WeekDay:
          return ((int)(t / msPerDay) + 4) % 7;
      }
      return 0;
    }

    private static long ToUtc(long t) {
      return t - tz - GetDaylightSavingAdjustment(t - tz);
    }

    private static long ToLocal(long t) {
      return t + tz + GetDaylightSavingAdjustment(t);
    }

    private static int GetDaylightSavingAdjustment(long t) {
      try {
        DateTime d = UnixEpochUtc.AddMilliseconds(t);
        if (TimeZoneInfo.Local.IsAmbiguousTime(d)) {
          return 0;
        }
        if (TimeZoneInfo.Local.IsDaylightSavingTime(d)) {
          return (int)TimeZoneInfo.Local.GetUtcOffset(d).TotalMilliseconds - tz;
        }
      } catch (ArgumentOutOfRangeException) { }
      return 0;
    }
    #endregion
  }
}
