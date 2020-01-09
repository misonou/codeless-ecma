using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.PromiseConstructor)]
  internal static class PromiseConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.DenyCall, ObjectType = typeof(Promise), Prototype = WellKnownObject.PromisePrototype)]
    public static EcmaValue Promise([This] EcmaValue thisValue, EcmaValue executor) {
      Promise promise = thisValue.GetUnderlyingObject<Promise>();
      if (!executor.IsCallable) {
        throw new EcmaTypeErrorException(InternalString.Error.NotFunction);
      }
      promise.InitWithExecutor(executor.ToObject());
      return thisValue;
    }

    [IntrinsicMember(WellKnownSymbol.Species, Getter = true)]
    public static EcmaValue Species([This] EcmaValue thisValue) {
      return thisValue;
    }

    [IntrinsicMember]
    [EcmaSpecification("PromiseResolve", EcmaSpecificationKind.AbstractOperations)]
    public static EcmaValue Resolve([This] EcmaValue thisValue, EcmaValue value) {
      Guard.ArgumentIsObject(thisValue);
      if (thisValue.Equals(value[WellKnownProperty.Constructor], EcmaValueComparison.SameValue)) {
        return value;
      }
      PromiseCapability capability = PromiseCapability.CreateFromConstructor(thisValue.ToObject());
      capability.Resolve(value);
      return capability.Promise;
    }

    [IntrinsicMember]
    public static EcmaValue Reject([This] EcmaValue thisValue, EcmaValue reason) {
      Guard.ArgumentIsObject(thisValue);
      PromiseCapability capability = PromiseCapability.CreateFromConstructor(thisValue.ToObject());
      capability.Reject(reason);
      return capability.Promise;
    }

    [IntrinsicMember]
    [EcmaSpecification("PerformPromiseAll", EcmaSpecificationKind.RuntimeSemantics)]
    public static EcmaValue All([This] EcmaValue thisValue, EcmaValue iterable) {
      Guard.ArgumentIsObject(thisValue);
      PromiseCapability capability = PromiseCapability.CreateFromConstructor(thisValue.ToObject());
      PromiseAggregator aggregator = new AllFulfilledAggregator(capability);
      try {
        using (EcmaIteratorEnumerator iterator = iterable.ForOf()) {
          EcmaValue resolve = thisValue[WellKnownProperty.Resolve];
          while (MoveNextSafe(iterator)) {
            EcmaValue thenable = resolve.Call(thisValue, iterator.Current);
            thenable.Invoke(WellKnownProperty.Then, (PromiseResolver)aggregator.CreateHandler().ResolveHandler, capability.RejectCallback);
          }
        }
        aggregator.ResolveIfConditionMet();
      } catch (Exception ex) {
        capability.Reject(EcmaValueUtility.GetValueFromException(ex));
      }
      return capability.Promise;
    }

    [IntrinsicMember]
    [EcmaSpecification("PerformPromiseAllSettled", EcmaSpecificationKind.RuntimeSemantics)]
    public static EcmaValue AllSettled([This] EcmaValue thisValue, EcmaValue iterable) {
      Guard.ArgumentIsObject(thisValue);
      PromiseCapability capability = PromiseCapability.CreateFromConstructor(thisValue.ToObject());
      PromiseAggregator aggregator = new AllSettledAggregator(capability);
      try {
        using (EcmaIteratorEnumerator iterator = iterable.ForOf()) {
          EcmaValue resolve = thisValue[WellKnownProperty.Resolve];
          while (MoveNextSafe(iterator)) {
            PromiseAggregateHandler handler = aggregator.CreateHandler();
            EcmaValue thenable = resolve.Call(thisValue, iterator.Current);
            thenable.Invoke(WellKnownProperty.Then, (PromiseResolver)handler.ResolveHandler, (PromiseResolver)handler.RejectHandler);
          }
        }
        aggregator.ResolveIfConditionMet();
      } catch (Exception ex) {
        capability.Reject(EcmaValueUtility.GetValueFromException(ex));
      }
      return capability.Promise;
    }

    [IntrinsicMember]
    [EcmaSpecification("PerformPromiseRace", EcmaSpecificationKind.RuntimeSemantics)]
    public static EcmaValue Race([This] EcmaValue thisValue, EcmaValue iterable) {
      Guard.ArgumentIsObject(thisValue);
      PromiseCapability capability = PromiseCapability.CreateFromConstructor(thisValue.ToObject());
      try {
        using (EcmaIteratorEnumerator iterator = iterable.ForOf()) {
          EcmaValue resolve = thisValue[WellKnownProperty.Resolve];
          while (MoveNextSafe(iterator)) {
            EcmaValue thenable = resolve.Call(thisValue, iterator.Current);
            thenable.Invoke(WellKnownProperty.Then, capability.ResolveCallback, capability.RejectCallback);
          }
        }
      } catch (Exception ex) {
        capability.Reject(EcmaValueUtility.GetValueFromException(ex));
      }
      return capability.Promise;
    }

    private static bool MoveNextSafe(EcmaIteratorEnumerator iterator) {
      try {
        return iterator.MoveNext();
      } catch {
        // do not explicitly close iterator when abrupt completion from getting next value
        // mark iterator done when exception throw from iterator.MoveNext()
        iterator.Done();
        throw;
      }
    }

    private class AllFulfilledAggregator : PromiseAggregator {
      public AllFulfilledAggregator(PromiseCapability capability)
        : base(capability) { }

      protected override PromiseState GetState(out EcmaValue value) {
        if (this.Count == 0 || this.All(v => v.State == PromiseState.Fulfilled)) {
          value = new EcmaArray(this.Select(v => v.Value).ToList());
          return PromiseState.Fulfilled;
        }
        return base.GetState(out value);
      }
    }

    private class AllSettledAggregator : PromiseAggregator {
      public AllSettledAggregator(PromiseCapability capability)
        : base(capability) { }

      protected override PromiseState GetState(out EcmaValue value) {
        if (this.Count == 0 || this.All(v => v.State != PromiseState.Pending)) {
          RuntimeObject objectProto = RuntimeRealm.Current.GetRuntimeObject(WellKnownObject.ObjectPrototype);
          List<EcmaValue> values = new List<EcmaValue>();
          foreach (PromiseAggregateHandler handler in this) {
            RuntimeObject obj = RuntimeObject.Create(objectProto);
            if (handler.State == PromiseState.Fulfilled) {
              obj.CreateDataPropertyOrThrow(WellKnownProperty.Status, "fulfilled");
              obj.CreateDataPropertyOrThrow(WellKnownProperty.Value, handler.Value);
            } else {
              obj.CreateDataPropertyOrThrow(WellKnownProperty.Status, "rejected");
              obj.CreateDataPropertyOrThrow(WellKnownProperty.Reason, handler.Value);
            }
            values.Add(obj);
          }
          value = new EcmaArray(values);
          return PromiseState.Fulfilled;
        }
        return base.GetState(out value);
      }
    }
  }
}
