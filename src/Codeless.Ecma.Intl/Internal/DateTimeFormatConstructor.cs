using Codeless.Ecma.Intl.Utilities;
using Codeless.Ecma.Runtime;
using System.Collections.Generic;
using System.Linq;

namespace Codeless.Ecma.Intl.Internal {
  [IntrinsicObject(IntlObjectKind.DateTimeFormat)]
  internal static class DateTimeFormatConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.AlwaysConstruct, Global = false, ObjectType = typeof(DateTimeFormat), Prototype = IntlObjectKind.DateTimeFormatPrototype)]
    public static EcmaValue DateTimeFormat([This] EcmaValue thisValue, EcmaValue locales, EcmaValue options) {
      DateTimeFormat formatter = thisValue.GetUnderlyingObject<DateTimeFormat>();
      ICollection<string> requestedLocales = IntlUtility.CanonicalizeLocaleList(locales);
      formatter.Init(requestedLocales, CreateOptions(options, true, true, true, false));
      return thisValue;
    }

    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue SupportedLocalesOf(EcmaValue locales, EcmaValue options) {
      ICollection<string> requestedLocales = IntlUtility.CanonicalizeLocaleList(locales);
      List<string> result = IntlUtility.GetSupportedLocales(CldrCalendarInfo.AvailableLocales, requestedLocales, new DateTimeFormatOptions(options));
      return new EcmaArray(result.Select(v => (EcmaValue)v).ToArray());
    }

    public static DateTimeFormatOptions CreateOptions(EcmaValue options, bool requiredDate, bool requiredTime, bool defaultsDate, bool defaultsTime) {
      RuntimeObject obj = RuntimeObject.Create(options == default ? null : options.ToObject());
      bool needDefaults = true;
      if (requiredDate) {
        needDefaults &= obj["weekday"] == default;
        needDefaults &= obj["year"] == default;
        needDefaults &= obj["month"] == default;
        needDefaults &= obj["day"] == default;
      }
      if (requiredTime) {
        needDefaults &= obj["hour"] == default;
        needDefaults &= obj["minute"] == default;
        needDefaults &= obj["second"] == default;
      }
      if (needDefaults) {
        if (defaultsDate) {
          obj.CreateDataPropertyOrThrow("year", "numeric");
          obj.CreateDataPropertyOrThrow("month", "numeric");
          obj.CreateDataPropertyOrThrow("day", "numeric");
        }
        if (defaultsTime) {
          obj.CreateDataPropertyOrThrow("hour", "numeric");
          obj.CreateDataPropertyOrThrow("minute", "numeric");
          obj.CreateDataPropertyOrThrow("second", "numeric");
        }
      }
      return new DateTimeFormatOptions(obj);
    }
  }
}
