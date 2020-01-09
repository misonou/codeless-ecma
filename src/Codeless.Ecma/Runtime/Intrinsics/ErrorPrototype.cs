using System;
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
    public static string ToString([This] EcmaValue thisValue) {
      Guard.ArgumentIsObject(thisValue);
      EcmaValue name = thisValue[WellKnownProperty.Name];
      EcmaValue msg = thisValue[WellKnownProperty.Message];
      string strName = (name == default) ? ErrorPrototype.Name : name.ToStringOrThrow();
      string strMsg = (msg == default) ? String.Empty : msg.ToStringOrThrow();
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
  [IntrinsicObject(WellKnownObject.EvalErrorPrototype, Prototype = WellKnownObject.ErrorPrototype)]
  internal static class EvalErrorPrototype {
    [IntrinsicMember]
    public const string Message = "";

    [IntrinsicMember]
    public const string Name = "EvalError";
  }

  [IntrinsicObject(WellKnownObject.RangeErrorPrototype, Prototype = WellKnownObject.ErrorPrototype)]
  internal static class RangeErrorPrototype {
    [IntrinsicMember]
    public const string Message = "";

    [IntrinsicMember]
    public const string Name = "RangeError";
  }

  [IntrinsicObject(WellKnownObject.ReferenceErrorPrototype, Prototype = WellKnownObject.ErrorPrototype)]
  internal static class ReferenceErrorPrototype {
    [IntrinsicMember]
    public const string Message = "";

    [IntrinsicMember]
    public const string Name = "ReferenceError";
  }

  [IntrinsicObject(WellKnownObject.SyntaxErrorPrototype, Prototype = WellKnownObject.ErrorPrototype)]
  internal static class SyntaxErrorPrototype {
    [IntrinsicMember]
    public const string Message = "";

    [IntrinsicMember]
    public const string Name = "SyntaxError";
  }

  [IntrinsicObject(WellKnownObject.TypeErrorPrototype, Prototype = WellKnownObject.ErrorPrototype)]
  internal static class TypeErrorPrototype {
    [IntrinsicMember]
    public const string Message = "";

    [IntrinsicMember]
    public const string Name = "TypeError";
  }

  [IntrinsicObject(WellKnownObject.UriErrorPrototype, Prototype = WellKnownObject.ErrorPrototype)]
  internal static class UriErrorPrototype {
    [IntrinsicMember]
    public const string Message = "";

    [IntrinsicMember]
    public const string Name = "URIError";
  }
  #endregion
}
