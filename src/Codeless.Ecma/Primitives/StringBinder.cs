using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Codeless.Ecma.Primitives {
  internal class StringBinder : PrimitiveBinder<string> {
    public static readonly StringBinder Default = new StringBinder();

    protected StringBinder() { }

    public override string ToStringTag {
      get { return InternalString.ObjectTag.String; }
    }

    public override EcmaValueType ValueType {
      get { return EcmaValueType.String; }
    }

    public override EcmaNumberType NumberType {
      get { return EcmaNumberType.Invalid; }
    }

    public override string FromHandle(EcmaValueHandle handle) {
      return (string)handle.GetTargetAsGCHandle();
    }

    public override EcmaValueHandle ToHandle(string value) {
      throw new InvalidOperationException();
    }

    public override RuntimeObject ToRuntimeObject(string value) {
      return new TransientIntrinsicObject(value, WellKnownObject.StringPrototype);
    }

    public override bool ToBoolean(string value) {
      return value.Length > 0;
    }

    public override double ToDouble(string value) {
      double intValue;
      return Double.TryParse(value, out intValue) ? intValue : Double.NaN;
    }

    public override int ToInt32(string value) {
      int intValue;
      if (Int32.TryParse(value, out intValue)) {
        return intValue;
      }
      return DoubleBinder.Default.ToInt32(ToDouble(value));
    }

    public override long ToInt64(string value) {
      long intValue;
      if (Int64.TryParse(value, out intValue)) {
        return intValue;
      }
      return DoubleBinder.Default.ToInt64(ToDouble(value));
    }

    public override string ToString(string value) {
      return value;
    }

    public override EcmaValue ToNumber(string value) {
      return EcmaValueUtility.ParseStringNumericLiteral(value);
    }

    public override bool HasOwnProperty(string target, EcmaPropertyKey name) {
      return name.Name == "length" || base.HasOwnProperty(target, name);
    }

    public override bool TryGet(string target, EcmaPropertyKey name, out EcmaValue value) {
      if (name.IsCanonicalNumericIndex) {
        value = name.CanonicalNumericIndex < target.Length ? new String(target[(int)name.CanonicalNumericIndex], 1) : default;
        return true;
      }
      if (name.Name == "length") {
        value = target.Length;
        return true;
      }
      return base.TryGet(target, name, out value);
    }
  }
}
