using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.NumberConstructor)]
  internal static class NumberConstructor {
    [IntrinsicConstructor(ObjectType = typeof(IntrinsicObject))]
    public static EcmaValue Number([NewTarget] RuntimeObject constructor, [This] EcmaValue thisValue, EcmaValue value) {
      if (constructor == null) {
        return value.ToNumber();
      }
      ((IntrinsicObject)thisValue.ToObject()).IntrinsicValue = value.ToNumber();
      return thisValue;
    }

    [IntrinsicMember("EPSILON", EcmaPropertyAttributes.None)]
    public const double Epsilon = Double.Epsilon;

    [IntrinsicMember("MAX_VALUE", EcmaPropertyAttributes.None)]
    public const double MaxValue = Double.MaxValue;

    [IntrinsicMember("MIN_VALUE", EcmaPropertyAttributes.None)]
    public const double MinValue = Double.MinValue;

    [IntrinsicMember("MAX_SAFE_INTEGER", EcmaPropertyAttributes.None)]
    public const long MaxSafeInteger = (1 << 53) - 1;

    [IntrinsicMember("MIN_SAFE_INTEGER", EcmaPropertyAttributes.None)]
    public const long MinSafeInteger = -MaxSafeInteger;

    [IntrinsicMember("POSITIVE_INFINITY", EcmaPropertyAttributes.None)]
    public const double PositiveInfinity = Double.PositiveInfinity;

    [IntrinsicMember("NEGATIVE_INFINITY", EcmaPropertyAttributes.None)]
    public const double NegativeInfinity = Double.NegativeInfinity;

    [IntrinsicMember("NaN", EcmaPropertyAttributes.None)]
    public const double NaN = Double.NaN;

    [IntrinsicMember]
    public static bool IsFinite(EcmaValue value) {
      return value.IsFinite;
    }

    [IntrinsicMember]
    public static bool IsInteger(EcmaValue value) {
      return value.ToNumber().IsInteger;
    }

    [IntrinsicMember]
    public static bool IsNaN(EcmaValue value) {
      return value.IsNaN;
    }

    [IntrinsicMember]
    public static bool IsSafeInteger(EcmaValue value) {
      value = value.ToNumber();
      if (value.IsInteger) {
        long l = value.ToInt64();
        return l >= MinSafeInteger && l <= MaxSafeInteger;
      }
      return false;
    }

    [IntrinsicMember(Global = true)]
    public static EcmaValue ParseFloat(EcmaValue value) {
      return Global.ParseFloat(value);
    }

    [IntrinsicMember(Global = true)]
    public static EcmaValue ParseInt(EcmaValue value, EcmaValue radix) {
      return Global.ParseInt(value, radix);
    }
  }
}
