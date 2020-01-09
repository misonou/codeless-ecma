using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.BooleanConstructor)]
  internal static class BooleanConstructor {
    [IntrinsicConstructor(ObjectType = typeof(PrimitiveObject), Prototype = WellKnownObject.BooleanPrototype)]
    public static EcmaValue Boolean([NewTarget] RuntimeObject constructor, [This] EcmaValue thisValue, EcmaValue value) {
      if (constructor == null) {
        return value.ToBoolean();
      }
      ((PrimitiveObject)thisValue.ToObject()).PrimitiveValue = value.ToBoolean();
      return thisValue;
    }
  }
}