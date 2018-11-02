using Codeless.Ecma.Runtime.Intrinsics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Codeless.Ecma {
  [IntrinsicObject(WellKnownObject.Global)]
  public static class EcmaGlobal {
    [IntrinsicMember(EcmaPropertyAttributes.None)]
    public static readonly EcmaValue Infinity = EcmaValue.Infinity;

    [IntrinsicMember(EcmaPropertyAttributes.None)]
    public static readonly EcmaValue NaN = EcmaValue.NaN;

    [IntrinsicMember("undefined", EcmaPropertyAttributes.None)]
    public static readonly EcmaValue Undefined = EcmaValue.Undefined;

    [IntrinsicMember]
    public static bool IsFinite(EcmaValue value) {
      return value.IsFinite;
    }

    [IntrinsicMember]
    public static bool IsNaN(EcmaValue value) {
      value = value.ToNumber();
      return value.IsNaN;
    }

    [IntrinsicMember]
    public static EcmaValue ParseInt(EcmaValue str, EcmaValue radix) {
      throw new NotImplementedException();
    }

    [IntrinsicMember]
    public static EcmaValue ParseFloat(EcmaValue str) {
      throw new NotImplementedException();
    }

    [IntrinsicMember]
    public static EcmaValue EncodeURI(EcmaValue str) {
      throw new NotImplementedException();
    }

    [IntrinsicMember]
    public static EcmaValue DecodeURI(EcmaValue str) {
      throw new NotImplementedException();
    }

    [IntrinsicMember]
    public static EcmaValue EncodeURIComponent(EcmaValue str) {
      throw new NotImplementedException();
    }

    [IntrinsicMember]
    public static EcmaValue DecodeURIComponent(EcmaValue str) {
      throw new NotImplementedException();
    }

    [IntrinsicMember]
    public static EcmaValue Escape(EcmaValue str) {
      throw new NotImplementedException();
    }

    [IntrinsicMember]
    public static EcmaValue Unescape(EcmaValue str) {
      throw new NotImplementedException();
    }
  }
}