﻿using Codeless.Ecma.InteropServices;
using Codeless.Ecma.Runtime;

namespace Codeless.Ecma {
  public static class Keywords {
    public static readonly EcmaValue Null = EcmaValue.Null;

    public static readonly IStaticModifier Static = ClassLiteralBuilder.Static;

    public static EcmaValue This {
      get {
        RuntimeFunctionInvocation invocation = RuntimeFunctionInvocation.Current;
        if (invocation != null && invocation.NewTarget != null && invocation.Super != null && !invocation.Super.ConstructorInvoked) {
          throw new EcmaReferenceErrorException(InternalString.Error.SuperConstructorNotCalled);
        }
        return invocation != null ? invocation.ThisValue : Global.GlobalThis;
      }
    }

    public static ArgumentsObject Arguments {
      get {
        RuntimeFunctionInvocation invocation = RuntimeFunctionInvocation.Current;
        return invocation != null ? invocation.Arguments : default;
      }
    }

    public static SuperAccessor Super {
      get {
        RuntimeFunctionInvocation invocation = RuntimeFunctionInvocation.Current;
        if (invocation == null || invocation.Super == null) {
          throw new EcmaSyntaxErrorException(InternalString.Error.UnexpectedSuper);
        }
        return invocation.Super;
      }
    }

    public static EcmaValue Void(params EcmaValue[] exp) {
      return EcmaValue.Undefined;
    }

    public static EcmaValue Return(params EcmaValue[] exp) {
      return exp == null || exp.Length == 0 ? EcmaValue.Undefined : exp[exp.Length - 1];
    }

    public static EcmaValue Throw(EcmaValue value) {
      throw EcmaException.FromValue(value);
    }

    [EcmaSpecification("typeof", EcmaSpecificationKind.RuntimeSemantics)]
    public static string TypeOf(EcmaValue value) {
      switch (value.Type) {
        case EcmaValueType.Undefined:
          return InternalString.TypeOf.Undefined;
        case EcmaValueType.Null:
          return InternalString.TypeOf.Object;
        case EcmaValueType.Boolean:
          return InternalString.TypeOf.Boolean;
        case EcmaValueType.Number:
          return InternalString.TypeOf.Number;
        case EcmaValueType.String:
          return InternalString.TypeOf.String;
        case EcmaValueType.Symbol:
          return InternalString.TypeOf.Symbol;
        case EcmaValueType.BigInt:
          return InternalString.TypeOf.BigInt;
      }
      return value.IsCallable ? InternalString.TypeOf.Function : InternalString.TypeOf.Object;
    }

    public static IExtendsModifier Extends(EcmaValue from) {
      return ClassLiteralBuilder.Extends(from);
    }
  }
}
