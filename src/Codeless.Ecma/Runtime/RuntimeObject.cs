using Codeless.Ecma.Diagnostics;
using Codeless.Ecma.Diagnostics.VisualStudio;
using Codeless.Ecma.Native;
using Codeless.Ecma.Runtime.Intrinsics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Codeless.Ecma.Runtime {
  public enum RuntimeObjectIntegrityLevel {
    Default,
    ExtensionPrevented,
    Sealed,
    Frozen
  }

  [DebuggerTypeProxy(typeof(RuntimeObjectDebuggerProxy))]
  [DebuggerDisplay("{DebuggerDisplay,nq}")]
  public class RuntimeObject : WeakKeyedItem, IEcmaValueBinder {
    private static readonly WellKnownPropertyName[] convertToPrimitiveMethodString = { WellKnownPropertyName.ToString, WellKnownPropertyName.ValueOf };
    private static readonly WellKnownPropertyName[] convertToPrimitiveMethodNumber = { WellKnownPropertyName.ValueOf, WellKnownPropertyName.ToString };

    private readonly RuntimeRealm realm = RuntimeRealm.Current;
    private Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor> dictionary;
    private EcmaPropertyKeyCollection keys;
    private RuntimeObject prototype;
    private RuntimeObjectIntegrityLevel integrityLevel;

    public RuntimeObject(WellKnownObject defaultProto) {
      ThrowIfRealmDisposed();
      this.prototype = RuntimeRealm.GetRuntimeObject(defaultProto);
    }

    public RuntimeObject(WellKnownObject defaultProto, RuntimeObject constructor) {
      ThrowIfRealmDisposed();
      this.prototype = GetPrototypeFromConstructor(constructor, defaultProto);
    }

    protected RuntimeObject(object target)
      : base(target) {
      ThrowIfRealmDisposed();
    }

    public virtual bool IsCallable => false;

    public virtual bool IsConstructor => false;

    public RuntimeObjectIntegrityLevel IntegrityLevel => integrityLevel;

    public RuntimeRealm Realm => realm;

    [EcmaSpecification("OrdinaryCreateFromConstructor", EcmaSpecificationKind.AbstractOperations)]
    public static T CreateFromConstructor<T>(WellKnownObject defaultProto, RuntimeObject constructor) where T : RuntimeObject, new() {
      T obj = new T();
      obj.prototype = GetPrototypeFromConstructor(constructor, defaultProto);
      return obj;
    }

    [EcmaSpecification("ObjectCreate", EcmaSpecificationKind.AbstractOperations)]
    public static RuntimeObject Create(RuntimeObject proto) {
      RuntimeObject obj = new EcmaObject();
      obj.prototype = proto;
      return obj;
    }

    [EcmaSpecification("GetPrototypeFromConstructor", EcmaSpecificationKind.AbstractOperations)]
    public static RuntimeObject GetPrototypeFromConstructor(RuntimeObject constructor, WellKnownObject defaultProto) {
      Guard.ArgumentNotNull(constructor, "constructor");
      if (!constructor.IsCallable) {
        throw new EcmaTypeErrorException(InternalString.Error.NotFunction);
      }
      EcmaValue proto = constructor.Get(WellKnownPropertyName.Prototype);
      if (proto.Type == EcmaValueType.Object) {
        return proto.ToObject();
      }
      return RuntimeRealm.GetRuntimeObject(defaultProto);
    }

    [EcmaSpecification("OrdinaryIsExtensible", EcmaSpecificationKind.InternalMethod)]
    public virtual bool IsExtensible {
      get { return integrityLevel == RuntimeObjectIntegrityLevel.Default; }
    }

    [EcmaSpecification("OrdinaryOwnPropertyKeys", EcmaSpecificationKind.InternalMethod)]
    public virtual IList<EcmaPropertyKey> OwnPropertyKeys {
      get {
        if (keys == null) {
          return new EcmaPropertyKey[0];
        }
        return keys.ToArray();
      }
    }

    [EcmaSpecification("OrdinaryGetPrototypeOf", EcmaSpecificationKind.InternalMethod)]
    public virtual RuntimeObject GetPrototypeOf() {
      return prototype;
    }

    [EcmaSpecification("OrdinarySetPrototypeOf", EcmaSpecificationKind.InternalMethod)]
    public virtual bool SetPrototypeOf(RuntimeObject proto) {
      EcmaValue v = new EcmaValue(proto);
      if (v.Type != EcmaValueType.Null && v.Type != EcmaValueType.Object) {
        throw new EcmaTypeErrorException(InternalString.Error.PrototypeMustBeObjectOrNull);
      }
      if (prototype == proto) {
        return true;
      }
      if (!this.IsExtensible) {
        throw new EcmaTypeErrorException(InternalString.Error.NotExtensible);
      }
      for (RuntimeObject p = proto; p != null; p = p.GetPrototypeOf()) {
        if (p == this) {
          return false;
        }
      }
      prototype = proto;
      return true;
    }

    [EcmaSpecification("OrdinaryPreventExtensions", EcmaSpecificationKind.InternalMethod)]
    public virtual bool PreventExtensions() {
      if (integrityLevel == RuntimeObjectIntegrityLevel.Default) {
        integrityLevel = RuntimeObjectIntegrityLevel.ExtensionPrevented;
      }
      return true;
    }

    [EcmaSpecification("OrdinaryGetOwnProperty", EcmaSpecificationKind.InternalMethod)]
    public virtual EcmaPropertyDescriptor GetOwnProperty(EcmaPropertyKey propertyKey) {
      EcmaPropertyDescriptor value;
      if (dictionary != null && dictionary.TryGetValue(propertyKey, out value)) {
        return value;
      }
      return null;
    }

    [EcmaSpecification("OrdinaryDefineOwnProperty", EcmaSpecificationKind.InternalMethod)]
    public virtual bool DefineOwnProperty(EcmaPropertyKey propertyKey, EcmaPropertyDescriptor descriptor) {
      EcmaPropertyDescriptor current = GetOwnProperty(propertyKey);
      if (EcmaPropertyDescriptor.ValidateAndApplyPropertyDescriptor(ref descriptor, current, !this.IsExtensible)) {
        DefineOwnPropertyNoChecked(propertyKey, descriptor);
        return true;
      }
      return false;
    }

    [EcmaSpecification("OrdinaryHasProperty ", EcmaSpecificationKind.InternalMethod)]
    public virtual bool HasProperty(EcmaPropertyKey propertyKey) {
      return GetProperty(propertyKey) != null;
    }

    [EcmaSpecification("OrdinaryGet", EcmaSpecificationKind.InternalMethod)]
    public virtual EcmaValue Get(EcmaPropertyKey propertyKey, RuntimeObject receiver) {
      EcmaPropertyDescriptor descriptor = GetProperty(propertyKey);
      if (descriptor == null) {
        return default;
      }
      if (descriptor.IsAccessorDescriptor) {
        return descriptor.Get ? descriptor.Get.Call(new EcmaValue(receiver ?? this)) : default;
      }
      return descriptor.Value;
    }

    [EcmaSpecification("OrdinarySet", EcmaSpecificationKind.InternalMethod)]
    public virtual bool Set(EcmaPropertyKey propertyKey, EcmaValue value, RuntimeObject receiver) {
      EcmaPropertyDescriptor descriptor = GetProperty(propertyKey);
      if (descriptor == null) {
        return this.CreateDataProperty(propertyKey, value);
      }
      if (descriptor.IsAccessorDescriptor) {
        EcmaValue thisValue = new EcmaValue(receiver ?? this);
        if (descriptor.Set != default && thisValue.Type == EcmaValueType.Object) {
          descriptor.Set.Call(thisValue, value);
          return true;
        }
        return false;
      }
      if (descriptor.Writable == false) {
        return false;
      }
      return this.CreateDataProperty(propertyKey, value);

      //EcmaPropertyDescriptor ownDesc = GetOwnProperty(propertyKey);
      //if (ownDesc == null) {
      //  object parent = GetPrototypeOf();
      //  if (parent != null) {
      //    return GetRuntimeData(parent).Set(propertyKey, value, receiver);
      //  }
      //  ownDesc = new EcmaPropertyDescriptor();
      //}
      //if (ownDesc.IsDataDescriptor) {
      //  if (!ownDesc.Writable) {
      //    return false;
      //  }
      //  EcmaPropertyDescriptor existing = GetRuntimeData(receiver).GetOwnProperty(propertyKey);
      //  if (existing != null) {
      //    if (existing.IsAccessorDescriptor || !existing.Writable) {
      //      return false;
      //    }
      //    existing.Value = value;
      //    return true;
      //  }
      //  return GetRuntimeData(receiver).CreateDataProperty(propertyKey, value);
      //} else {
      //  if (ownDesc.Set != EcmaValue.Undefined) {
      //    ownDesc.Set.Call(new EcmaValue(receiver), value); return true;
      //  }
      //}
    }

    [EcmaSpecification("OrdinaryDelete", EcmaSpecificationKind.InternalMethod)]
    public virtual bool Delete(EcmaPropertyKey propertyKey) {
      EcmaPropertyDescriptor property = GetOwnProperty(propertyKey);
      if (property == null) {
        return true;
      }
      if (property.Configurable == false) {
        return false;
      }
      if (keys != null) {
        keys.Remove(propertyKey);
      }
      return dictionary.Remove(propertyKey);
    }

    [EcmaSpecification("OrdinaryHasInstance", EcmaSpecificationKind.InternalMethod)]
    public virtual bool HasInstance(RuntimeObject obj) {
      if (IsCallable) {
        BoundRuntimeFunction boundFn = this as BoundRuntimeFunction;
        if (boundFn != null) {
          return new EcmaValue(obj).InstanceOf(new EcmaValue(boundFn));
        }
        EcmaValue proto = Get(WellKnownPropertyName.Prototype, this);
        if (proto.Type != EcmaValueType.Object) {
          throw new EcmaTypeErrorException(InternalString.Error.PrototypeMustBeObjectOrNull);
        }
        RuntimeObject protoObj = proto.ToObject();
        for (RuntimeObject p = obj.GetPrototypeOf(); p != null; p = p.GetPrototypeOf()) {
          if (p == protoObj) {
            return true;
          }
        }
      }
      return false;
    }

    [EcmaSpecification("Call", EcmaSpecificationKind.InternalMethod)]
    public virtual EcmaValue Call(EcmaValue thisValue, params EcmaValue[] arguments) {
      throw new EcmaTypeErrorException(InternalString.Error.NotFunction);
    }

    [EcmaSpecification("Construct", EcmaSpecificationKind.InternalMethod)]
    public virtual EcmaValue Construct(RuntimeObject newTarget, params EcmaValue[] arguments) {
      throw new EcmaTypeErrorException(InternalString.Error.NotFunction);
    }

    public virtual bool IsWellKnownMethodOverriden(string method) {
      return false;
    }

    public virtual bool IsWellKnownSymbolOverriden(WellKnownSymbol symbol) {
      return false;
    }

    public override string ToString() {
      return ObjectPrototype.ToString(new EcmaValue(this)).ToString();
    }

    public EcmaValue ToValue() {
      return new EcmaValue(new EcmaValueHandle(GetHashCode()), this);
    }

    public static explicit operator RuntimeObject(WellKnownObject type) {
      return RuntimeRealm.GetRuntimeObject(type);
    }

    protected void DefineOwnPropertyNoChecked(EcmaPropertyKey propertyKey, EcmaPropertyDescriptor descriptor) {
      if (dictionary == null) {
        dictionary = new Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor>();
        keys = new EcmaPropertyKeyCollection();
      }
      dictionary[propertyKey] = descriptor.Clone();
      if (!keys.Contains(propertyKey)) {
        keys.Add(propertyKey);
      }
      if (descriptor.Configurable == false) {
        integrityLevel = this.TestIntegrityLevel();
      }
    }

    private EcmaPropertyDescriptor GetProperty(EcmaPropertyKey propertyKey) {
      for (RuntimeObject cur = this; cur != null; cur = cur.GetPrototypeOf()) {
        EcmaPropertyDescriptor property = cur.GetOwnProperty(propertyKey);
        if (property != null) {
          return property.Clone();
        }
      }
      return null;
    }

    private EcmaValue ToPrimitive(EcmaPreferredPrimitiveType kind) {
      EcmaValue result;
      if (IsWellKnownSymbolOverriden(WellKnownSymbol.ToPrimitive)) {
        if (TryConvertToPrimitive(Symbol.ToPrimitive, out result)) {
          return result;
        }
        throw new EcmaTypeErrorException(InternalString.Error.NotConvertibleToPrimitive);
      }
      WellKnownPropertyName[] m = kind == EcmaPreferredPrimitiveType.String ? convertToPrimitiveMethodString : convertToPrimitiveMethodNumber;
      if (TryConvertToPrimitive(m[0], out result) || TryConvertToPrimitive(m[1], out result)) {
        return result;
      }
      throw new EcmaTypeErrorException(InternalString.Error.NotConvertibleToPrimitive);
    }

    private bool TryConvertToPrimitive(EcmaPropertyKey name, out EcmaValue result) {
      RuntimeObject method = this.GetMethod(name);
      if (method != null) {
        result = method.Call(new EcmaValue(this));
        return result.Type != EcmaValueType.Object;
      }
      result = default;
      return false;
    }

    private void ThrowIfRealmDisposed() {
      if (realm.Disposed) {
        throw new ObjectDisposedException("realm");
      }
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay {
      get { return InspectorUtility.WriteValue((RuntimeObject)this); }
    }

    #region IEcmaValueBinder
    object IEcmaValueBinder.FromHandle(EcmaValueHandle handle) {
      return this;
    }

    EcmaValueHandle IEcmaValueBinder.ToHandle(object value) {
      return new EcmaValueHandle(GetHashCode());
    }

    string IEcmaValueBinder.GetToStringTag(EcmaValueHandle handle) {
      if (this.IsCallable) {
        return InternalString.ObjectTag.Function;
      }
      if (this is IntrinsicObject t) {
        return t.IntrinsicValue.ToStringTag;
      }
      EcmaValue customTag = Get(WellKnownSymbol.ToStringTag, null);
      if (!customTag.IsNullOrUndefined) {
        return customTag.ToString();
      }
      return InternalString.ObjectTag.Object;
    }

    EcmaValueType IEcmaValueBinder.GetValueType(EcmaValueHandle handle) {
      return EcmaValueType.Object;
    }

    EcmaNumberType IEcmaValueBinder.GetNumberType(EcmaValueHandle handle) {
      return EcmaNumberType.Invalid;
    }

    int IEcmaValueBinder.GetHashCode(EcmaValueHandle handle) {
      return GetHashCode();
    }

    bool IEcmaValueBinder.IsExtensible(EcmaValueHandle handle) {
      return this.IsExtensible;
    }

    bool IEcmaValueBinder.IsCallable(EcmaValueHandle handle) {
      return this.IsCallable;
    }

    bool IEcmaValueBinder.ToBoolean(EcmaValueHandle handle) {
      return true;
    }

    int IEcmaValueBinder.ToInt32(EcmaValueHandle handle) {
      return ToPrimitive(EcmaPreferredPrimitiveType.Number).ToInt32();
    }

    long IEcmaValueBinder.ToInt64(EcmaValueHandle handle) {
      return ToPrimitive(EcmaPreferredPrimitiveType.Number).ToInt64();
    }

    double IEcmaValueBinder.ToDouble(EcmaValueHandle handle) {
      return ToPrimitive(EcmaPreferredPrimitiveType.Number).ToDouble();
    }

    string IEcmaValueBinder.ToString(EcmaValueHandle handle) {
      return ToPrimitive(EcmaPreferredPrimitiveType.String).ToString();
    }

    EcmaValue IEcmaValueBinder.ToPrimitive(EcmaValueHandle handle, EcmaPreferredPrimitiveType preferredType) {
      return ToPrimitive(preferredType);
    }

    EcmaValue IEcmaValueBinder.ToNumber(EcmaValueHandle handle) {
      return ToPrimitive(EcmaPreferredPrimitiveType.Number).ToNumber();
    }

    RuntimeObject IEcmaValueBinder.ToRuntimeObject(EcmaValueHandle handle) {
      return this;
    }

    bool IEcmaValueBinder.TryGet(EcmaValueHandle handle, EcmaPropertyKey name, out EcmaValue value) {
      if (HasProperty(name)) {
        value = Get(name, this);
        return true;
      }
      value = default;
      return false;
    }

    bool IEcmaValueBinder.TrySet(EcmaValueHandle handle, EcmaPropertyKey name, EcmaValue value) {
      return Set(name, value, this);
    }

    EcmaValue IEcmaValueBinder.Call(EcmaValueHandle handle, EcmaValue thisValue, EcmaValue[] arguments) {
      return Call(thisValue, arguments);
    }

    bool IEcmaValueBinder.HasProperty(EcmaValueHandle handle, EcmaPropertyKey name) {
      return HasProperty(name);
    }

    bool IEcmaValueBinder.HasOwnProperty(EcmaValueHandle handle, EcmaPropertyKey name) {
      return this.OwnPropertyKeys.Contains(name);
    }

    IEnumerable<EcmaPropertyKey> IEcmaValueBinder.GetEnumerableOwnProperties(EcmaValueHandle handle) {
      return this.OwnPropertyKeys.Where(v => GetOwnProperty(v).Enumerable.Value);
    }
    #endregion
  }
}
