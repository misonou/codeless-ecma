using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.ObjectPrototype)]
  internal static class ObjectPrototype {
    [IntrinsicMember]
    public static EcmaValue HasOwnProperty([This] EcmaValue thisValue, EcmaValue key) {
      Guard.RequireObjectCoercible(thisValue);
      return thisValue.HasOwnProperty(EcmaPropertyKey.FromValue(key));
    }

    [IntrinsicMember]
    public static EcmaValue IsPrototypeOf([This] EcmaValue thisValue, EcmaValue obj) {
      Guard.RequireObjectCoercible(thisValue);
      if (obj.Type != EcmaValueType.Object) {
        return false;
      }
      RuntimeObject thisObj = thisValue.ToObject();
      for (RuntimeObject v = obj.ToObject(); v != null; v = v.GetPrototypeOf()) {
        if (v == thisObj) {
          return true;
        }
      }
      return false;
    }

    [IntrinsicMember]
    public static EcmaValue PropertyIsEnumerable([This] EcmaValue thisValue, EcmaValue key) {
      Guard.RequireObjectCoercible(thisValue);
      EcmaPropertyKey pk = EcmaPropertyKey.FromValue(key);
      EcmaPropertyDescriptor pd = thisValue.ToObject().GetOwnProperty(pk);
      return pd != null && pd.Enumerable;
    }

    [IntrinsicMember]
    public static EcmaValue ToLocaleString([This] EcmaValue thisValue) {
      Guard.RequireObjectCoercible(thisValue);
      return thisValue.Invoke("toString");
    }

    [IntrinsicMember]
    public static EcmaValue ToString([This] EcmaValue thisValue) {
      return "[object " + thisValue.ToStringTag + "]";
    }

    [IntrinsicMember]
    public static EcmaValue ValueOf([This] EcmaValue thisValue) {
      Guard.RequireObjectCoercible(thisValue);
      return thisValue.ToObject();
    }

    [IntrinsicMember("__proto__", EcmaPropertyAttributes.Configurable, Getter = true)]
    public static EcmaValue GetPrototypeOf([This] EcmaValue thisValue) {
      return EcmaObject.GetPrototypeOf(thisValue);
    }

    [IntrinsicMember("__proto__", EcmaPropertyAttributes.Configurable, Setter = true)]
    public static EcmaValue SetPrototypeOf([This] EcmaValue thisValue, EcmaValue proto) {
      return EcmaObject.SetPrototypeOf(thisValue, proto);
    }

    [IntrinsicMember("__defineGetter__")]
    public static EcmaValue DefineGetter([This] EcmaValue thisValue, EcmaValue key, EcmaValue getter) {
      Guard.RequireObjectCoercible(thisValue);
      return thisValue.ToObject().DefineOwnProperty(EcmaPropertyKey.FromValue(key),
         new EcmaPropertyDescriptor { Get = getter });
    }

    [IntrinsicMember("__defineSetter__")]
    public static EcmaValue DefineSetter([This] EcmaValue thisValue, EcmaValue key, EcmaValue setter) {
      Guard.RequireObjectCoercible(thisValue);
      return thisValue.ToObject().DefineOwnProperty(EcmaPropertyKey.FromValue(key),
         new EcmaPropertyDescriptor { Set = setter });
    }

    [IntrinsicMember("__lookupGetter__")]
    public static EcmaValue LookupGetter([This] EcmaValue thisValue, EcmaValue key) {
      Guard.RequireObjectCoercible(thisValue);
      EcmaPropertyDescriptor descriptor = thisValue.ToObject().GetOwnProperty(EcmaPropertyKey.FromValue(key));
      return descriptor.IsAccessorDescriptor ? descriptor.Get : default;
    }

    [IntrinsicMember("__lookupSetter__")]
    public static EcmaValue LookupSetter([This] EcmaValue thisValue, EcmaValue key) {
      Guard.RequireObjectCoercible(thisValue);
      EcmaPropertyDescriptor descriptor = thisValue.ToObject().GetOwnProperty(EcmaPropertyKey.FromValue(key));
      return descriptor.IsAccessorDescriptor ? descriptor.Set : default;
    }
  }
}
