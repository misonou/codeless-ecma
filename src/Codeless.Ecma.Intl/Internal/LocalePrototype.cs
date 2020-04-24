using Codeless.Ecma.Intl.Utilities;
using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.Intl.Internal {
  [IntrinsicObject(IntlObjectKind.LocalePrototype)]
  internal static class LocalePrototype {
    [IntrinsicMember(WellKnownSymbol.ToStringTag, EcmaPropertyAttributes.Configurable)]
    public const string ToStringTag = "Intl.Locale";

    [IntrinsicMember]
    public static EcmaValue Maximize([This] EcmaValue thisValue) {
      Locale locale = thisValue.GetUnderlyingObject<Locale>();
      return locale.Maximize();
    }

    [IntrinsicMember]
    public static EcmaValue Minimize([This] EcmaValue thisValue) {
      Locale locale = thisValue.GetUnderlyingObject<Locale>();
      return locale.Minimize();
    }

    [IntrinsicMember]
    public static EcmaValue ToString([This] EcmaValue thisValue) {
      Locale locale = thisValue.GetUnderlyingObject<Locale>();
      return locale.LocaleString;
    }

    [IntrinsicMember(Getter = true)]
    public static EcmaValue BaseName([This] EcmaValue thisValue) {
      Locale locale = thisValue.GetUnderlyingObject<Locale>();
      return IntlUtility.RemoveUnicodeExtensions(locale.LocaleString);
    }

    [IntrinsicMember(Getter = true)]
    public static EcmaValue Calendar([This] EcmaValue thisValue) {
      Locale locale = thisValue.GetUnderlyingObject<Locale>();
      BcpLanguageTag tag = BcpLanguageTag.Parse(locale.LocaleString);
      return tag.UExtensions["ca"];
    }

    [IntrinsicMember(Getter = true)]
    public static EcmaValue CaseFirst([This] EcmaValue thisValue) {
      Locale locale = thisValue.GetUnderlyingObject<Locale>();
      BcpLanguageTag tag = BcpLanguageTag.Parse(locale.LocaleString);
      return tag.UExtensions["kf"];
    }

    [IntrinsicMember(Getter = true)]
    public static EcmaValue Collation([This] EcmaValue thisValue) {
      Locale locale = thisValue.GetUnderlyingObject<Locale>();
      BcpLanguageTag tag = BcpLanguageTag.Parse(locale.LocaleString);
      return tag.UExtensions["co"];
    }

    [IntrinsicMember(Getter = true)]
    public static EcmaValue HourCycle([This] EcmaValue thisValue) {
      Locale locale = thisValue.GetUnderlyingObject<Locale>();
      BcpLanguageTag tag = BcpLanguageTag.Parse(locale.LocaleString);
      return tag.UExtensions["hc"];
    }

    [IntrinsicMember(Getter = true)]
    public static EcmaValue Numeric([This] EcmaValue thisValue) {
      Locale locale = thisValue.GetUnderlyingObject<Locale>();
      BcpLanguageTag tag = BcpLanguageTag.Parse(locale.LocaleString);
      return tag.UExtensions["kn"] == "" || tag.UExtensions["kn"] == "true";
    }

    [IntrinsicMember(Getter = true)]
    public static EcmaValue NumberingSystem([This] EcmaValue thisValue) {
      Locale locale = thisValue.GetUnderlyingObject<Locale>();
      BcpLanguageTag tag = BcpLanguageTag.Parse(locale.LocaleString);
      return tag.UExtensions["nu"];
    }

    [IntrinsicMember(Getter = true)]
    public static EcmaValue Language([This] EcmaValue thisValue) {
      Locale locale = thisValue.GetUnderlyingObject<Locale>();
      BcpLanguageTag tag = BcpLanguageTag.Parse(locale.LocaleString);
      return tag.Language;
    }

    [IntrinsicMember(Getter = true)]
    public static EcmaValue Script([This] EcmaValue thisValue) {
      Locale locale = thisValue.GetUnderlyingObject<Locale>();
      BcpLanguageTag tag = BcpLanguageTag.Parse(locale.LocaleString);
      return tag.Script == "" ? default : tag.Script;
    }

    [IntrinsicMember(Getter = true)]
    public static EcmaValue Region([This] EcmaValue thisValue) {
      Locale locale = thisValue.GetUnderlyingObject<Locale>();
      BcpLanguageTag tag = BcpLanguageTag.Parse(locale.LocaleString);
      return tag.Region == "" ? default : tag.Region;
    }
  }
}
