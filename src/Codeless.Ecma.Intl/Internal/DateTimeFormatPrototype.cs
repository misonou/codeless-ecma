using Codeless.Ecma.Runtime;
using System;
using System.Linq;

namespace Codeless.Ecma.Intl.Internal {
  [IntrinsicObject(IntlObjectKind.DateTimeFormatPrototype)]
  internal static class DateTimeFormatPrototype {
    [IntrinsicMember(WellKnownSymbol.ToStringTag, EcmaPropertyAttributes.Configurable)]
    public const string ToStringTag = "Object";

    [IntrinsicMember(Getter = true)]
    public static EcmaValue Format([This] EcmaValue thisValue) {
      DateTimeFormat formatter = thisValue.GetUnderlyingObject<DateTimeFormat>();
      return formatter.BoundFormat;
    }

    [IntrinsicMember]
    public static EcmaValue FormatRange([This] EcmaValue thisValue, EcmaValue startDate, EcmaValue endDate) {
      DateTimeFormat formatter = thisValue.GetUnderlyingObject<DateTimeFormat>();
      return formatter.FormatRange(startDate, endDate, out _).ToString();
    }

    [IntrinsicMember]
    public static EcmaValue FormatToParts([This] EcmaValue thisValue, EcmaValue date) {
      DateTimeFormat formatter = thisValue.GetUnderlyingObject<DateTimeFormat>();
      return formatter.Format(date).ToPartArray();
    }

    [IntrinsicMember]
    public static EcmaValue FormatRangeToParts([This] EcmaValue thisValue, EcmaValue startDate, EcmaValue endDate) {
      DateTimeFormat formatter = thisValue.GetUnderlyingObject<DateTimeFormat>();
      return formatter.FormatRange(startDate, endDate, out string[] annotations).ToPartArray(PropertyKey.Source, annotations);
    }

    [IntrinsicMember]
    public static EcmaValue ResolvedOptions([This] EcmaValue thisValue) {
      DateTimeFormat formatter = thisValue.GetUnderlyingObject<DateTimeFormat>();
      DateTimePartStyles dateTimePartStyles = formatter.DateTimePartStyles;
      EcmaObject obj = new EcmaObject();
      obj.CreateDataPropertyOrThrow(PropertyKey.Locale, formatter.Locale);
      obj.CreateDataPropertyOrThrow(PropertyKey.Calendar, formatter.Calendar);
      obj.CreateDataPropertyOrThrow(PropertyKey.TimeZone, formatter.TimeZone);
      obj.CreateDataPropertyOrThrow(PropertyKey.Hour12, formatter.Hour12);
      obj.CreateDataPropertyOrThrow(PropertyKey.HourCycle, IntlProviderOptions.ToStringValue(formatter.HourCycle));
      obj.CreateDataPropertyOrThrow(PropertyKey.FormatMatcher, IntlProviderOptions.ToStringValue(formatter.FormatMatcher));
      if (dateTimePartStyles.Weekday != default) {
        obj.CreateDataPropertyOrThrow(PropertyKey.Weekday, IntlProviderOptions.ToStringValue(dateTimePartStyles.Weekday));
      }
      if (dateTimePartStyles.Era != default) {
        obj.CreateDataPropertyOrThrow(PropertyKey.Era, IntlProviderOptions.ToStringValue(dateTimePartStyles.Era));
      }
      if (dateTimePartStyles.Year != default) {
        obj.CreateDataPropertyOrThrow(PropertyKey.Year, IntlProviderOptions.ToStringValue(dateTimePartStyles.Year));
      }
      if (dateTimePartStyles.Month != default) {
        obj.CreateDataPropertyOrThrow(PropertyKey.Month, IntlProviderOptions.ToStringValue(dateTimePartStyles.Month));
      }
      if (dateTimePartStyles.Day != default) {
        obj.CreateDataPropertyOrThrow(PropertyKey.Day, IntlProviderOptions.ToStringValue(dateTimePartStyles.Day));
      }
      if (dateTimePartStyles.Hour != default) {
        obj.CreateDataPropertyOrThrow(PropertyKey.Hour, IntlProviderOptions.ToStringValue(dateTimePartStyles.Hour));
      }
      if (dateTimePartStyles.Minute != default) {
        obj.CreateDataPropertyOrThrow(PropertyKey.Minute, IntlProviderOptions.ToStringValue(dateTimePartStyles.Minute));
      }
      if (dateTimePartStyles.Second != default) {
        obj.CreateDataPropertyOrThrow(PropertyKey.Second, IntlProviderOptions.ToStringValue(dateTimePartStyles.Second));
      }
      if (dateTimePartStyles.TimeZoneName != default) {
        obj.CreateDataPropertyOrThrow(PropertyKey.TimeZoneName, IntlProviderOptions.ToStringValue(dateTimePartStyles.TimeZoneName));
      }
      return obj;
    }
  }
}
