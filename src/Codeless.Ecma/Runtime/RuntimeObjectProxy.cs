using Codeless.Ecma.Diagnostics;
using Codeless.Ecma.Runtime.Intrinsics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime {
  public class RuntimeObjectProxy : RuntimeObject, IInspectorMetaProvider {
    private RuntimeObject handler;
    private RuntimeObject target;

    public RuntimeObjectProxy()
      : base(WellKnownObject.ObjectPrototype) { }

    public RuntimeObjectProxy(RuntimeObject target, RuntimeObject handler)
       : base(WellKnownObject.ObjectPrototype) {
      Init(target, handler);
    }

    public RuntimeObject ProxyTarget => ThrowIfProxyRevoked();

    public override bool IsExtensible => GetIsExtensible();

    public override bool IsCallable => ThrowIfProxyRevoked().IsCallable;

    public override bool IsConstructor => ThrowIfProxyRevoked().IsConstructor;

    protected override string ToStringTag {
      get {
        RuntimeObject target = ThrowIfProxyRevoked();
        if (EcmaArray.IsArray(target) || target.IsCallable) {
          return new EcmaValue(target).ToStringTag;
        }
        return base.ToStringTag;
      }
    }

    [EcmaSpecification("ProxyCreate", EcmaSpecificationKind.AbstractOperations)]
    internal void Init(RuntimeObject target, RuntimeObject handler) {
      if (target is RuntimeObjectProxy p1 && p1.target == null) {
        throw new EcmaTypeErrorException(InternalString.Error.TargetOrHandlerRevoked);
      }
      if (handler is RuntimeObjectProxy p2 && p2.target == null) {
        throw new EcmaTypeErrorException(InternalString.Error.TargetOrHandlerRevoked);
      }
      this.target = target;
      this.handler = handler;
      this.Realm = target.Realm;
    }

    #region Proxy overriden methods
    public override IEnumerable<EcmaPropertyKey> GetOwnPropertyKeys() {
      RuntimeObject target = ThrowIfProxyRevoked();
      RuntimeObject trap = handler.GetMethod(WellKnownProperty.OwnKeys);
      if (trap == null) {
        return target.GetOwnPropertyKeys();
      }
      EcmaValue resultArray = trap.Call(handler, target);
      Guard.ArgumentIsObject(resultArray);
      List<EcmaPropertyKey> list = new List<EcmaPropertyKey>();
      long len = resultArray[WellKnownProperty.Length].ToLength();
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

      List<EcmaPropertyKey> targetKeys = new List<EcmaPropertyKey>(target.GetOwnPropertyKeys());
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

    public override bool PreventExtensions() {
      RuntimeObject target = ThrowIfProxyRevoked();
      RuntimeObject trap = handler.GetMethod(WellKnownProperty.PreventExtensions);
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
      RuntimeObject target = ThrowIfProxyRevoked();
      RuntimeObject trap = handler.GetMethod(WellKnownProperty.GetPrototypeOf);
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
      RuntimeObject target = ThrowIfProxyRevoked();
      RuntimeObject trap = handler.GetMethod(WellKnownProperty.SetPrototypeOf);
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
      RuntimeObject target = ThrowIfProxyRevoked();
      RuntimeObject method = handler.GetMethod(WellKnownProperty.GetOwnPropertyDescriptor);
      EcmaPropertyDescriptor targetDesc = target.GetOwnProperty(propertyKey);
      if (method != null) {
        EcmaValue result = method.Call(handler, target, propertyKey.ToValue());
        if (result.Type == EcmaValueType.Undefined) {
          if (targetDesc != null && (!targetDesc.Configurable || !target.IsExtensible)) {
            throw new EcmaTypeErrorException(InternalString.Error.InvalidTrapResult);
          }
          return null;
        }
        EcmaPropertyDescriptor resultDesc = EcmaPropertyDescriptor.FromValue(result);
        resultDesc.CompleteDescriptor();
        if (!EcmaPropertyDescriptor.ValidateAndApplyPropertyDescriptor(ref resultDesc, targetDesc, !target.IsExtensible) ||
            resultDesc.Configurable == false && (targetDesc == null || targetDesc.Configurable == true)) {
          throw new EcmaTypeErrorException(InternalString.Error.InvalidTrapResult);
        }
        return resultDesc;
      }
      return targetDesc;
    }

    public override bool DefineOwnProperty(EcmaPropertyKey propertyKey, EcmaPropertyDescriptor descriptor) {
      RuntimeObject target = ThrowIfProxyRevoked();
      RuntimeObject trap = handler.GetMethod(WellKnownProperty.DefineProperty);
      if (trap == null) {
        return target.DefineOwnProperty(propertyKey, descriptor);
      }
      if (!(bool)trap.Call(handler, target, propertyKey.ToValue(), descriptor.ToValue())) {
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
        if (settingUnconfigurable && current.Configurable) {
          throw new EcmaTypeErrorException(InternalString.Error.InvalidTrapResult);
        }
      }
      return true;
    }

    public override bool HasProperty(EcmaPropertyKey propertyKey) {
      RuntimeObject target = ThrowIfProxyRevoked();
      RuntimeObject trap = handler.GetMethod(WellKnownProperty.Has);
      if (trap == null) {
        return target.HasProperty(propertyKey);
      }
      bool result = (bool)trap.Call(handler, target, propertyKey.ToValue());
      if (!result) {
        EcmaPropertyDescriptor descriptor = target.GetOwnProperty(propertyKey);
        if (descriptor != null && !descriptor.Configurable) {
          throw new EcmaTypeErrorException(InternalString.Error.InvalidTrapResult);
        }
        if (!target.IsExtensible) {
          throw new EcmaTypeErrorException(InternalString.Error.InvalidTrapResult);
        }
      }
      return result;
    }

    public override EcmaValue Get(EcmaPropertyKey propertyKey, RuntimeObject receiver) {
      RuntimeObject target = ThrowIfProxyRevoked();
      RuntimeObject trap = handler.GetMethod(WellKnownProperty.Get);
      if (trap == null) {
        return target.Get(propertyKey, receiver);
      }
      EcmaValue result = trap.Call(handler, target, propertyKey.ToValue(), receiver);
      EcmaPropertyDescriptor descriptor = target.GetOwnProperty(propertyKey);
      if (descriptor != null && !descriptor.Configurable) {
        if (descriptor.IsDataDescriptor && !descriptor.Writable && !descriptor.Value.Equals(result, EcmaValueComparison.SameValue)) {
          throw new EcmaTypeErrorException(InternalString.Error.InvalidTrapResult);
        }
        if (descriptor.IsAccessorDescriptor && descriptor.Get == default) {
          throw new EcmaTypeErrorException(InternalString.Error.InvalidTrapResult);
        }
      }
      return result;
    }

    public override bool Set(EcmaPropertyKey propertyKey, EcmaValue value, RuntimeObject receiver) {
      RuntimeObject target = ThrowIfProxyRevoked();
      RuntimeObject trap = handler.GetMethod(WellKnownProperty.Set);
      if (trap == null) {
        return target.Set(propertyKey, value, receiver);
      }
      if (!(bool)trap.Call(handler, target, propertyKey.ToValue(), value, receiver)) {
        return false;
      }
      EcmaPropertyDescriptor descriptor = target.GetOwnProperty(propertyKey);
      if (descriptor != null && !descriptor.Configurable) {
        if (descriptor.IsDataDescriptor && !descriptor.Writable && !descriptor.Value.Equals(value, EcmaValueComparison.SameValue)) {
          throw new EcmaTypeErrorException(InternalString.Error.InvalidTrapResult);
        }
        if (descriptor.IsAccessorDescriptor && descriptor.Set == default) {
          throw new EcmaTypeErrorException(InternalString.Error.InvalidTrapResult);
        }
      }
      return true;
    }

    public override bool Delete(EcmaPropertyKey propertyKey) {
      RuntimeObject target = ThrowIfProxyRevoked();
      RuntimeObject trap = handler.GetMethod(WellKnownProperty.DeleteProperty);
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
      if (!descriptor.Configurable) {
        throw new EcmaTypeErrorException(InternalString.Error.InvalidTrapResult);
      }
      return true;
    }

    public override EcmaValue Call(EcmaValue thisValue, params EcmaValue[] arguments) {
      RuntimeObject target = ThrowIfProxyRevoked();
      RuntimeObject trap = handler.GetMethod(WellKnownProperty.Apply);
      if (trap == null) {
        return target.Call(thisValue, arguments);
      }
      return trap.Call(handler, target, thisValue, new EcmaArray(arguments));
    }

    public override EcmaValue Construct(EcmaValue[] arguments, RuntimeObject newTarget) {
      RuntimeObject target = ThrowIfProxyRevoked();
      RuntimeObject trap = handler.GetMethod(WellKnownProperty.Construct);
      if (trap == null) {
        return target.Construct(arguments, newTarget);
      }
      EcmaValue returnValue = trap.Call(handler, target, new EcmaArray(arguments), newTarget);
      if (returnValue.Type != EcmaValueType.Object) {
        throw new EcmaTypeErrorException(InternalString.Error.NotObject);
      }
      return returnValue;
    }

    private bool GetIsExtensible() {
      RuntimeObject target = ThrowIfProxyRevoked();
      RuntimeObject trap = handler.GetMethod(WellKnownProperty.IsExtensible);
      bool isExtensible = target.IsExtensible;
      if (trap == null) {
        return isExtensible;
      }
      if ((bool)trap.Call(handler, target) != isExtensible) {
        throw new EcmaTypeErrorException(InternalString.Error.InvalidTrapResult);
      }
      return isExtensible;
    }

    private RuntimeObject ThrowIfProxyRevoked() {
      if (target == null) {
        throw new EcmaTypeErrorException(InternalString.Error.ProxyRevoked);
      }
      return target;
    }
    #endregion

    #region Interface
    void IInspectorMetaProvider.FillInInspectorMetaObject(InspectorMetaObject meta) {
      meta.EnumerableProperties.Add("[[Handler]]", handler);
      meta.EnumerableProperties.Add("[[Target]]", target);
      meta.EnumerableProperties.Add("[[IsRevoked]]", target == null);
    }
    #endregion

    #region Helper class
    internal class RevokeFunction : RuntimeFunction {
      private RuntimeObjectProxy proxy;

      public RevokeFunction(RuntimeObjectProxy proxy) {
        InitProperty(String.Empty, 0);
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
