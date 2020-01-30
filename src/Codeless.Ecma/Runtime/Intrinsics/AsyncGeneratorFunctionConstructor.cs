using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.AsyncGeneratorFunction)]
  internal static class AsyncGeneratorFunctionConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.AlwaysConstruct, Global = false, ObjectType = typeof(ScriptFunction), SuperClass = WellKnownObject.FunctionConstructor, Prototype = WellKnownObject.AsyncGenerator)]
    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue AsyncGeneratorFunction([This] EcmaValue thisValue, params EcmaValue[] args) {
      throw new NotImplementedException();
    }
  }
}
