using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma {
  public static class Operator {
    public static EcmaValue BitwiseAnd(this EcmaValue x, EcmaValue y) {
      switch (EcmaValue.GetNumberCoercion(x, y)) {
        case EcmaNumberType.BigInt:
          return BigIntHelper.BitwiseAnd(x, y);
        case EcmaNumberType.BigInt64:
          return BigIntHelper.BitwiseAnd64(x, y);
      }
      return (+x).ToInt32() & (+y).ToInt32();
    }

    public static EcmaValue BitwiseOr(this EcmaValue x, EcmaValue y) {
      switch (EcmaValue.GetNumberCoercion(x, y)) {
        case EcmaNumberType.BigInt:
          return BigIntHelper.BitwiseOr(x, y);
        case EcmaNumberType.BigInt64:
          return BigIntHelper.BitwiseOr64(x, y);
      }
      return (+x).ToInt32() | (+y).ToInt32();
    }

    public static EcmaValue LeftShift(this EcmaValue x, EcmaValue y) {
      switch (EcmaValue.GetNumberCoercion(x, y)) {
        case EcmaNumberType.BigInt:
          return BigIntHelper.LeftShift(x, y);
        case EcmaNumberType.BigInt64:
          return BigIntHelper.LeftShift64(x, y);
      }
      return (+x).ToInt32() << (+y).ToInt32();
    }

    public static EcmaValue RightShift(this EcmaValue x, EcmaValue y) {
      switch (EcmaValue.GetNumberCoercion(x, y)) {
        case EcmaNumberType.BigInt:
          return BigIntHelper.RightShift(x, y);
        case EcmaNumberType.BigInt64:
          return BigIntHelper.RightShift64(x, y);
      }
      return (+x).ToInt32() >> (+y).ToInt32();
    }

    public static EcmaValue LogicalRightShift(this EcmaValue x, EcmaValue y) {
      return (+x).ToUInt32() >> (+y).ToInt32();
    }

    public static EcmaValue Pow(this EcmaValue x, EcmaValue y) {
      switch (EcmaValue.GetNumberCoercion(x, y)) {
        case EcmaNumberType.BigInt:
          return BigIntHelper.Pow(x, y);
        case EcmaNumberType.BigInt64:
          return BigIntHelper.Pow64(x, y);
      }
      return EcmaMath.Pow(x, y);
    }

    [EcmaSpecification("InstanceofOperator", EcmaSpecificationKind.RuntimeSemantics)]
    public static bool InstanceOf(this EcmaValue thisValue, EcmaValue constructor) {
      if (constructor.Type != EcmaValueType.Object) {
        throw new EcmaTypeErrorException(InternalString.Error.NotFunction);
      }
      RuntimeObject obj = constructor.ToObject();
      RuntimeObject instOfHandler = obj.GetMethod(Symbol.HasInstance);
      if (instOfHandler != null) {
        return instOfHandler.Call(constructor, thisValue).ToBoolean();
      }
      if (!constructor.IsCallable) {
        throw new EcmaTypeErrorException(InternalString.Error.NotFunction);
      }
      return obj.HasInstance(thisValue.ToObject());
    }

    [EcmaSpecification("Relational Operators `in`", EcmaSpecificationKind.RuntimeSemantics)]
    public static bool In(this EcmaValue thisValue, EcmaValue other) {
      if (other.Type != EcmaValueType.Object) {
        throw new EcmaTypeErrorException(InternalString.Error.NotObject);
      }
      return other.HasProperty(EcmaPropertyKey.FromValue(thisValue));
    }
  }
}
