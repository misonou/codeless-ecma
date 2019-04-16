using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.StringConstructor)]
  internal static class StringConstructor {
    [IntrinsicConstructor(ObjectType = typeof(IntrinsicObject))]
    public static EcmaValue String([NewTarget] RuntimeObject constructor, [This] EcmaValue thisValue, EcmaValue value) {
      if (constructor == null) {
        return value.ToString();
      }
      ((IntrinsicObject)thisValue.ToObject()).IntrinsicValue = value.ToString();
      return thisValue;
    }
  }
}