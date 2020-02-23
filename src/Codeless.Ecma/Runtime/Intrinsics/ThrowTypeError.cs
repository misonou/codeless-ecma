using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.ThrowTypeError)]
  internal static class ThrowTypeError {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.DenyConstruct, Name = "", Global = false)]
    public static void ThrowTypeErrorFunction() {
      throw new EcmaTypeErrorException(InternalString.Error.StrictMode);
    }
  }
}
