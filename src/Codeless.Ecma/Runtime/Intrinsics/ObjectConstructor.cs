using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.ObjectConstructor)]
  internal static class ObjectConstructor {
    [IntrinsicConstructor]
    public static EcmaValue Object([NewTarget] RuntimeObject constructor, [This] EcmaValue thisValue, EcmaValue value) {
      if (constructor != null) {
        return thisValue;
      }
      if (value.IsNullOrUndefined) {
        return new EcmaObject();
      }
      return value.ToObject();
    }
  }
}
