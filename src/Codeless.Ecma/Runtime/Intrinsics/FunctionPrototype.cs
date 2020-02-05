using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.FunctionPrototype)]
  internal static class FunctionPrototype {
    [IntrinsicMember("caller", EcmaPropertyAttributes.Configurable, Getter = true)]
    [IntrinsicMember("caller", EcmaPropertyAttributes.Configurable, Setter = true)]
    [IntrinsicMember("arguments", EcmaPropertyAttributes.Configurable, Getter = true)]
    [IntrinsicMember("arguments", EcmaPropertyAttributes.Configurable, Setter = true)]
    public static WellKnownObject ThrowTypeError = WellKnownObject.ThrowTypeError;

    [IntrinsicMember]
    public static EcmaValue Apply([This] EcmaValue thisValue, EcmaValue thisArg, EcmaValue args) {
      Guard.ArgumentIsCallable(thisValue);
      return thisValue.Call(thisArg, EcmaValueUtility.CreateListFromArrayLike(args));
    }

    [IntrinsicMember]
    public static EcmaValue Bind([This] EcmaValue thisValue, EcmaValue thisArg, params EcmaValue[] args) {
      RuntimeFunction fn = thisValue.GetUnderlyingObject<RuntimeFunction>();
      return new BoundRuntimeFunction(fn, thisArg, args);
    }

    [IntrinsicMember]
    public static EcmaValue Call([This] EcmaValue thisValue, EcmaValue thisArg, params EcmaValue[] args) {
      Guard.ArgumentIsCallable(thisValue);
      return thisValue.Call(thisArg, args);
    }

    [IntrinsicMember]
    public static EcmaValue ToString([This] EcmaValue thisValue) {
      Guard.ArgumentIsCallable(thisValue);
      RuntimeFunction fn = thisValue.GetUnderlyingObject<RuntimeFunction>();
      return fn.Source;
    }
  }
}
