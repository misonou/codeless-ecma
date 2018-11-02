﻿using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Native {
  internal class NativeInt32Binder : PrimitiveBinder<int> {
    public static readonly NativeInt32Binder Default = new NativeInt32Binder();

    protected NativeInt32Binder() { }

    public override string ToStringTag {
      get { return InternalString.ObjectTag.Number; }
    }

    public override EcmaNumberType NumberType {
      get { return EcmaNumberType.Int32; }
    }

    public override EcmaValueType ValueType {
      get { return EcmaValueType.Number; }
    }

    public override int FromHandle(EcmaValueHandle handle) {
      return (int)handle.Value;
    }

    public override EcmaValueHandle ToHandle(int value) {
      return new EcmaValueHandle(value);
    }

    public override RuntimeObject ToRuntimeObject(int value) {
      return new TransientIntrinsicObject(value, WellKnownObject.NumberPrototype);
    }

    public override double ToDouble(int value) {
      return value;
    }

    public override int ToInt32(int value) {
      return value;
    }

    public override long ToInt64(int value) {
      return value;
    }

    public override string ToString(int value) {
      return value.ToString();
    }

    public override bool ToBoolean(int value) {
      return value != 0;
    }

    public override EcmaValue ToNumber(int value) {
      return value;
    }
  }
}