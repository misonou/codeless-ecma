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
      return new TransientPrimitiveObject(value, WellKnownObject.StringPrototype);
    }

    public override bool ToBoolean(string value) {
      return value.Length > 0;
    }

    public override double ToDouble(string value) {
      return EcmaValueUtility.ParseStringNumericLiteral(value).ToDouble();
    }

    public override int ToInt32(string value) {
      return EcmaValueUtility.ParseStringNumericLiteral(value).ToInt32();
    }

    public override long ToInt64(string value) {
      return EcmaValueUtility.ParseStringNumericLiteral(value).ToInt64();
    }

    public override string ToString(string value) {
      return value;
    }

    public override EcmaValue ToNumber(string value) {
      return EcmaValueUtility.ParseStringNumericLiteral(value);
    }

    public override bool HasOwnProperty(string target, EcmaPropertyKey name) {
      return name.Name == "length" || (name.IsArrayIndex && name.ToArrayIndex() < target.Length) || base.HasOwnProperty(target, name);
    }

    public override bool TryGet(string target, EcmaPropertyKey name, out EcmaValue value) {
      if (name.IsArrayIndex) {
        return EcmaValueUtility.TryIndexByPropertyKey(target, name, out value);
      }
      if (name.Name == "length") {
        value = target.Length;
        return true;
      }
      return base.TryGet(target, name, out value);
    }
  }
}
