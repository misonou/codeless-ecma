using System;

namespace Codeless.Ecma.Intl {
  public struct DateTimePartStyles : IEquatable<DateTimePartStyles> {
    private const int TimePartMask = 268402688;

    private readonly int value;

    public DateTimePartStyles(WeekdayStyle weekday, EraStyle era, NumericDateTimePartStyle year, MonthStyle month, NumericDateTimePartStyle day, NumericDateTimePartStyle hour, NumericDateTimePartStyle minute, NumericDateTimePartStyle second, TimeZoneNameStyle timeZoneName, bool isHour12) {
      int value = 0;
      value |= (int)weekday << 0;
      value |= (int)era << 3;
      value |= (int)year << 6;
      value |= (int)month << 9;
      value |= (int)day << 12;
      value |= (int)hour << 15;
      value |= (int)minute << 18;
      value |= (int)second << 21;
      value |= (int)timeZoneName << 24;
      value |= isHour12 ? 1 << 27 : 0;
      this.value = value;
    }

    public DateTimePartStyles(DateTimePartStyles dateStyles, DateTimePartStyles timeStyles) {
      this.value = (dateStyles.value & ~TimePartMask) | (timeStyles.value & TimePartMask);
    }

    public DateTimePartStyles(DateTimeFormatOptions options) {
      Guard.ArgumentNotNull(options, "options");
      int value = 0;
      value |= (int)options.Weekday << 0;
      value |= (int)options.Era << 3;
      value |= (int)options.Year << 6;
      value |= (int)options.Month << 9;
      value |= (int)options.Day << 12;
      value |= (int)options.Hour << 15;
      value |= (int)options.Minute << 18;
      value |= (int)options.Second << 21;
      value |= (int)options.TimeZoneName << 24;
      this.value = value;
    }

    public WeekdayStyle Weekday => (WeekdayStyle)((value >> 0) & 0b111);

    public EraStyle Era => (EraStyle)((value >> 3) & 0b111);

    public NumericDateTimePartStyle Year => (NumericDateTimePartStyle)((value >> 6) & 0b111);

    public MonthStyle Month => (MonthStyle)((value >> 9) & 0b111);

    public NumericDateTimePartStyle Day => (NumericDateTimePartStyle)((value >> 12) & 0b111);

    public NumericDateTimePartStyle Hour => (NumericDateTimePartStyle)((value >> 15) & 0b111);

    public NumericDateTimePartStyle Minute => (NumericDateTimePartStyle)((value >> 18) & 0b111);

    public NumericDateTimePartStyle Second => (NumericDateTimePartStyle)((value >> 21) & 0b111);

    public TimeZoneNameStyle TimeZoneName => (TimeZoneNameStyle)((value >> 24) & 0b111);

    public bool IsHour12 => (value & (1 << 27)) != 0;

    public bool IsDateOnly {
      get { return (value & TimePartMask) == 0; }
    }

    public bool IsTimeOnly {
      get { return (value & ~TimePartMask) == 0; }
    }

    public int Match(DateTimePartStyles options) {
      const int removalPenalty = 120;
      const int additionPenalty = 20;
      const int longLessPenalty = 8;
      const int longMorePenalty = 6;
      const int shortLessPenalty = 6;
      const int shortMorePenalty = 3;
      int score = 0;
      for (int i = 0; i <= 24; i += 3) {
        int thisValue = (value >> i) & 0b111;
        int thatValue = (options.value >> i) & 0b111;
        if (thatValue == 0 && thisValue != 0) {
          score -= additionPenalty;
        } else if (thatValue != 0 && thisValue == 0) {
          score -= removalPenalty;
        } else if (thatValue != thisValue) {
          switch (Math.Max(Math.Min(thisValue - thatValue, 2), -2)) {
            case 2:
              score -= longMorePenalty;
              break;
            case 1:
              score -= shortMorePenalty;
              break;
            case -1:
              score -= shortLessPenalty;
              break;
            case -2:
              score -= longLessPenalty;
              break;
          }
        }
      }
      return score + (removalPenalty * 9);
    }

    public bool Equals(DateTimePartStyles other) {
      return value == other.value;
    }

    public override int GetHashCode() {
      return value;
    }
  }
}
