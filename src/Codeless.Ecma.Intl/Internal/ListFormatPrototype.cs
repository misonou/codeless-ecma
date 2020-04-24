using Codeless.Ecma.Runtime;
using System;
using System.Linq;

namespace Codeless.Ecma.Intl.Internal {
  [IntrinsicObject(IntlObjectKind.ListFormatPrototype)]
  internal static class ListFormatPrototype {
    [IntrinsicMember(WellKnownSymbol.ToStringTag, EcmaPropertyAttributes.Configurable)]
    public const string ToStringTag = "Object";

    [IntrinsicMember(Getter = true)]
    public static EcmaValue Format([This] EcmaValue thisValue) {
      ListFormat formatter = thisValue.GetUnderlyingObject<ListFormat>();
      return formatter.BoundFormat;
    }

    [IntrinsicMember]
    public static EcmaValue FormatToParts([This] EcmaValue thisValue, EcmaValue list) {
      ListFormat formatter = thisValue.GetUnderlyingObject<ListFormat>();
      return formatter.Format(list).ToPartArray();
    }

    [IntrinsicMember]
    public static EcmaValue ResolvedOptions([This] EcmaValue thisValue) {
      ListFormat formatter = thisValue.GetUnderlyingObject<ListFormat>(); EcmaObject obj = new EcmaObject();
      obj.CreateDataPropertyOrThrow(PropertyKey.Locale, formatter.Locale);
      obj.CreateDataPropertyOrThrow(PropertyKey.Style, IntlProviderOptions.ToStringValue(formatter.Style));
      obj.CreateDataPropertyOrThrow(PropertyKey.Type, IntlProviderOptions.ToStringValue(formatter.Type));
      return obj;
    }
  }
}
