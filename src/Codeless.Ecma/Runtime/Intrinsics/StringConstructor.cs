using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.StringConstructor)]
  internal static class StringConstructor {
    [IntrinsicConstructor]
    public static EcmaValue String([NewTarget] RuntimeObject constructor, EcmaValue value) {
      if (constructor == null) {
        return value.ToString();
      }
      return new IntrinsicObject(value.ToString(), WellKnownObject.StringPrototype, constructor);
    }
  }
}