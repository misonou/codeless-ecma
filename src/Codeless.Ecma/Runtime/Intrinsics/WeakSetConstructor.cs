using System;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.WeakSetConstructor)]
  internal static class WeakSetConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.DenyCall, ObjectType = typeof(EcmaWeakSet))]
    public static EcmaValue WeakSet([This] EcmaValue thisValue, EcmaValue iterable) {
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
