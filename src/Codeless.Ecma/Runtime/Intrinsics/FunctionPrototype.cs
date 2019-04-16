using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.FunctionPrototype)]
  internal static class FunctionPrototype {
    [IntrinsicMember]
    public static EcmaValue Apply([This] RuntimeFunction fn, EcmaValue thisValue, EcmaValue args) {
      return fn.Call(thisValue, EcmaValueUtility.CreateListFromArrayLike(args));
    }

    [IntrinsicMember]
    public static EcmaValue Bind([This] RuntimeFunction fn, EcmaValue thisValue, params EcmaValue[] args) {
      return new BoundRuntimeFunction(fn, thisValue, args);
    }

    [IntrinsicMember]
    public static EcmaValue Call([This] RuntimeFunction fn, EcmaValue thisValue, params EcmaValue[] args) {
      return fn.Call(thisValue, args);
    }
  }
}