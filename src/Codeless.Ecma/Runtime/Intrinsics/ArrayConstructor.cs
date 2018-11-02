using System;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.ArrayConstructor)]
  internal static class ArrayConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.AlwaysConstruct)]
    public static EcmaValue Array([NewTarget] RuntimeObject constructor, params EcmaValue[] args) {
      if (args.Length == 1 && args[0].Type == EcmaValueType.Number) {
        uint length = args[0].ToUInt32();
        if (args[0] != length) {
          throw new EcmaRangeErrorException("");
        }
        return new EcmaArray(length, constructor);
      }
      return new EcmaArray(args, constructor);
    }

    [IntrinsicMember(WellKnownSymbol.Species, Getter = true)]
    public static EcmaValue Species([This] EcmaValue thisArg) {
      return thisArg;
    }

    [IntrinsicMember]
    public static bool IsArray(EcmaValue value) {
      return EcmaArray.IsArray(value);
    }
  }
}
