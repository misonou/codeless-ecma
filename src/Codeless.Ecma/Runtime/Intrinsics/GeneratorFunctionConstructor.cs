using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.GeneratorFunction)]
  internal static class GeneratorFunctionConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.AlwaysConstruct, Global = false, ObjectType = typeof(ScriptFunction), SuperClass = WellKnownObject.FunctionConstructor, Prototype = WellKnownObject.Generator)]
    [IntrinsicMember(FunctionLength = 1)]
    public static EcmaValue GeneratorFunction([This] EcmaValue thisValue, params EcmaValue[] args) {
      throw new NotImplementedException();
    }
  }
}
