using System;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.SetConstructor)]
  internal static class SetConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.DenyCall, ObjectType = typeof(EcmaSet))]
    public static EcmaValue Set([This] EcmaValue thisValue, EcmaValue iterable) {
      if (!iterable.IsNullOrUndefined) {
        EcmaValue adder = thisValue["add"];
        Guard.ArgumentIsCallable(adder);
        using (EcmaIteratorEnumerator iterator = iterable.ForOf()) {
          foreach (EcmaValue value in iterator) {
            adder.Call(thisValue, value);
          }
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
