﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.ErrorPrototype)]
  internal static class ErrorPrototype {
    [IntrinsicMember]
    public const string Message = "";

    [IntrinsicMember]
    public const string Name = "Error";

    [IntrinsicMember]
    public static string ToString([This] EcmaValue thisArg) {
      if (thisArg.Type != EcmaValueType.Object) {
        throw new EcmaTypeErrorException("");
      }
      EcmaValue name = thisArg["name"];
      EcmaValue msg = thisArg["msg"];
      string strName = (name == EcmaValue.Undefined) ? Name : name.ToString();
      string strMsg = (msg == EcmaValue.Undefined) ? String.Empty : msg.ToString();
      if (strName == "") {
        return strMsg;
      }
      if (strMsg == "") {
        return strName;
      }
      return String.Concat(strName, ": ", strMsg);
    }
  }

  #region NativeError
  [IntrinsicObject(WellKnownObject.EvalErrorPrototype)]
  internal static class EvalErrorPrototype {
    [IntrinsicMember]
    public const string Message = "";

    [IntrinsicMember]
    public const string Name = "EvalError";
  }

  [IntrinsicObject(WellKnownObject.RangeErrorPrototype)]
  internal static class RangeErrorPrototype {
    [IntrinsicMember]
    public const string Message = "";

    [IntrinsicMember]
    public const string Name = "RangeError";
  }

  [IntrinsicObject(WellKnownObject.ReferenceErrorPrototype)]
  internal static class ReferenceErrorPrototype {
    [IntrinsicMember]
    public const string Message = "";

    [IntrinsicMember]
    public const string Name = "ReferenceError";
  }

  [IntrinsicObject(WellKnownObject.SyntaxErrorPrototype)]
  internal static class SyntaxErrorPrototype {
    [IntrinsicMember]
    public const string Message = "";

    [IntrinsicMember]
    public const string Name = "SyntaxError";
  }

  [IntrinsicObject(WellKnownObject.TypeErrorPrototype)]
  internal static class TypeErrorPrototype {
    [IntrinsicMember]
    public const string Message = "";

    [IntrinsicMember]
    public const string Name = "TypeError";
  }

  [IntrinsicObject(WellKnownObject.UriErrorPrototype)]
  internal static class UriErrorPrototype {
    [IntrinsicMember]
    public const string Message = "";

    [IntrinsicMember]
    public const string Name = "URIError";
  }
  #endregion
}