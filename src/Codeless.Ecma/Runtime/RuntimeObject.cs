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
    private static readonly EcmaValue[] hintString = { "default", "string", "number" };
    private Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor> dictionary;
    private EcmaPropertyKeyCollection keys;
    private RuntimeObject prototype;

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

    public RuntimeObjectIntegrityLevel IntegrityLevel { get; private set; }

    public RuntimeRealm Realm { get; } = RuntimeRealm.Current;

    protected virtual string ToStringTag => InternalString.ObjectTag.Object;

    public EcmaValue this[EcmaPropertyKey index] {
      get { return Get(index, this); }
      set { Set(index, value, this); }
    }

    public EcmaValue this[string key] {
      get { return this[new EcmaPropertyKey(key)]; }
      set { this[new EcmaPropertyKey(key)] = value; }
    }

    public EcmaValue this[int key] {
      get { return this[new EcmaPropertyKey(key)]; }
      set { this[new EcmaPropertyKey(key)] = value; }
    }

    public EcmaValue this[long key] {
      get { return this[new EcmaPropertyKey(key)]; }
      set { this[new EcmaPropertyKey(key)] = value; }
    }

    public EcmaValue this[Symbol key] {
      get { return this[new EcmaPropertyKey(key)]; }
      set { this[new EcmaPropertyKey(key)] = value; }
    }

    public EcmaValue this[EcmaValue key] {
      get { return this[EcmaPropertyKey.FromValue(key)]; }
      set { this[EcmaPropertyKey.FromValue(key)] = value; }
    }

    [EcmaSpecification("OrdinaryIsExtensible", EcmaSpecificationKind.InternalMethod)]
    public virtual bool IsExtensible {
      get { return IntegrityLevel == RuntimeObjectIntegrityLevel.Default; }
    }

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

    [EcmaSpecification("SpeciesConstructor", EcmaSpecificationKind.AbstractOperations)]
    public static RuntimeObject GetSpeciesConstructor(RuntimeObject obj, WellKnownObject defaultConstructor) {
      Guard.ArgumentNotNull(obj, "source");
      EcmaValue constructor = obj.Get(WellKnownPropertyName.Constructor);
      if (constructor == default) {
        return RuntimeRealm.GetRuntimeObject(defaultConstructor);
      }
      Guard.ArgumentIsObject(constructor);
      constructor = constructor[WellKnownSymbol.Species];
      if (constructor.IsNullOrUndefined) {
        return RuntimeRealm.GetRuntimeObject(defaultConstructor);
      }
      RuntimeObject runtimeObject = constructor.ToObject();
      if (runtimeObject.IsConstructor) {
        return runtimeObject;
      }
      throw new EcmaTypeErrorException(InternalString.Error.NotConstructor);
    }

    public virtual IEnumerable<EcmaPropertyKey> GetOwnPropertyKeys() {
      if (keys == null) {
        return Enumerable.Empty<EcmaPropertyKey>();
      }
      return keys.ToArray();
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
      if (IntegrityLevel == RuntimeObjectIntegrityLevel.Default) {
        IntegrityLevel = RuntimeObjectIntegrityLevel.ExtensionPrevented;
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

    [EcmaSpecification("OrdinaryHasProperty", EcmaSpecificationKind.InternalMethod)]
    public virtual bool HasProperty(EcmaPropertyKey propertyKey) {
      return GetProperty(propertyKey) != null;
    }

    public EcmaValue Get(EcmaPropertyKey propertyKey) {
      return Get(propertyKey, this);
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

    public bool Set(EcmaPropertyKey propertyKey, EcmaValue value) {
      return Set(propertyKey, value, this);
    }

    [EcmaSpecification("OrdinarySet", EcmaSpecificationKind.InternalMethod)]
    public virtual bool Set(EcmaPropertyKey propertyKey, EcmaValue value, RuntimeObject receiver) {
      EcmaPropertyDescriptor descriptor = GetProperty(propertyKey);
      if (descriptor == null) {
        RuntimeObject parent = GetPrototypeOf();
        if (parent != null) {
          return parent.Set(propertyKey, value, receiver ?? this);
        }
        return receiver.CreateDataProperty(propertyKey, value);
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
      return DefineOwnProperty(propertyKey, new EcmaPropertyDescriptor(value));
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

    public EcmaValue Invoke(EcmaPropertyKey propertyKey, params EcmaValue[] arguments) {
      return this.GetMethod(propertyKey).Call(this, arguments);
    }

    public EcmaValue Call() {
      return Call(EcmaValue.Undefined);
    }

    [EcmaSpecification("Call", EcmaSpecificationKind.InternalMethod)]
    public virtual EcmaValue Call(EcmaValue thisValue, params EcmaValue[] arguments) {
      throw new EcmaTypeErrorException(InternalString.Error.NotFunction);
    }

    public EcmaValue Construct(params EcmaValue[] arguments) {
      return Construct(arguments, this);
    }

    [EcmaSpecification("Construct", EcmaSpecificationKind.InternalMethod)]
    public virtual EcmaValue Construct(EcmaValue[] arguments, RuntimeObject newTarget) {
      throw new EcmaTypeErrorException(InternalString.Error.NotFunction);
    }

    public override string ToString() {
      return ToPrimitive(EcmaPreferredPrimitiveType.String).ToString();
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
        IntegrityLevel = this.TestIntegrityLevel();
      }
    }

    protected EcmaPropertyDescriptor GetProperty(EcmaPropertyKey propertyKey) {
      for (RuntimeObject cur = this; cur != null; cur = cur.GetPrototypeOf()) {
        EcmaPropertyDescriptor property = cur.GetOwnProperty(propertyKey);
        if (property != null) {
          return property.Clone();
        }
      }
      return null;
    }

    [EcmaSpecification("ToPrimitive", EcmaSpecificationKind.AbstractOperations)]
    protected EcmaValue ToPrimitive(EcmaPreferredPrimitiveType kind) {
      RuntimeFunction exoticToPrim = this.GetMethod(WellKnownSymbol.ToPrimitive);
      if (exoticToPrim != null) {
        EcmaValue result = exoticToPrim.Call(new EcmaValue(this), hintString[(int)kind]);
        if (result.Type == EcmaValueType.Object) {
          throw new EcmaTypeErrorException(InternalString.Error.NotConvertibleToPrimitive);
        }
        return result;
      }
      return this.OrdinaryToPrimitive(kind);
    }

    private void ThrowIfRealmDisposed() {
      if (Realm.Disposed) {
        throw new ObjectDisposedException("realm");
      }
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay {
      get { return InspectorUtility.WriteValue(this); }
    }

    #region IEcmaValueBinder
    object IEcmaValueBinder.FromHandle(EcmaValueHandle handle) {
      return this;
    }

    EcmaValueHandle IEcmaValueBinder.ToHandle(object value) {
      return new EcmaValueHandle(GetHashCode());
    }

    string IEcmaValueBinder.GetToStringTag(EcmaValueHandle handle) {
      EcmaValue customTag = Get(WellKnownSymbol.ToStringTag, null);
      if (customTag.Type == EcmaValueType.String) {
        return customTag.ToString();
      }
      return ToStringTag;
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
      return this.GetOwnPropertyKeys().Contains(name);
    }

    IEnumerable<EcmaPropertyKey> IEcmaValueBinder.GetEnumerableOwnProperties(EcmaValueHandle handle) {
      return this.GetOwnPropertyKeys().Where(v => GetOwnProperty(v).Enumerable);
    }
    #endregion
  }
}
