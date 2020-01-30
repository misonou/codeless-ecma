using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.ObjectConstructor)]
  internal static class ObjectConstructor {
    [IntrinsicConstructor(Prototype = WellKnownObject.ObjectPrototype)]
    public static EcmaValue Object([NewTarget] RuntimeObject constructor, EcmaValue value) {
      if (constructor != null && !constructor.IsWellknownObject(WellKnownObject.ObjectConstructor)) {
        return RuntimeObject.CreateFromConstructor<EcmaObject>(constructor, 0);
      }
      switch (value.Type) {
        case EcmaValueType.Boolean:
          return new PrimitiveObject(value, WellKnownObject.BooleanPrototype);
        case EcmaValueType.Number:
          return new PrimitiveObject(value, WellKnownObject.NumberPrototype);
        case EcmaValueType.String:
          return new PrimitiveObject(value, WellKnownObject.StringPrototype);
        case EcmaValueType.Symbol:
          return new PrimitiveObject(value, WellKnownObject.SymbolPrototype);
        case EcmaValueType.BigInt:
          return new PrimitiveObject(value, WellKnownObject.BigIntPrototype);
        case EcmaValueType.Object:
          return value;
      }
      return new EcmaObject();
    }
  }
}
