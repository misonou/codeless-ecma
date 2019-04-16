using System;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.WeakMapConstructor)]
  internal static class WeakMapConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.DenyCall, ObjectType = typeof(EcmaWeakMap))]
    public static EcmaValue WeakMap([This] EcmaValue thisValue, EcmaValue iterable) {
      if (!iterable.IsNullOrUndefined) {
        EcmaValue adder = thisValue["set"];
        Guard.ArgumentIsCallable(adder);
        using (EcmaIteratorEnumerator iterator = iterable.ForOf()) {
          foreach (EcmaValue value in iterator) {
            Guard.ArgumentIsObject(value);
            adder.Call(thisValue, value[0], value[1]);
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
