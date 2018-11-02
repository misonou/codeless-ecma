using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.BooleanPrototype)]
  internal static class BooleanPrototype {
    [IntrinsicMember]
    public static EcmaValue ToString([This] EcmaValue thisArg) {
      return ValueOf(thisArg).ToString();
    }

    [IntrinsicMember]
    [EcmaSpecification("thisBooleanValue", EcmaSpecificationKind.AbstractOperations)]
    public static EcmaValue ValueOf([This] EcmaValue thisArg) {
      return EcmaValueUtility.GetIntrinsicPrimitiveValue(thisArg, EcmaValueType.Boolean);
    }
  }
}