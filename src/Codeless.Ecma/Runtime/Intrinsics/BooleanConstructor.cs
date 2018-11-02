using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.BooleanConstructor)]
  internal static class BooleanConstructor {
    [IntrinsicConstructor]
    public static EcmaValue Boolean([NewTarget] RuntimeObject constructor, EcmaValue value) {
      if (constructor == null) {
        return value.ToBoolean();
      }
      return new IntrinsicObject(value.ToBoolean(), WellKnownObject.BooleanPrototype, constructor);
    }
  }
}