using System;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.SetConstructor)]
  internal static class SetConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.DenyCall, ObjectType = typeof(EcmaSet))]
    [IntrinsicMember(FunctionLength = 0)]
    public static EcmaValue Set([This] EcmaValue thisValue, EcmaValue iterable) {
      if (!iterable.IsNullOrUndefined) {
        EcmaValue adder = thisValue[WellKnownProperty.Add];
        Guard.ArgumentIsCallable(adder);
        foreach (EcmaValue value in iterable.ForOf()) {
          adder.Call(thisValue, value);
        }
      }
      return thisValue;
    }

    [IntrinsicMember(WellKnownSymbol.Species, Getter = true)]
    public static EcmaValue Species([This] EcmaValue thisValue) {
      return thisValue;
    }
  }
}
