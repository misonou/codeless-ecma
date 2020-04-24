using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.BigIntPrototype)]
  internal static class BigIntPrototype {
    [IntrinsicMember(WellKnownSymbol.ToStringTag, EcmaPropertyAttributes.Configurable)]
    public const string ToStringTag = InternalString.ObjectTag.BigInt;

    [IntrinsicMember(Overridable = true)]
    public static EcmaValue ToLocaleString([This] EcmaValue thisValue) {
      EcmaValue bigInt = thisValue.GetIntrinsicPrimitiveValue(EcmaValueType.BigInt);
      return bigInt.ToString();
    }

    [IntrinsicMember(FunctionLength = 0)]
    public static EcmaValue ToString([This] EcmaValue thisValue, EcmaValue radix) {
      EcmaValue bigInt = thisValue.GetIntrinsicPrimitiveValue(EcmaValueType.BigInt);
      EcmaValue b = radix == default ? 10 : radix.ToInteger();
      return BigIntHelper.ToString(bigInt, b.ToInt32());
    }

    [IntrinsicMember]
    public static EcmaValue ValueOf([This] EcmaValue thisValue) {
      return thisValue.GetIntrinsicPrimitiveValue(EcmaValueType.BigInt);
    }
  }
}
