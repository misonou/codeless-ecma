using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.AsyncFunction)]
  internal static class AsyncFunctionConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.AlwaysConstruct, Global = false, ObjectType = typeof(ScriptFunction), SuperClass = WellKnownObject.FunctionConstructor, Prototype = WellKnownObject.AsyncFunctionPrototype)]
    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue AsyncFunction([This] EcmaValue thisValue, params EcmaValue[] args) {
      throw new NotImplementedException();
    }
  }
}
