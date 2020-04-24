using Codeless.Ecma.Runtime;
using System;
using System.Linq;

namespace Codeless.Ecma.Intl.Internal {
  [IntrinsicObject(IntlObjectKind.NumberFormatPrototype)]
  internal static class NumberFormatPrototype {
    [IntrinsicMember(WellKnownSymbol.ToStringTag, EcmaPropertyAttributes.Configurable)]
    public const string ToStringTag = "Object";

    [IntrinsicMember(Getter = true)]
    public static EcmaValue Format([This] EcmaValue thisValue) {
      NumberFormat formatter = thisValue.GetUnderlyingObject<NumberFormat>();
      return formatter.BoundFormat;
    }

    [IntrinsicMember]
    public static EcmaValue FormatToParts([This] EcmaValue thisValue, EcmaValue value) {
      NumberFormat formatter = thisValue.GetUnderlyingObject<NumberFormat>();
      return formatter.Format(value).ToPartArray();
    }

    [IntrinsicMember]
    public static EcmaValue ResolvedOptions([This] EcmaValue thisValue) {
      NumberFormat formatter = thisValue.GetUnderlyingObject<NumberFormat>();
      EcmaObject obj = new EcmaObject();
      obj.CreateDataPropertyOrThrow(PropertyKey.Locale, formatter.Locale);
      obj.CreateDataPropertyOrThrow(PropertyKey.NumberingSystem, formatter.NumberingSystem);
      obj.CreateDataPropertyOrThrow(PropertyKey.Style, IntlProviderOptions.ToStringValue(formatter.Style));
      obj.CreateDataPropertyOrThrow(PropertyKey.Notation, IntlProviderOptions.ToStringValue(formatter.Notation));
      obj.CreateDataPropertyOrThrow(PropertyKey.CompactDisplay, IntlProviderOptions.ToStringValue(formatter.CompactDisplay));
      obj.CreateDataPropertyOrThrow(PropertyKey.SignDisplay, IntlProviderOptions.ToStringValue(formatter.SignDisplay));
      obj.CreateDataPropertyOrThrow(PropertyKey.UseGrouping, formatter.UseGrouping);
      switch (formatter.Style) {
        case NumberStyle.Currency:
          obj.CreateDataPropertyOrThrow(PropertyKey.Currency, formatter.Currency);
          obj.CreateDataPropertyOrThrow(PropertyKey.CurrencyDisplay, IntlProviderOptions.ToStringValue(formatter.CurrencyDisplay));
          obj.CreateDataPropertyOrThrow(PropertyKey.CurrencySign, IntlProviderOptions.ToStringValue(formatter.CurrencySign));
          break;
        case NumberStyle.Unit:
          obj.CreateDataPropertyOrThrow(PropertyKey.Unit, formatter.Unit);
          obj.CreateDataPropertyOrThrow(PropertyKey.UnitDisplay, IntlProviderOptions.ToStringValue(formatter.UnitDisplay));
          break;
      }
      if (formatter.Digits.UseSignificantDigits) {
        obj.CreateDataPropertyOrThrow(PropertyKey.MinimumSignificantDigits, formatter.Digits.MinimumSignificantDigits);
        obj.CreateDataPropertyOrThrow(PropertyKey.MaximumSignificantDigits, formatter.Digits.MaximumSignificantDigits);
      } else {
        obj.CreateDataPropertyOrThrow(PropertyKey.MinimumIntegerDigits, formatter.Digits.MinimumSignificantDigits);
        obj.CreateDataPropertyOrThrow(PropertyKey.MinimumFractionDigits, formatter.Digits.MinimumFractionDigits);
        obj.CreateDataPropertyOrThrow(PropertyKey.MaximumFractionDigits, formatter.Digits.MaximumFractionDigits);
      }
      return obj;
    }
  }
}
