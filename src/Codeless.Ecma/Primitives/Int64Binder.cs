﻿using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Primitives {
  internal class Int64Binder : PrimitiveBinder<long> {
    public static readonly Int64Binder Default = new Int64Binder();

    protected Int64Binder() { }

    public override string ToStringTag {
      get { return InternalString.ObjectTag.Number; }
    }

    public override EcmaNumberType NumberType {
      get { return EcmaNumberType.Int64; }
    }

    public override EcmaValueType ValueType {
      get { return EcmaValueType.Number; }
    }

    public override long FromHandle(EcmaValueHandle handle) {
      return handle.Value;
    }
    
    public override EcmaValueHandle ToHandle(long value) {
      return new EcmaValueHandle(value);
    }

    public override RuntimeObject ToRuntimeObject(long value) {
      return new TransientPrimitiveObject(value, WellKnownObject.NumberPrototype);
    }

    public override double ToDouble(long value) {
      return value;
    }

    public override int ToInt32(long value) {
      return (int)value;
    }

    public override long ToInt64(long value) {
      return value;
    }

    public override string ToString(long value) {
      return value.ToString();
    }

    public override bool ToBoolean(long value) {
      return value != 0;
    }

    public override EcmaValue ToNumber(long value) {
      return value;
    }
  }
}
