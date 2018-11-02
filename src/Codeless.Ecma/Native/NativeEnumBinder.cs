using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Codeless.Ecma.Runtime;

namespace Codeless.Ecma.Native {
  internal class NativeEnumBinder<T> : InternalDataBinder<T> where T : struct {
    public NativeEnumBinder() { }

    public override string ToStringTag {
      get { return InternalString.ObjectTag.Number; }
    }

    public override EcmaValueType ValueType {
      get { return EcmaValueType.Number; }
    }

    public override EcmaNumberType NumberType {
      get { return EcmaNumberType.Int64; }
    }

    public override RuntimeObject ToRuntimeObject(T value) {
      return new TransientIntrinsicObject(new EcmaValue(ToHandle(value), this), WellKnownObject.NumberPrototype);
    }

    public override bool ToBoolean(T value) {
      return ToHandle(value).Value != 0;
    }

    public override double ToDouble(T value) {
      return ToHandle(value).Value;
    }

    public override int ToInt32(T value) {
      return (int)ToHandle(value).Value;
    }

    public override long ToInt64(T value) {
      return ToHandle(value).Value;
    }

    public override string ToString(T value) {
      return value.ToString();
    }
  }
}
