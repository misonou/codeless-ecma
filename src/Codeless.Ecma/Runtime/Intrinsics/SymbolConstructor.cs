using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.SymbolConstructor)]
  internal static class SymbolConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.DenyConstruct)]
    [IntrinsicMember(FunctionLength = 0)]
    public static EcmaValue Symbol(EcmaValue description) {
      return new EcmaValue(new Symbol(description == default ? null : description.ToString(true)));
    }
  }
}