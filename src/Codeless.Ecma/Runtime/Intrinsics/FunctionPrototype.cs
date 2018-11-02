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
      // check arguments as array
      return fn.Call(thisValue, args);
    }

    [IntrinsicMember]
    public static EcmaValue Bind([This] RuntimeFunction fn, EcmaValue thisArg, params EcmaValue[] args) {
      return new BoundRuntimeFunction(fn, thisArg, args);
    }

    [IntrinsicMember]
    public static EcmaValue Call([This] RuntimeFunction fn, EcmaValue thisValue, params EcmaValue[] args) {
      return fn.Call(thisValue, args);
    }
  }
}