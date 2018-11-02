using Codeless.Ecma.Runtime;
using Codeless.Ecma.Runtime.Intrinsics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Native {
  internal class NativeDateTimeBinder : PrimitiveBinder<DateTime> {
    public static readonly NativeDateTimeBinder Default = new NativeDateTimeBinder();

    protected NativeDateTimeBinder() { }

    public override EcmaNumberType NumberType {
      get { return EcmaNumberType.Invalid; }
    }

    public override string ToStringTag {
      get { return InternalString.ObjectTag.Date; }
    }

    public override EcmaValueType ValueType {
      get { return EcmaValueType.Object; }
    }

    public override DateTime FromHandle(EcmaValueHandle handle) {
      return new DateTime(handle.Value, DateTimeKind.Utc);
    }

    public override EcmaValueHandle ToHandle(DateTime value) {
      return new EcmaValueHandle(value.ToUniversalTime().Ticks);
    }

    public override bool ToBoolean(DateTime value) {
      return true;
    }

    public override double ToDouble(DateTime value) {
      return EcmaTimestamp.FromNativeDateTime(value);
    }

    public override int ToInt32(DateTime value) {
      return (int)EcmaTimestamp.FromNativeDateTime(value);
    }

    public override long ToInt64(DateTime value) {
      return EcmaTimestamp.FromNativeDateTime(value);
    }

    public override RuntimeObject ToRuntimeObject(DateTime value) {
      return new EcmaDate(value);
    }

    public override string ToString(DateTime value) {
      return DatePrototype.ToString(new EcmaValue(new EcmaDate(value)));
    }
  }
}
