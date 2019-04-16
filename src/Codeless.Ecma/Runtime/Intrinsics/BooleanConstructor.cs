using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.BooleanConstructor)]
  internal static class BooleanConstructor {
    [IntrinsicConstructor(ObjectType = typeof(IntrinsicObject))]
    public static EcmaValue Boolean([NewTarget] RuntimeObject constructor, [This] EcmaValue thisValue, EcmaValue value) {
      if (constructor == null) {
        return value.ToBoolean();
      }
      ((IntrinsicObject)thisValue.ToObject()).IntrinsicValue = value.ToBoolean();
      return thisValue;
    }
  }
}