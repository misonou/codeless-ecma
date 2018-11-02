using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.SymbolConstructor)]
  internal static class SymbolConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.DenyConstruct)]
    public static EcmaValue Symbol(EcmaValue description) {
      return new EcmaValue(new Symbol(description.ToString()));
    }
  }
}