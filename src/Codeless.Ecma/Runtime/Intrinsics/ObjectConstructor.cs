using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.ObjectConstructor)]
  internal static class ObjectConstructor {
    [IntrinsicConstructor]
    public static EcmaValue Object([NewTarget] RuntimeObject constructor, EcmaValue value) {
      if (constructor != null || constructor != RuntimeRealm.GetWellKnownObject(WellKnownObject.ObjectConstructor)) {
        return new RuntimeObject(WellKnownObject.ObjectPrototype, constructor);
      }
      if (value.IsNullOrUndefined) {
        return new EcmaObject();
      }
      return value.ToRuntimeObject();
    }
  }
}