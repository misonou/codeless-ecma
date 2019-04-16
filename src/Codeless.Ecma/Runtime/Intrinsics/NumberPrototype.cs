using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.NumberPrototype)]
  internal static class NumberPrototype {
    [IntrinsicMember]
    public static EcmaValue ToExponential([This] EcmaValue thisValue, EcmaValue digits) {
      EcmaValue number = thisValue.GetIntrinsicPrimitiveValue(EcmaValueType.Number);
      if (digits != default) {
        digits = digits.ToNumber();
      }
      if (!number.IsFinite) {
        return number.ToString();
      }
      int b = digits == default ? 0 : digits.ToInt32Checked();
      if (b < 0 || b > 100) {
        throw new EcmaRangeErrorException("toExponential() argument must be between 0 and 100");
      }
      if (number == 0) {
        return b == 0 ? "0e+0" : "0." + new String('0', b) + "e+0";
      }
      if (digits == default) {
        return GetString(number.ToDouble(), -1, true);
      }
      return GetString(number.ToDouble(), b + 1, true);
    }

    [IntrinsicMember]
    public static EcmaValue ToFixed([This] EcmaValue thisValue, EcmaValue digits) {
      EcmaValue number = thisValue.GetIntrinsicPrimitiveValue(EcmaValueType.Number);
      int b = digits == default ? 0 : digits.ToInt32Checked();
      if (!number.IsFinite) {
        return number.ToString();
      }
      if (b < 0 || b > 100) {
        throw new EcmaRangeErrorException("toFixed() argument must be between 0 and 100");
      }
      if (EcmaValue.GetNumberCoercion(number) == EcmaNumberType.Double) {
        double value = number.ToDouble();
        return value >= 1e21 ? number.ToString() : GetString(value, b + 1, false);
      }
      return b == 0 ? number.ToString() : number.ToString() + "." + new String('0', b);
    }

    [IntrinsicMember]
    public static EcmaValue ToLocaleString([This] EcmaValue thisValue) {
      return ToString(thisValue, 10);
    }

    [IntrinsicMember]
    public static EcmaValue ToPrecision([This] EcmaValue thisValue, EcmaValue digits) {
      EcmaValue number = thisValue.GetIntrinsicPrimitiveValue(EcmaValueType.Number);
      if (digits == default) {
        return number.ToString();
      }
      digits = digits.ToNumber();
      if (!number.IsFinite) {
        return number.ToString();
      }
      int b = digits.ToInt32Checked();
      if (b < 1 || b > 100) {
        throw new EcmaRangeErrorException("toPrecision() argument must be between 1 and 100");
      }
      if (number == 0) {
        return b == 1 ? "0" : "0." + new String('0', b - 1);
      }
      return GetString(number.ToDouble(), b, false);
    }

    [IntrinsicMember]
    public static EcmaValue ToString([This] EcmaValue thisValue, EcmaValue radix) {
      EcmaValue number = thisValue.GetIntrinsicPrimitiveValue(EcmaValueType.Number);
      int b = radix == default ? 10 : radix.ToInt32Checked();
      if (b < 2 || b > 36) {
        throw new EcmaRangeErrorException("toString() radix argument must be between 2 and 36");
      }
      if (!number.IsFinite) {
        return number.ToString();
      }
      if (b == 10) {
        return number.ToString();
      }
      if (number == 0) {
        return "0";
      }
      if (EcmaValue.GetNumberCoercion(number) == EcmaNumberType.Double) {
        return GetString(number.ToDouble(), b);
      }
      return GetString(number.ToInt64(), b);
    }

    [IntrinsicMember]
    [EcmaSpecification("thisNumberValue", EcmaSpecificationKind.AbstractOperations)]
    public static EcmaValue ValueOf([This] EcmaValue thisValue) {
      return thisValue.GetIntrinsicPrimitiveValue(EcmaValueType.Number);
    }

    private const string letters = "0123456789abcdefghijklmnopqrstuvwxyz";

    private static string GetString(long signedValue, int radix) {
      StringBuilder sb = new StringBuilder();
      long value = Math.Abs(signedValue);
      if (signedValue == 0) {
        return "0";
      }
      if (radix == 2 || radix == 8 || radix == 10 || radix == 16) {
        return Convert.ToString(signedValue, radix);
      }
      while (value >= 1) {
        long q = value / radix;
        sb.Insert(0, letters[(int)(value - q * radix)]);
        value = q;
      }
      if (signedValue < 0) {
        sb.Insert(0, '-');
      }
      return sb.ToString();
    }

    private static string GetString(double signedValue, int radix) {
      double value = Math.Abs(signedValue);
      int exp = 0;
      if (value > Int64.MaxValue) {
        exp = (int)Math.Ceiling(Math.Log(value / Int64.MaxValue, radix));
        value *= Math.Pow(radix, -exp);
      }

      long intValue = (long)value;
      StringBuilder sb = new StringBuilder(GetString(intValue, radix));
      exp += sb.Length;
      value -= intValue;
      for (int p = 0; value > 0 && p < 21; p++) {
        value *= radix;
        int d = (int)value;
        if (d >= radix) {
          break;
        }
        value -= d;
        sb.Append(letters[d]);
      }
      if (exp < sb.Length) {
        sb.Insert(exp, '.');
      }
      if (value < 0) {
        sb.Insert(0, '-');
      }
      return sb.ToString();
    }

    private static string GetString(double signedValue, int precision, bool useExponent) {
      double value = Math.Abs(signedValue);
      int exp = (int)Math.Floor(Math.Log(value, 10));
      if (value / Math.Pow(10, exp) >= 10) {
        exp++;
      }
      if (exp < -6 || exp >= precision) {
        useExponent = true;
      }

      StringBuilder sb = new StringBuilder();
      if (precision > 0) {
        value *= Math.Pow(10, precision - exp - 1);
        double f = Math.Floor(value);
        value = value - f >= 0.5 ? f + 1 : f;
        if (value >= Math.Pow(10, precision)) {
          value /= 10;
          exp++;
        }
        sb.Append(value.ToString("0"));
      } else {
        value *= Math.Pow(10, -exp);
        sb.Append(value.ToString().Replace(".", ""));
      }
      if (useExponent) {
        if (sb.Length > 1) {
          sb.Insert(1, '.');
        }
        sb.Append('e');
        if (exp >= 0) {
          sb.Append('+');
        }
        sb.Append(exp);
      } else {
        if (exp < 0) {
          sb.Insert(0, "0", -exp);
          exp = 0;
        }
        if (sb.Length - 1 > exp) {
          sb.Insert(exp + 1, '.');
        }
      }
      if (signedValue < 0) {
        sb.Insert(0, '-');
      }
      return sb.ToString();
    }
  }
}
