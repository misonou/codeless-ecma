using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Primitives {
  internal class DoubleBinder : PrimitiveBinder<double> {
    public static readonly DoubleBinder Default = new DoubleBinder();

    protected DoubleBinder() { }

    public override string ToStringTag {
      get { return InternalString.ObjectTag.Number; }
    }

    public override EcmaNumberType NumberType {
      get { return EcmaNumberType.Double; }
    }

    public override EcmaValueType ValueType {
      get { return EcmaValueType.Number; }
    }

    public override double FromHandle(EcmaValueHandle handle) {
      return BitConverter.Int64BitsToDouble(handle.Value);
    }

    public override EcmaValueHandle ToHandle(double value) {
      return new EcmaValueHandle(BitConverter.DoubleToInt64Bits(value));
    }

    public override RuntimeObject ToRuntimeObject(double value) {
      return new TransientIntrinsicObject(value, WellKnownObject.NumberPrototype);
    }

    public override double ToDouble(double value) {
      return value;
    }

    public override int ToInt32(double value) {
      return (int)value;
    }

    public override long ToInt64(double value) {
      return (long)value;
    }

    public override string ToString(double value) {
      if (Double.IsPositiveInfinity(value)) {
        return "Infinity";
      }
      if (Double.IsNegativeInfinity(value)) {
        return "-Infinity";
      }
      if (Double.IsNaN(value)) {
        return "NaN";
      }
      if (value == 0) {
        return "0";
      }
      double abs = Math.Abs(value);
      if (abs < EXP_POS && abs >= EXP_NEG) {
        return value.ToString("0.####################");
      }
      return value.ToString("0.####################e+0");
    }

    public override bool ToBoolean(double value) {
      return value != 0 && !Double.IsNaN(value);
    }

    public override EcmaValue ToNumber(double value) {
      return value;
    }

    public override int GetHashCode(double value) {
      if (Math.Floor(value) == value) {
        long m = (long)value;
        return m.GetHashCode();
      }
      return base.GetHashCode(value);
    }

    private readonly double EXP_POS = Math.Pow(10, 21);
    private readonly double EXP_NEG = Math.Pow(10, -6);
  }
}
