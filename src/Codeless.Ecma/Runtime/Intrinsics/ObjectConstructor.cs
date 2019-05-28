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
      if (constructor != null && !constructor.IsWellknownObject(WellKnownObject.ObjectConstructor)) {
        return RuntimeObject.CreateFromConstructor<EcmaObject>(WellKnownObject.ObjectPrototype, constructor);
      }
      switch (value.Type) {
        case EcmaValueType.Boolean:
          return new PrimitiveObject(value, WellKnownObject.BooleanPrototype);
        case EcmaValueType.Number:
          return new PrimitiveObject(value, WellKnownObject.NumberPrototype);
        case EcmaValueType.String:
          return new PrimitiveObject(value, WellKnownObject.StringPrototype);
        case EcmaValueType.Symbol:
          return new Symbol(value.GetUnderlyingObject<Symbol>().Description);
        case EcmaValueType.Object:
          return value;
      }
      return new EcmaObject();
    }
  }
}
