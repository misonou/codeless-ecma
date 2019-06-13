using System;
using System.Collections.Generic;
using System.Linq;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.PromiseConstructor)]
  internal static class PromiseConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.DenyCall, ObjectType = typeof(Promise))]
    public static EcmaValue Promise([This] EcmaValue thisValue, EcmaValue executor) {
      Promise promise = thisValue.GetUnderlyingObject<Promise>();
      if (!executor.IsCallable) {
        throw new EcmaTypeErrorException(InternalString.Error.NotFunction);
      }
      promise.InitWithCallback(executor.GetUnderlyingObject<RuntimeFunction>());
      return thisValue;
    }

    [IntrinsicMember(WellKnownSymbol.Species, Getter = true)]
    public static EcmaValue Species([This] EcmaValue thisValue) {
      return thisValue;
    }

    [IntrinsicMember]
    public static EcmaValue Resolve(EcmaValue value) {
      if (value.GetUnderlyingObject() is Promise promise) {
        return value[WellKnownProperty.Constructor].ToObject().IsWellknownObject(WellKnownObject.PromiseConstructor) ? value : new Promise(promise);
      }
      return new Promise(PromiseState.Fulfilled, value);
    }

    [IntrinsicMember]
    public static EcmaValue Reject(EcmaValue reason) {
      return new Promise(PromiseState.Rejected, reason);
    }

    [IntrinsicMember]
    public static EcmaValue All(EcmaValue iterable) {
      try {
        using (EcmaIteratorEnumerator iterator = iterable.ForOf()) {
          List<Promise> promises = new List<Promise>(iterator.Select(v => Resolve(v).GetUnderlyingObject<Promise>()));
          return Ecma.Promise.All(promises);
        }
      } catch (Exception ex) {
        return new Promise(ex);
      }
    }

    [IntrinsicMember]
    public static EcmaValue Race(EcmaValue iterable) {
      try {
        using (EcmaIteratorEnumerator iterator = iterable.ForOf()) {
          List<Promise> promises = new List<Promise>(iterator.Select(v => Resolve(v).GetUnderlyingObject<Promise>()));
          return Ecma.Promise.Any(promises);
        }
      } catch (Exception ex) {
        return new Promise(ex);
      }
    }
  }
}
