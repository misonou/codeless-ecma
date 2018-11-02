using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Codeless.Ecma.Runtime;

namespace Codeless.Ecma.Native {
  internal class NativeBooleanBinder : PrimitiveBinder<bool> {
    public static readonly NativeBooleanBinder Default = new NativeBooleanBinder();

    protected NativeBooleanBinder() { }

    public override string ToStringTag {
      get { return InternalString.ObjectTag.Boolean; }
    }

    public override EcmaNumberType NumberType {
      get { return EcmaNumberType.Int32; }
    }

    public override EcmaValueType ValueType {
      get { return EcmaValueType.Boolean; }
    }

    public override bool FromHandle(EcmaValueHandle handle) {
      return handle.Value > 0 ? true : false;
    }

    public override EcmaValueHandle ToHandle(bool value) {
      return new EcmaValueHandle(value ? 1 : 0);
    }

    public override RuntimeObject ToRuntimeObject(bool value) {
      return new TransientIntrinsicObject(value, WellKnownObject.BooleanPrototype);
    }

    public override double ToDouble(bool value) {
      return value ? 1 : 0;
    }

    public override bool ToBoolean(bool value) {
      return value;
    }

    public override int ToInt32(bool value) {
      return value ? 1 : 0;
    }

    public override long ToInt64(bool value) {
      return value ? 1 : 0;
    }

    public override string ToString(bool value) {
      return value ? "true" : "false";
    }

    public override EcmaValue ToNumber(bool value) {
      return value ? 1 : 0;
    }
  }
}
