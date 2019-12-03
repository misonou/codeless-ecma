using System;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.PromisePrototype)]
  internal static class PromisePrototype {
    [IntrinsicMember]
    public static EcmaValue Then([This] EcmaValue thisValue, EcmaValue onfulfill, EcmaValue onreject) {
      Promise promise = thisValue.GetUnderlyingObject<Promise>();
      PromiseCallback c1 = null;
      PromiseCallback c2 = null;
      if (!onfulfill.IsNullOrUndefined) {
        Guard.ArgumentIsCallable(onfulfill);
        c1 = v => onfulfill.Call(EcmaValue.Undefined, v);
      }
      if (!onreject.IsNullOrUndefined) {
        Guard.ArgumentIsCallable(onreject);
        c2 = v => onreject.Call(EcmaValue.Undefined, v);
      }
      return new Promise(promise, c1, c2);
    }

    [IntrinsicMember]
    public static EcmaValue Catch([This] EcmaValue thisValue, EcmaValue onreject) {
      Promise promise = thisValue.GetUnderlyingObject<Promise>();
      PromiseCallback callback = null;
      if (!onreject.IsNullOrUndefined) {
        Guard.ArgumentIsCallable(onreject);
        callback = v => onreject.Call(EcmaValue.Undefined, v);
      }
      return new Promise(promise, null, callback);
    }

    [IntrinsicMember]
    public static EcmaValue Finally([This] EcmaValue thisValue, EcmaValue onfinally) {
      Promise promise = thisValue.GetUnderlyingObject<Promise>();
      PromiseCallback callback = null;
      if (!onfinally.IsNullOrUndefined) {
        Guard.ArgumentIsCallable(onfinally);
        callback = v => onfinally.Call(EcmaValue.Undefined, v);
      }
      return new Promise(promise, null, null, callback);
    }
  }
}
