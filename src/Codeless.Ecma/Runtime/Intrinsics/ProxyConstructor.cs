using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codeless.Ecma.Runtime.Intrinsics {
  [IntrinsicObject(WellKnownObject.ProxyConstructor)]
  internal static class ProxyConstructor {
    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.DenyCall, ObjectType = typeof(RuntimeObjectProxy))]
    public static EcmaValue Proxy([This] EcmaValue thisValue, EcmaValue target, EcmaValue handler) {
      Guard.ArgumentIsObject(target, InternalString.Error.TargetOrHandlerNotObject);
      Guard.ArgumentIsObject(handler, InternalString.Error.TargetOrHandlerNotObject);
      RuntimeObjectProxy inst = thisValue.GetUnderlyingObject<RuntimeObjectProxy>();
      inst.Init(target.ToObject(), handler.ToObject());
      return thisValue;
    }

    [IntrinsicMember]
    public static EcmaValue Revocable(EcmaValue target, EcmaValue handler) {
      Guard.ArgumentIsObject(target, InternalString.Error.TargetOrHandlerNotObject);
      Guard.ArgumentIsObject(handler, InternalString.Error.TargetOrHandlerNotObject);
      RuntimeObjectProxy proxy = new RuntimeObjectProxy(target.ToObject(), handler.ToObject());
      EcmaObject result = new EcmaObject();
      result.CreateDataProperty("proxy", proxy);
      result.CreateDataProperty("revoke", new RuntimeObjectProxy.RevokeFunction(proxy));
      return result;
    }
  }
}
