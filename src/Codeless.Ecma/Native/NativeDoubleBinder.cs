using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Native {
  internal class NativeDoubleBinder : PrimitiveBinder<double> {
    public static readonly NativeDoubleBinder Default = new NativeDoubleBinder();

    protected NativeDoubleBinder() { }

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
      if (Double.IsNaN(value) || Math.Floor(Math.Abs(value)) != Math.Abs(value)) {
        return value.ToString();
      }
      return value.ToString("G0");
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
  }
}
