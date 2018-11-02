using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.FunctionConstructor)]
  internal static class FunctionConstructor {
    [IntrinsicConstructor]
    public static EcmaValue Function([NewTarget] RuntimeObject constructor) {
      throw new NotImplementedException();
    }
  }
}