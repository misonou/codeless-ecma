using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.BigIntConstructor)]
  internal static class BigIntConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.DenyConstruct, Prototype = WellKnownObject.BigIntPrototype, Global = BigIntHelper.Supported)]
    public static EcmaValue BigInt(EcmaValue value) {
      value = value.ToPrimitive(EcmaPreferredPrimitiveType.Number);
      switch (EcmaValue.GetNumberCoercion(value)) {
        case EcmaNumberType.BigInt:
        case EcmaNumberType.BigInt64:
          return value;
        case EcmaNumberType.Double:
          return BigIntHelper.ToBigInt(value.ToDouble());
        case EcmaNumberType.Int64:
        case EcmaNumberType.Int32:
          return BigIntHelper.ToBigInt(value.ToInt64());
      }
      return BigIntHelper.ToBigInt(value);
    }

    [IntrinsicMember]
    public static EcmaValue AsIntN(EcmaValue bits, EcmaValue value) {
      long n = bits.ToIndex();
      value = value.ToPrimitive(EcmaPreferredPrimitiveType.Number);
      return BigIntHelper.ToBigIntN(value, (int)Math.Min(n, Int32.MaxValue));
    }

    [IntrinsicMember]
    public static EcmaValue AsUintN(EcmaValue bits, EcmaValue value) {
      long n = bits.ToIndex();
      value = value.ToPrimitive(EcmaPreferredPrimitiveType.Number);
      return BigIntHelper.ToBigUIntN(value, (int)Math.Min(n, Int32.MaxValue));
    }
  }
}
