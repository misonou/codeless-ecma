using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.FunctionConstructor)]
  internal static class FunctionConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.AlwaysConstruct, ObjectType = typeof(ScriptFunction), Prototype = WellKnownObject.FunctionPrototype)]
    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue Function([This] EcmaValue thisValue, params EcmaValue[] args) {
      if (args.Length > 0) {
        throw new NotImplementedException();
      }
      return thisValue;
    }
  }
}
