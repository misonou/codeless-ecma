using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.ThrowTypeError)]
  internal static class ThrowTypeError {
    static ThrowTypeError() {
      RuntimeRealm.Initialized += (object sender, EventArgs e) => {
        RuntimeObject throwTypeError = ((RuntimeRealm)sender).GetRuntimeObject(WellKnownObject.ThrowTypeError);
        throwTypeError.SetIntegrityLevel(RuntimeObjectIntegrityLevel.Frozen);
      };
    }

    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.DenyConstruct, Name = "", Global = false)]
    public static void ThrowTypeErrorFunction() {
      throw new EcmaTypeErrorException(InternalString.Error.StrictMode);
    }
  }
}
