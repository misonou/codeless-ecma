using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.ObjectPrototype)]
  internal static class ObjectPrototype {
    [IntrinsicMember]
    public static EcmaValue HasOwnProperty([This] EcmaValue thisArg, EcmaValue key) {
      return thisArg.HasOwnProperty(EcmaPropertyKey.FromValue(key));
    }

    [IntrinsicMember]
    public static EcmaValue IsPrototypeOf([This] EcmaValue thisArg, EcmaValue obj) {
      if (obj.Type != EcmaValueType.Object) {
        return false;
      }
      RuntimeObject thisObj = thisArg.ToRuntimeObject();
      for (RuntimeObject v = obj.ToRuntimeObject(); v != null; v = v.GetPrototypeOf()) {
        if (v == thisObj) {
          return true;
        }
      }
      return false;
    }

    [IntrinsicMember]
    public static EcmaValue PropertyIsEnumerable([This] EcmaValue thisArg, EcmaValue key) {
      EcmaPropertyKey pk = EcmaPropertyKey.FromValue(key);
      EcmaPropertyDescriptor pd = thisArg.ToRuntimeObject().GetOwnProperty(pk);
      return pd != null && pd.Enumerable.Value;
    }

    [IntrinsicMember]
    public static EcmaValue ToLocaleString([This] EcmaValue thisArg) {
      return thisArg.ToString();
    }

    [IntrinsicMember]
    public static EcmaValue ToString([This] EcmaValue thisArg) {
      return "[object " + thisArg.ToStringTag + "]";
    }

    [IntrinsicMember]
    public static EcmaValue ValueOf([This] EcmaValue thisArg) {
      return thisArg;
    }
  }
}