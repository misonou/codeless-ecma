using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.NumberPrototype)]
  internal static class NumberPrototype {
    [IntrinsicMember]
    public static EcmaValue ToExponential() { throw new NotImplementedException(); }

    [IntrinsicMember]
    public static EcmaValue ToFixed() { throw new NotImplementedException(); }

    [IntrinsicMember]
    public static EcmaValue ToLocaleString() { throw new NotImplementedException(); }

    [IntrinsicMember]
    public static EcmaValue ToPrecision() { throw new NotImplementedException(); }

    [IntrinsicMember]
    public static EcmaValue ToString([This] EcmaValue thisArg) {
      return EcmaValueUtility.GetIntrinsicPrimitiveValue(thisArg, EcmaValueType.Number).ToString();
    }

    [IntrinsicMember]
    [EcmaSpecification("thisNumberValue", EcmaSpecificationKind.AbstractOperations)]
    public static EcmaValue ValueOf([This] EcmaValue thisArg) {
      return EcmaValueUtility.GetIntrinsicPrimitiveValue(thisArg, EcmaValueType.Number);
    }
  }
}