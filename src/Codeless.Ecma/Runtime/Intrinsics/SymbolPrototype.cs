using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.SymbolPrototype)]
  internal static class SymbolPrototype {
    [IntrinsicMember(WellKnownSymbol.ToStringTag)]
    public static string ToStringTag = InternalString.ObjectTag.Symbol;

    [IntrinsicMember(WellKnownSymbol.ToPrimitive)]
    public static EcmaValue ToPrimitive([This] EcmaValue thisValue) {
      return ValueOf(thisValue);
    }

    [IntrinsicMember]
    public static EcmaValue ToString([This] EcmaValue thisValue) {
      return ValueOf(thisValue).ToString();
    }

    [IntrinsicMember]
    public static EcmaValue ValueOf([This] EcmaValue thisValue) {
      return thisValue.GetIntrinsicPrimitiveValue(EcmaValueType.Symbol);
    }
  }
}
