using Codeless.Ecma.Intl.Utilities;
using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Intl.Internal {
  internal static class BuiltInFunctionReplacement {
    [IntrinsicMember("toLocaleString")]
    public static EcmaValue NumberPrototypeToLocaleString([This] EcmaValue thisValue, EcmaValue locales, EcmaValue options) {
      EcmaValue number = thisValue.GetIntrinsicPrimitiveValue(EcmaValueType.Number);
      NumberFormat formatter = new NumberFormat();
      NumberFormatConstructor.NumberFormat(formatter, locales, options);
      return formatter.Format(number).ToString();
    }

    [IntrinsicMember("toLocaleString")]
    public static EcmaValue ArrayPrototypeToLocaleString([This] EcmaValue thisValue, EcmaValue locales, EcmaValue options) {
      RuntimeObject obj = thisValue.ToObject();
      long length = obj.Get(WellKnownProperty.Length).ToLength();
      if (length == 0) {
        return String.Empty;
      }
      StringBuilder sb = new StringBuilder();
      for (long i = 0; i < length; i++) {
        if (i > 0) {
          sb.Append(",");
        }
        EcmaValue item = obj.Get(i);
        if (!item.IsNullOrUndefined) {
          sb.Append(item.Invoke("toLocaleString", locales, options).ToStringOrThrow());
        }
      }
      return sb.ToString();
    }

    [IntrinsicMember("toLocaleString")]
    public static EcmaValue DatePrototypeToLocaleString([This] EcmaValue thisValue, EcmaValue locales, EcmaValue options) {
      EcmaTimestamp dt = thisValue.GetUnderlyingObject<EcmaDate>().Timestamp;
      if (!dt.IsValid) {
        return "Invalid Date";
      }
      DateTimeFormat formatter = new DateTimeFormat();
      ICollection<string> requestedLocales = IntlUtility.CanonicalizeLocaleList(locales);
      formatter.Init(requestedLocales, DateTimeFormatConstructor.CreateOptions(options, true, true, true, true));
      return formatter.Format(thisValue).ToString();
    }

    [IntrinsicMember("toLocaleDateString")]
    public static EcmaValue DatePrototypeToLocaleDateString([This] EcmaValue thisValue, EcmaValue locales, EcmaValue options) {
      EcmaTimestamp dt = thisValue.GetUnderlyingObject<EcmaDate>().Timestamp;
      if (!dt.IsValid) {
        return "Invalid Date";
      }
      DateTimeFormat formatter = new DateTimeFormat();
      ICollection<string> requestedLocales = IntlUtility.CanonicalizeLocaleList(locales);
      formatter.Init(requestedLocales, DateTimeFormatConstructor.CreateOptions(options, true, false, true, false));
      return formatter.Format(thisValue).ToString();
    }

    [IntrinsicMember("toLocaleTimeString")]
    public static EcmaValue DatePrototypeToLocaleTimeString([This] EcmaValue thisValue, EcmaValue locales, EcmaValue options) {
      EcmaTimestamp dt = thisValue.GetUnderlyingObject<EcmaDate>().Timestamp;
      if (!dt.IsValid) {
        return "Invalid Date";
      }
      DateTimeFormat formatter = new DateTimeFormat();
      ICollection<string> requestedLocales = IntlUtility.CanonicalizeLocaleList(locales);
      formatter.Init(requestedLocales, DateTimeFormatConstructor.CreateOptions(options, false, true, false, false));
      return formatter.Format(thisValue).ToString();
    }

    [IntrinsicMember("toLocaleUpperCase")]
    public static EcmaValue StringPrototypeToLocaleUpperCase([This] EcmaValue thisValue, EcmaValue locales) {
      Guard.RequireObjectCoercible(thisValue);
      string str = thisValue.ToStringOrThrow();
      string locale = IntlUtility.GetBestAvailableLocale(IntlUtility.SystemLocales, IntlUtility.CanonicalizeLocaleList(locales), LocaleMatcher.BestFit, out _);
      return CultureInfo.GetCultureInfo(locale).TextInfo.ToUpper(str);
    }

    [IntrinsicMember("toLocaleLowerCase")]
    public static EcmaValue StringPrototypeToLocaleLowerCase([This] EcmaValue thisValue, EcmaValue locales) {
      Guard.RequireObjectCoercible(thisValue);
      string str = thisValue.ToStringOrThrow();
      string locale = IntlUtility.GetBestAvailableLocale(IntlUtility.SystemLocales, IntlUtility.CanonicalizeLocaleList(locales), LocaleMatcher.BestFit, out _);
      return CultureInfo.GetCultureInfo(locale).TextInfo.ToLower(str);
    }

    [IntrinsicMember("localeCompare")]
    public static EcmaValue StringPrototypeLocaleCompare([This] EcmaValue thisValue, EcmaValue thatValue, EcmaValue locales, EcmaValue options) {
      Guard.RequireObjectCoercible(thisValue);
      string thisString = thisValue.ToStringOrThrow();
      string thatString = thatValue.ToStringOrThrow();
      Collator collator = new Collator();
      CollatorConstructor.Collator(collator, locales, options);
      return collator.Compare(thisString, thatString);
    }
  }
}
