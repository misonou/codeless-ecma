using Codeless.Ecma.Runtime.Intrinsics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma {
  [IntrinsicObject(WellKnownObject.Math, Global = true, Name = "Math")]
  public static class EcmaMath {
    private static Random rnd = new Random();

    [IntrinsicMember(WellKnownSymbol.ToStringTag, EcmaPropertyAttributes.Configurable)]
    public const string ToStringTag = "Math";

    [IntrinsicMember(EcmaPropertyAttributes.None)]
    public const double E = Math.E;

    [IntrinsicMember(EcmaPropertyAttributes.None)]
    public static readonly double LN10 = Math.Log(10);

    [IntrinsicMember(EcmaPropertyAttributes.None)]
    public static readonly double LN2 = Math.Log(2);

    [IntrinsicMember(EcmaPropertyAttributes.None)]
    public static readonly double LOG10E = Math.Log10(Math.E);

    [IntrinsicMember(EcmaPropertyAttributes.None)]
    public static readonly double LOG2E = Math.Log(Math.E, 2);

    [IntrinsicMember(EcmaPropertyAttributes.None)]
    public const double PI = Math.PI;

    [IntrinsicMember(EcmaPropertyAttributes.None)]
    public static readonly double SQRT1_2 = Math.Sqrt(0.5d);

    [IntrinsicMember(EcmaPropertyAttributes.None)]
    public static readonly double SQRT2 = Math.Sqrt(2d);

    [IntrinsicMember]
    public static EcmaValue Abs(EcmaValue value) {
      switch (EcmaValue.GetNumberCoercion(value)) {
        case EcmaNumberType.Double:
          return Math.Abs(value.ToDouble());
        case EcmaNumberType.Int64:
          return Math.Abs(value.ToInt64());
        case EcmaNumberType.Int32:
          return Math.Abs(value.ToInt32());
      }
      return Abs(value.ToNumber());
    }

    [IntrinsicMember]
    public static EcmaValue Acos(EcmaValue value) {
      return Math.Acos(value.ToDouble());
    }

    [IntrinsicMember]
    public static EcmaValue Acosh(EcmaValue value) {
      double x = value.ToDouble();
      return Math.Log(x + Math.Sqrt(x * x - 1));
    }

    [IntrinsicMember]
    public static EcmaValue Asin(EcmaValue value) {
      return Math.Asin(value.ToDouble());
    }

    [IntrinsicMember]
    public static EcmaValue Asinh(EcmaValue value) {
      double x = value.ToDouble();
      return Double.IsInfinity(x) || x == 0 ? x : Math.Log(x + Math.Sqrt(x * x + 1));
    }

    [IntrinsicMember]
    public static EcmaValue Atan(EcmaValue value) {
      return Math.Atan(value.ToDouble());
    }

    [IntrinsicMember]
    public static EcmaValue Atanh(EcmaValue value) {
      double x = value.ToDouble();
      return x == 0 ? x : Math.Log((1 + x) / (1 - x)) / 2;
    }

    [IntrinsicMember]
    public static EcmaValue Atan2(EcmaValue y, EcmaValue x) {
      return Math.Atan2(y.ToDouble(), x.ToDouble());
    }

    [IntrinsicMember]
    public static EcmaValue Ceil(EcmaValue value) {
      if (value.Type != EcmaValueType.Number) {
        value = value.ToNumber();
      }
      if (EcmaValue.GetNumberCoercion(value) != EcmaNumberType.Double) {
        return value;
      }
      double x = value.ToDouble();
      double c = Math.Ceiling(x);
      return c == 0 && x < 0 ? -0d : c;
    }

    [IntrinsicMember]
    public static EcmaValue Cbrt(EcmaValue value) {
      double x = value.ToDouble();
      return Double.IsInfinity(x) || x == 0 ? x : Math.Pow(value.ToDouble(), 1f / 3);
    }

    [IntrinsicMember]
    public static EcmaValue Clz32(EcmaValue value) {
      uint x = value.ToUInt32();
      return x == 0 ? 32 : 31 - (int)Math.Floor(Math.Log(x) * LOG2E);
    }

    [IntrinsicMember]
    public static EcmaValue Cos(EcmaValue value) {
      return Math.Cos(value.ToDouble());
    }

    [IntrinsicMember]
    public static EcmaValue Cosh(EcmaValue value) {
      double x = value.ToDouble();
      return (Math.Exp(x) + Math.Exp(-x)) / 2;
    }

    [IntrinsicMember]
    public static EcmaValue Exp(EcmaValue value) {
      return Math.Exp(value.ToDouble());
    }

    [IntrinsicMember]
    public static EcmaValue Expm1(EcmaValue value) {
      double x = value.ToDouble();
      if (x == 0) {
        return x;
      }
      if (Math.Abs(x) < 1e-5) {
        return x + 0.5 * x * x;
      }
      return Math.Exp(x) - 1d;
    }

    [IntrinsicMember]
    public static EcmaValue Floor(EcmaValue value) {
      if (value.Type != EcmaValueType.Number) {
        value = value.ToNumber();
      }
      if (EcmaValue.GetNumberCoercion(value) != EcmaNumberType.Double) {
        return value;
      }
      return Math.Floor(value.ToDouble());
    }

    [IntrinsicMember]
    public static EcmaValue Fround(EcmaValue value) {
      return (float)value.ToDouble();
    }

    [IntrinsicMember(FunctionLength = 2)]
    public static EcmaValue Hypot(params EcmaValue[] values) {
      double y = 0;
      foreach (EcmaValue value in values) {
        double x = value.ToDouble();
        if (Double.IsInfinity(x)) {
          return EcmaValue.Infinity;
        }
        y += x * x;
      }
      return Math.Sqrt(y);
    }

    [IntrinsicMember]
    public static EcmaValue Imul(EcmaValue x, EcmaValue y) {
      uint a = x.ToUInt32();
      uint b = y.ToUInt32();
      uint ah = (a >> 16) & 0xffff;
      uint al = a & 0xffff;
      uint bh = (b >> 16) & 0xffff;
      uint bl = b & 0xffff;
      uint result = (al * bl) + ((ah * bl + al * bh) << 16);
      return (int)result;
    }

    [IntrinsicMember]
    public static EcmaValue Log(EcmaValue value) {
      return Math.Log(value.ToDouble());
    }

    [IntrinsicMember]
    public static EcmaValue Log1p(EcmaValue value) {
      double x = value.ToDouble();
      if (x < -1) {
        return EcmaValue.NaN;
      }
      if (Math.Abs(x) > 1e-4) {
        return Math.Log(1 + x);
      }
      return (-0.5 * x + 1.0) * x;
    }

    [IntrinsicMember]
    public static EcmaValue Log2(EcmaValue value) {
      return Math.Log(value.ToDouble()) / LN2;
    }

    [IntrinsicMember]
    public static EcmaValue Log10(EcmaValue value) {
      if (value.Type != EcmaValueType.Number) {
        value = value.ToNumber();
      }
      return Math.Log10(value.ToDouble());
    }

    [IntrinsicMember(FunctionLength = 2)]
    public static EcmaValue Max(params EcmaValue[] arr) {
      EcmaValue x = EcmaValue.NegativeInfinity;
      foreach (EcmaValue val in arr) {
        EcmaValue num = val.ToNumber();
        if (num.IsNaN) {
          return EcmaValue.NaN;
        }
        if (num.Equals(EcmaValue.NegativeZero, EcmaValueComparison.SameValue) && x >= 0) {
          continue;
        }
        if (x <= num) {
          x = num;
        }
      }
      return x;
    }

    [IntrinsicMember(FunctionLength = 2)]
    public static EcmaValue Min(params EcmaValue[] arr) {
      EcmaValue x = EcmaValue.Infinity;
      foreach (EcmaValue val in arr) {
        EcmaValue num = val.ToNumber();
        if (num.IsNaN) {
          return EcmaValue.NaN;
        }
        if (num.Equals(0, EcmaValueComparison.SameValue) && x <= 0) {
          continue;
        }
        if (x >= num) {
          x = num;
        }
      }
      return x;
    }

    [IntrinsicMember]
    public static EcmaValue Pow(EcmaValue x, EcmaValue y) {
      if ((x == 1 || x == -1) && (y == EcmaValue.Infinity || y == EcmaValue.NegativeInfinity)) {
        return EcmaValue.NaN;
      }
      return Math.Pow(x.ToDouble(), y.ToDouble());
    }

    [IntrinsicMember]
    public static EcmaValue Random() {
      return rnd.NextDouble();
    }

    [IntrinsicMember]
    public static EcmaValue Round(EcmaValue value) {
      if (value.Type != EcmaValueType.Number) {
        value = value.ToNumber();
      }
      if (EcmaValue.GetNumberCoercion(value) != EcmaNumberType.Double) {
        return value;
      }
      double x = value.ToDouble();
      if (x == 0) {
        return x;
      }
      double r = Math.Floor(x + 0.5);
      return r == 0 && x < 0 ? -0d : r;
    }

    [IntrinsicMember]
    public static EcmaValue Sign(EcmaValue value) {
      switch (EcmaValue.GetNumberCoercion(value)) {
        case EcmaNumberType.Int64:
          return Math.Sign(value.ToInt64());
        case EcmaNumberType.Int32:
          return Math.Sign(value.ToInt32());
      }
      double x = value.ToDouble();
      return Double.IsNaN(x) ? EcmaValue.NaN : x == 0 ? x : Math.Sign(x);
    }

    [IntrinsicMember]
    public static EcmaValue Sin(EcmaValue value) {
      return Math.Sin(value.ToDouble());
    }

    [IntrinsicMember]
    public static EcmaValue Sinh(EcmaValue value) {
      double x = value.ToDouble();
      return x == 0 ? x : (Math.Exp(x) - Math.Exp(-x)) / 2;
    }

    [IntrinsicMember]
    public static EcmaValue Sqrt(EcmaValue value) {
      return Math.Sqrt(value.ToDouble());
    }

    [IntrinsicMember]
    public static EcmaValue Tan(EcmaValue value) {
      return Math.Tan(value.ToDouble());
    }

    [IntrinsicMember]
    public static EcmaValue Tanh(EcmaValue value) {
      double x = value.ToDouble();
      if (Double.IsInfinity(x)) {
        return x > 0 ? 1 : -1;
      }
      return x == 0 ? x : (Math.Exp(x) - Math.Exp(-x)) / (Math.Exp(x) + Math.Exp(-x));
    }

    [IntrinsicMember]
    public static EcmaValue Trunc(EcmaValue value) {
      if (value.Type != EcmaValueType.Number) {
        value = value.ToNumber();
      }
      if (EcmaValue.GetNumberCoercion(value) != EcmaNumberType.Double) {
        return value;
      }
      return Math.Truncate(value.ToDouble());
    }
  }
}
