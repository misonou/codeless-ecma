using System;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.MapConstructor)]
  internal static class MapConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.DenyCall, ObjectType = typeof(EcmaMap))]
    public static EcmaValue Map([This] EcmaValue thisValue, EcmaValue iterable) {
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
