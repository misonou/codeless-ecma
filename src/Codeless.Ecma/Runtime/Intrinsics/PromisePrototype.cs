using System;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.PromisePrototype)]
  internal static class PromisePrototype {
    [IntrinsicMember(WellKnownSymbol.ToStringTag, EcmaPropertyAttributes.Configurable)]
    public const string ToStringTag = InternalString.ObjectTag.Promise;

    [IntrinsicMember]
    public static EcmaValue Then([This] EcmaValue thisValue, EcmaValue onfulfill, EcmaValue onreject) {
      Promise promise = thisValue.GetUnderlyingObject<Promise>();
      RuntimeObject constructor = RuntimeObject.GetSpeciesConstructor(promise, WellKnownObject.PromiseConstructor);
      PromiseCallback c1 = null;
      PromiseCallback c2 = null;
      if (onfulfill.IsCallable) {
        c1 = v => onfulfill.Call(EcmaValue.Undefined, v);
      }
      if (onreject.IsCallable) {
        c2 = v => onreject.Call(EcmaValue.Undefined, v);
      }
      PromiseCapability capability = PromiseCapability.CreateFromConstructor(constructor);
      capability.HandlePromise(promise, c1, c2);
      return capability.Promise;
    }

    [IntrinsicMember]
    public static EcmaValue Catch([This] EcmaValue thisValue, EcmaValue onreject) {
      return thisValue.Invoke(WellKnownProperty.Then, EcmaValue.Undefined, onreject);
    }

    [IntrinsicMember]
    public static EcmaValue Finally([This] EcmaValue thisValue, EcmaValue onfinally) {
      Guard.ArgumentIsObject(thisValue);
      EcmaValue constructor = RuntimeObject.GetSpeciesConstructor(thisValue.ToObject(), WellKnownObject.PromiseConstructor);
      if (onfinally.IsCallable) {
        FinallyCallbackHandler h = new FinallyCallbackHandler(constructor, onfinally);
        return thisValue.Invoke(WellKnownProperty.Then, (PromiseCallback)h.ResolveHandler, (PromiseCallback)h.RejectHandler);
      }
      return thisValue.Invoke(WellKnownProperty.Then, onfinally, onfinally);
    }

    private class FinallyCallbackHandler {
      private readonly EcmaValue constructor;
      private readonly EcmaValue onfinally;
      private EcmaValue originalValue;

      public FinallyCallbackHandler(EcmaValue constructor, EcmaValue onfinally) {
        this.constructor = constructor;
        this.onfinally = onfinally;
      }

      [IntrinsicMember(null)]
      [EcmaSpecification("Then Finally Functions", EcmaSpecificationKind.AbstractOperations)]
      public EcmaValue ResolveHandler(EcmaValue value) {
        originalValue = value;
        EcmaValue result = onfinally.Call();
        EcmaValue promise = PromiseConstructor.Resolve(constructor, result);
        return promise.Invoke("then", (PromiseCallback)ResolveOriginalValue);
      }

      [IntrinsicMember(null)]
      [EcmaSpecification("Catch Finally Functions", EcmaSpecificationKind.AbstractOperations)]
      public EcmaValue RejectHandler(EcmaValue value) {
        originalValue = value;
        EcmaValue result = onfinally.Call();
        EcmaValue promise = PromiseConstructor.Resolve(constructor, result);
        return promise.Invoke("then", (PromiseCallback)ThrowOriginalValue);
      }

      [IntrinsicMember(null)]
      private EcmaValue ResolveOriginalValue(EcmaValue _) {
        return originalValue;
      }

      [IntrinsicMember(null)]
      private EcmaValue ThrowOriginalValue(EcmaValue _) {
        throw EcmaException.FromValue(originalValue);
      }
    }
  }
}
