using Codeless.Ecma.Runtime;
using System;
using System.Linq;

namespace Codeless.Ecma.Intl.Internal {
  [IntrinsicObject(IntlObjectKind.RelativeTimeFormatPrototype)]
  internal static class RelativeTimeFormatPrototype {
    [IntrinsicMember(WellKnownSymbol.ToStringTag, EcmaPropertyAttributes.Configurable)]
    public const string ToStringTag = "Object";

    [IntrinsicMember(Getter = true)]
    public static EcmaValue Format([This] EcmaValue thisValue) {
      RelativeTimeFormat formatter = thisValue.GetUnderlyingObject<RelativeTimeFormat>();
      return formatter.BoundFormat;
    }

    [IntrinsicMember]
    public static EcmaValue FormatToParts([This] EcmaValue thisValue, EcmaValue value, EcmaValue unit) {
      RelativeTimeFormat formatter = thisValue.GetUnderlyingObject<RelativeTimeFormat>();
      return formatter.Format(value, unit, out string[] units).ToPartArray(PropertyKey.Unit, units);
    }

    [IntrinsicMember]
    public static EcmaValue ResolvedOptions([This] EcmaValue thisValue) {
      RelativeTimeFormat formatter = thisValue.GetUnderlyingObject<RelativeTimeFormat>();
      EcmaObject obj = new EcmaObject();
      obj.CreateDataPropertyOrThrow(PropertyKey.Locale, formatter.Locale);
      obj.CreateDataPropertyOrThrow(PropertyKey.Style, IntlProviderOptions.ToStringValue(formatter.Style));
      obj.CreateDataPropertyOrThrow(PropertyKey.Numeric, IntlProviderOptions.ToStringValue(formatter.Numeric));
      obj.CreateDataPropertyOrThrow(PropertyKey.NumberingSystem, formatter.NumberingSystem);
      return obj;
    }
  }
}
