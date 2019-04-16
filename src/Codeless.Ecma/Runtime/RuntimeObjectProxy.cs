using Codeless.Ecma.Runtime.Intrinsics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime {
  [IntrinsicObject(WellKnownObject.Proxy)]
  public class RuntimeObjectProxy : RuntimeObject {
    private RuntimeObject handler;
    private RuntimeObject target;

    public RuntimeObjectProxy()
      : base(WellKnownObject.ObjectPrototype) { }

    [EcmaSpecification("ProxyCreate", EcmaSpecificationKind.AbstractOperations)]
    public RuntimeObjectProxy(RuntimeObject target, RuntimeObject handler)
       : base(WellKnownObject.ObjectPrototype) {
      if (target is RuntimeObjectProxy p1 && p1.target == null) {
        throw new EcmaTypeErrorException(InternalString.Error.TargetOrHandlerRevoked);
      }
      if (handler is RuntimeObjectProxy p2 && p2.target == null) {
        throw new EcmaTypeErrorException(InternalString.Error.TargetOrHandlerRevoked);
      }
      this.target = target;
      this.handler = handler;
    }

    public RuntimeObject ProxyTarget => target;

    public override bool IsExtensible => GetIsExtensible();

    public override IList<EcmaPropertyKey> OwnPropertyKeys => GetOwnPropertyKeys();

    [IntrinsicConstructor(NativeRuntimeFunctionConstraint.DenyCall, Name = "Proxy", ObjectType = typeof(RuntimeObjectProxy))]
    public static EcmaValue Create([This] EcmaValue thisValue, EcmaValue target, EcmaValue handler) {
      Guard.ArgumentIsObject(target, InternalString.Error.TargetOrHandlerNotObject);
      Guard.ArgumentIsObject(handler, InternalString.Error.TargetOrHandlerNotObject);
      if (target.ToObject() is RuntimeObjectProxy p1 && p1.target == null) {
        throw new EcmaTypeErrorException(InternalString.Error.TargetOrHandlerRevoked);
      }
      if (handler.ToObject() is RuntimeObjectProxy p2 && p2.target == null) {
        throw new EcmaTypeErrorException(InternalString.Error.TargetOrHandlerRevoked);
      }
      RuntimeObjectProxy inst = thisValue.GetUnderlyingObject<RuntimeObjectProxy>();
      inst.target = target.ToObject();
      inst.handler = handler.ToObject();
      return thisValue;
    }

    [IntrinsicMember]
    public static EcmaValue Revocable(RuntimeObject target, RuntimeObject handler) {
      RuntimeObjectProxy proxy = new RuntimeObjectProxy(target, handler);
      EcmaObject result = new EcmaObject();
      result.CreateDataProperty("proxy", proxy);
      result.CreateDataProperty("revoke", new RevokeFunction(proxy));
      return result;
    }

    #region Proxy overriden methods
    public override bool PreventExtensions() {
      ThrowIfProxyRevoked();
      RuntimeObject trap = handler.GetMethod(WellKnownPropertyName.PreventExtensions);
      if (trap == null) {
        return target.PreventExtensions();
      }
      if ((bool)trap.Call(handler, target)) {
        if (target.IsExtensible) {
          throw new EcmaTypeErrorException(InternalString.Error.InvalidTrapResult);
        }
        return true;
      }
      return false;
    }

    public override RuntimeObject GetPrototypeOf() {
      ThrowIfProxyRevoked();
      RuntimeObject trap = handler.GetMethod(WellKnownPropertyName.GetPrototypeOf);
      if (trap == null) {
        return target.GetPrototypeOf();
      }
      EcmaValue handlerProto = trap.Call(handler, target);
      if (handlerProto.Type != EcmaValueType.Object && handlerProto.Type != EcmaValueType.Null) {
        throw new EcmaTypeErrorException(InternalString.Error.InvalidTrapResult);
      }
      if (target.IsExtensible) {
        return handlerProto.ToObject();
      }
      if (!handlerProto.Equals(target.GetPrototypeOf())) {
        throw new EcmaTypeErrorException(InternalString.Error.InvalidTrapResult);
      }
      return handlerProto.ToObject();
    }

    public override bool SetPrototypeOf(RuntimeObject proto) {
      ThrowIfProxyRevoked();
      RuntimeObject trap = handler.GetMethod(WellKnownPropertyName.SetPrototypeOf);
      if (trap == null) {
        return target.SetPrototypeOf(proto);
      }
      if (!(bool)trap.Call(handler, target, proto)) {
        return false;
      }
      if (target.IsExtensible) {
        return true;
      }
      if (!proto.Equals(target.GetPrototypeOf())) {
        throw new EcmaTypeErrorException(InternalString.Error.InvalidTrapResult);
      }
      return true;
    }

    public override EcmaPropertyDescriptor GetOwnProperty(EcmaPropertyKey propertyKey) {
      ThrowIfProxyRevoked();
      RuntimeObject method = handler.GetMethod(WellKnownPropertyName.GetOwnPropertyDescriptor);
      EcmaPropertyDescriptor targetDesc = target.GetOwnProperty(propertyKey);
      if (method != null) {
        EcmaValue result = method.Call(handler, target, propertyKey.ToValue());
        if (result.Type == EcmaValueType.Undefined) {
          if (targetDesc != null && (!targetDesc.Configurable.Value || !this.IsExtensible)) {
            throw new EcmaTypeErrorException(InternalString.Error.InvalidTrapResult);
          }
          return null;
        }
        EcmaPropertyDescriptor resultDesc = EcmaPropertyDescriptor.FromValue(result);
        if (!EcmaPropertyDescriptor.ValidateAndApplyPropertyDescriptor(ref resultDesc, targetDesc, !this.IsExtensible) ||
            resultDesc.Configurable == false && (targetDesc == null || targetDesc.Configurable == true)) {
          throw new EcmaTypeErrorException(InternalString.Error.InvalidTrapResult);
        }
        return resultDesc;
      }
      return targetDesc;
    }

    public override bool DefineOwnProperty(EcmaPropertyKey propertyKey, EcmaPropertyDescriptor descriptor) {
      ThrowIfProxyRevoked();
      RuntimeObject trap = handler.GetMethod(WellKnownPropertyName.DefineProperty);
      if (trap == null) {
        return target.DefineOwnProperty(propertyKey, descriptor);
      }
      if (!(bool)trap.Call(target, propertyKey.ToValue(), descriptor.ToValue())) {
        return false;
      }
      bool settingUnconfigurable = descriptor.Configurable == false;
      EcmaPropertyDescriptor current = target.GetOwnProperty(propertyKey);
      if (current == null) {
        if (!target.IsExtensible || settingUnconfigurable) {
          throw new EcmaTypeErrorException(InternalString.Error.InvalidTrapResult);
        }
      } else {
        if (!EcmaPropertyDescriptor.ValidateAndApplyPropertyDescriptor(ref descriptor, current, !target.IsExtensible)) {
          throw new EcmaTypeErrorException(InternalString.Error.InvalidTrapResult);
        }
        if (settingUnconfigurable && current.Configurable.Value) {
          throw new EcmaTypeErrorException(InternalString.Error.InvalidTrapResult);
        }
      }
      return true;
    }

    public override bool HasProperty(EcmaPropertyKey propertyKey) {
      ThrowIfProxyRevoked();
      RuntimeObject trap = handler.GetMethod(WellKnownPropertyName.Has);
      if (trap == null) {
        return target.HasProperty(propertyKey);
      }
      bool result = (bool)trap.Call(target, propertyKey.ToValue());
      if (!result) {
        EcmaPropertyDescriptor descriptor = target.GetOwnProperty(propertyKey);
        if (descriptor != null && !descriptor.Configurable.Value) {
          throw new EcmaTypeErrorException(InternalString.Error.InvalidTrapResult);
        }
        if (!target.IsExtensible) {
          throw new EcmaTypeErrorException(InternalString.Error.InvalidTrapResult);
        }
      }
      return result;
    }

    public override EcmaValue Get(EcmaPropertyKey propertyKey, RuntimeObject receiver) {
      ThrowIfProxyRevoked();
      RuntimeObject trap = handler.GetMethod(WellKnownPropertyName.Get);
      if (trap == null) {
        return target.Get(propertyKey, receiver);
      }
      EcmaValue result = trap.Call(target, propertyKey.ToValue(), receiver);
      EcmaPropertyDescriptor descriptor = target.GetOwnProperty(propertyKey);
      if (descriptor != null && !descriptor.Configurable.Value) {
        if (descriptor.IsDataDescriptor && !descriptor.Writable.Value && !descriptor.Value.Equals(result, EcmaValueComparison.SameValue)) {
          throw new EcmaTypeErrorException(InternalString.Error.InvalidTrapResult);
        }
        if (descriptor.IsAccessorDescriptor && descriptor.Get == default) {
          throw new EcmaTypeErrorException(InternalString.Error.InvalidTrapResult);
        }
      }
      return result;
    }

    public override bool Set(EcmaPropertyKey propertyKey, EcmaValue value, RuntimeObject receiver) {
      ThrowIfProxyRevoked();
      RuntimeObject trap = handler.GetMethod(WellKnownPropertyName.Set);
      if (trap == null) {
        return target.Set(propertyKey, value, receiver);
      }
      if (!(bool)trap.Call(handler, target, propertyKey.ToValue(), value, receiver)) {
        return false;
      }
      EcmaPropertyDescriptor descriptor = target.GetOwnProperty(propertyKey);
      if (descriptor != null && !descriptor.Configurable.Value) {
        if (descriptor.IsDataDescriptor && !descriptor.Writable.Value && !descriptor.Value.Equals(value, EcmaValueComparison.SameValue)) {
          throw new EcmaTypeErrorException(InternalString.Error.InvalidTrapResult);
        }
        if (descriptor.IsAccessorDescriptor && descriptor.Set == default) {
          throw new EcmaTypeErrorException(InternalString.Error.InvalidTrapResult);
        }
      }
      return true;
    }

    public override bool Delete(EcmaPropertyKey propertyKey) {
      ThrowIfProxyRevoked();
      RuntimeObject trap = handler.GetMethod(WellKnownPropertyName.DeleteProperty);
      if (trap == null) {
        return target.Delete(propertyKey);
      }
      if (!(bool)trap.Call(handler, target, propertyKey.ToValue())) {
        return false;
      }
      EcmaPropertyDescriptor descriptor = target.GetOwnProperty(propertyKey);
      if (descriptor == null) {
        return true;
      }
      if (!descriptor.Configurable.Value) {
        throw new EcmaTypeErrorException(InternalString.Error.InvalidTrapResult);
      }
      return true;
    }

    public override EcmaValue Call(EcmaValue thisValue, params EcmaValue[] arguments) {
      ThrowIfProxyRevoked();
      RuntimeObject trap = handler.GetMethod(WellKnownPropertyName.Apply);
      if (trap == null) {
        return target.Call(thisValue, arguments);
      }
      return trap.Call(handler, target, thisValue, new EcmaArray(arguments));
    }

    public override EcmaValue Construct(RuntimeObject newTarget, params EcmaValue[] arguments) {
      ThrowIfProxyRevoked();
      RuntimeObject trap = handler.GetMethod(WellKnownPropertyName.Construct);
      if (trap == null) {
        return target.Construct(newTarget, arguments);
      }
      EcmaValue returnValue = trap.Call(handler, target, new EcmaArray(arguments), newTarget);
      if (returnValue.Type != EcmaValueType.Object) {
        throw new EcmaTypeErrorException(InternalString.Error.NotObject);
      }
      return returnValue;
    }

    private bool GetIsExtensible() {
      ThrowIfProxyRevoked();
      RuntimeObject trap = handler.GetMethod(WellKnownPropertyName.IsExtensible);
      bool isExtensible = target.IsExtensible;
      if (trap == null) {
        return isExtensible;
      }
      if ((bool)trap.Call(handler, target) != isExtensible) {
        throw new EcmaTypeErrorException(InternalString.Error.InvalidTrapResult);
      }
      return isExtensible;
    }

    private IList<EcmaPropertyKey> GetOwnPropertyKeys() {
      ThrowIfProxyRevoked();
      RuntimeObject trap = handler.GetMethod(WellKnownPropertyName.OwnKeys);
      if (trap == null) {
        return target.OwnPropertyKeys;
      }
      EcmaValue resultArray = trap.Call(handler, target);
      Guard.ArgumentIsObject(resultArray);
      List<EcmaPropertyKey> list = new List<EcmaPropertyKey>();
      long len = resultArray["length"].ToLength();
      for (long i = 0; i < len; i++) {
        EcmaValue value = resultArray[i];
        if (!EcmaPropertyKey.IsPropertyKey(value)) {
          throw new EcmaTypeErrorException(InternalString.Error.InvalidTrapResult);
        }
        EcmaPropertyKey key = EcmaPropertyKey.FromValue(value);
        if (list.Contains(key)) {
          throw new EcmaTypeErrorException(InternalString.Error.InvalidTrapResult);
        }
        list.Add(key);
      }

      List<EcmaPropertyKey> targetKeys = new List<EcmaPropertyKey>(target.OwnPropertyKeys);
      List<EcmaPropertyKey> targetConfigurableKeys = new List<EcmaPropertyKey>();
      List<EcmaPropertyKey> targetNonConfigurableKeys = new List<EcmaPropertyKey>();
      foreach (EcmaPropertyKey key in targetKeys) {
        EcmaPropertyDescriptor descriptor = target.GetOwnProperty(key);
        (descriptor.Configurable != false ? targetConfigurableKeys : targetNonConfigurableKeys).Add(key);
      }
      if (target.IsExtensible && targetNonConfigurableKeys.Count == 0) {
        return list;
      }
      if (!targetNonConfigurableKeys.TrueForAll(list.Contains)) {
        throw new EcmaTypeErrorException(InternalString.Error.InvalidTrapResult);
      }
      if (target.IsExtensible) {
        return list;
      }
      if (!targetConfigurableKeys.TrueForAll(list.Contains)) {
        throw new EcmaTypeErrorException(InternalString.Error.InvalidTrapResult);
      }
      if (!list.TrueForAll(targetKeys.Contains)) {
        throw new EcmaTypeErrorException(InternalString.Error.InvalidTrapResult);
      }
      return list;
    }

    private void ThrowIfProxyRevoked() {
      if (target == null) {
        throw new EcmaTypeErrorException(InternalString.Error.ProxyRevoked);
      }
    }
    #endregion

    #region Helper class
    internal class RevokeFunction : RuntimeFunction {
      private RuntimeObjectProxy proxy;

      public RevokeFunction(RuntimeObjectProxy proxy) {
        this.proxy = proxy;
      }

      public override EcmaValue Call(EcmaValue thisValue, params EcmaValue[] arguments) {
        if (proxy != null) {
          proxy.target = null;
          proxy.handler = null;
          proxy = null;
        }
        return default;
      }
    }
    #endregion
  }
}
