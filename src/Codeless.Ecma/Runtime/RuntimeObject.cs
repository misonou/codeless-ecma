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
    private static readonly EcmaPropertyDescriptor sealedProperty = new EcmaPropertyDescriptor { Configurable = false };
    private static readonly EcmaPropertyDescriptor frozenProperty = new EcmaPropertyDescriptor { Configurable = false, Writable = false };
    [ThreadStatic]
    private static RuntimeRealm currentRealm;

    private Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor> dictionary;
    private EcmaPropertyKeyCollection keys;
    private RuntimeObject prototype;

    public RuntimeObject(WellKnownObject defaultProto) {
      this.Realm = currentRealm ?? RuntimeRealm.Current;
      this.prototype = this.Realm.GetRuntimeObject(defaultProto);
    }

    public RuntimeObject(WellKnownObject defaultProto, RuntimeObject constructor) {
      this.prototype = GetPrototypeFromConstructor(constructor, defaultProto);
      this.Realm = prototype != null ? prototype.Realm : RuntimeRealm.Current;
    }

    protected RuntimeObject(WellKnownObject defaultProto, bool shared) {
      this.Realm = shared ? RuntimeRealm.SharedRealm : RuntimeRealm.Current;
      this.prototype = this.Realm.GetRuntimeObject(defaultProto);
    }

    protected RuntimeObject(object target)
      : base(target) {
      this.Realm = RuntimeRealm.Current;
    }

    public virtual bool IsCallable => false;

    public virtual bool IsConstructor => false;

    public RuntimeObjectIntegrityLevel IntegrityLevel { get; private set; }

    public RuntimeRealm Realm { get; protected set; }

    protected virtual string ToStringTag => InternalString.ObjectTag.Object;

    public EcmaValue this[EcmaPropertyKey index] {
      get { return Get(index, this); }
      set { Set(index, value, this); }
    }

    public virtual EcmaValue this[string key] {
      get { return this[new EcmaPropertyKey(key)]; }
      set { this[new EcmaPropertyKey(key)] = value; }
    }

    public virtual EcmaValue this[int key] {
      get { return this[new EcmaPropertyKey(key)]; }
      set { this[new EcmaPropertyKey(key)] = value; }
    }

    public virtual EcmaValue this[long key] {
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
    public static T CreateFromConstructor<T>(RuntimeObject constructor, WellKnownObject defaultProto) where T : RuntimeObject, new() {
      RuntimeObject proto = GetPrototypeFromConstructor(constructor, defaultProto);
      if (proto != null) {
        currentRealm = proto.Realm;
      }
      T obj;
      try {
        obj = new T();
      } finally {
        currentRealm = null;
      }
      obj.prototype = proto;
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
      EcmaValue proto = constructor.Get(WellKnownProperty.Prototype);
      if (proto.Type == EcmaValueType.Object) {
        return proto.ToObject();
      }
      if (defaultProto != 0) {
        return constructor.Realm.GetRuntimeObject(defaultProto);
      }
      return null;
    }

    [EcmaSpecification("SpeciesConstructor", EcmaSpecificationKind.AbstractOperations)]
    public static RuntimeObject GetSpeciesConstructor(RuntimeObject obj, WellKnownObject defaultConstructor) {
      Guard.ArgumentNotNull(obj, "source");
      EcmaValue constructor = obj.Get(WellKnownProperty.Constructor);
      if (constructor == default) {
        return obj.Realm.GetRuntimeObject(defaultConstructor);
      }
      Guard.ArgumentIsObject(constructor);
      constructor = constructor[WellKnownSymbol.Species];
      if (constructor.IsNullOrUndefined) {
        return obj.Realm.GetRuntimeObject(defaultConstructor);
      }
      RuntimeObject runtimeObject = constructor.ToObject();
      if (runtimeObject.IsConstructor) {
        return runtimeObject;
      }
      throw new EcmaTypeErrorException(InternalString.Error.NotConstructor);
    }

    [EcmaSpecification("OrdinaryOwnPropertyKeys", EcmaSpecificationKind.InternalMethod)]
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
      EcmaValue v = proto == null ? EcmaValue.Null : proto;
      if (v.Type != EcmaValueType.Null && v.Type != EcmaValueType.Object) {
        throw new EcmaTypeErrorException(InternalString.Error.PrototypeMustBeObjectOrNull);
      }
      if (prototype == proto) {
        return true;
      }
      if (!this.IsExtensible) {
        return false;
      }
      for (RuntimeObject p = proto; p != null; p = p.GetPrototypeOf()) {
        if (p is RuntimeObjectProxy) {
          break;
        }
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
        IntegrityLevel = TestIntegrityLevel();
      }
      return true;
    }

    [EcmaSpecification("OrdinaryGetOwnProperty", EcmaSpecificationKind.InternalMethod)]
    public virtual EcmaPropertyDescriptor GetOwnProperty(EcmaPropertyKey propertyKey) {
      EcmaPropertyDescriptor value;
      if (dictionary != null && dictionary.TryGetValue(propertyKey, out value)) {
        EcmaPropertyDescriptor clone = value.EnsureSharedValue(this.Realm);
        if (clone != value) {
          dictionary[propertyKey] = clone;
        }
        return clone;
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
      if (GetOwnProperty(propertyKey) != null) {
        return true;
      }
      RuntimeObject parent = GetPrototypeOf();
      return parent != null ? parent.HasProperty(propertyKey) : false;
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
        descriptor = new EcmaPropertyDescriptor(EcmaValue.Undefined, EcmaPropertyAttributes.DefaultDataProperty);
      }
      if (descriptor.IsAccessorDescriptor) {
        if (descriptor.Set != default) {
          descriptor.Set.Call(receiver ?? this, value);
          return true;
        }
        return false;
      }
      if (descriptor.Writable == false) {
        return false;
      }
      EcmaPropertyDescriptor existingDesc = receiver.GetOwnProperty(propertyKey);
      if (existingDesc == null) {
        return receiver.CreateDataProperty(propertyKey, value);
      }
      if (existingDesc.IsAccessorDescriptor || !existingDesc.Writable) {
        return false;
      }
      return receiver.DefineOwnProperty(propertyKey, new EcmaPropertyDescriptor(value));
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
        EcmaValue proto = Get(WellKnownProperty.Prototype, this);
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
      RuntimeObject fn = this.GetMethod(propertyKey);
      if (fn == null) {
        throw new EcmaTypeErrorException(InternalString.Error.NotFunction);
      }
      return fn.Call(this, arguments);
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
      throw new EcmaTypeErrorException(InternalString.Error.NotConstructor);
    }

    [EcmaSpecification("SetIntegrityLevel", EcmaSpecificationKind.AbstractOperations)]
    public virtual bool SetIntegrityLevel(RuntimeObjectIntegrityLevel level) {
      if (!PreventExtensions()) {
        return false;
      }
      if (IntegrityLevel < level) {
        switch (level) {
          case RuntimeObjectIntegrityLevel.Sealed:
            foreach (EcmaPropertyKey key in GetOwnPropertyKeys()) {
              this.DefinePropertyOrThrow(key, sealedProperty);
            }
            break;
          case RuntimeObjectIntegrityLevel.Frozen:
            foreach (EcmaPropertyKey key in GetOwnPropertyKeys()) {
              this.DefinePropertyOrThrow(key, frozenProperty);
            }
            break;
        }
      }
      return true;
    }

    public override string ToString() {
      return ToPrimitive(EcmaPreferredPrimitiveType.String).ToString();
    }

    public EcmaValue ToValue() {
      return new EcmaValue(new EcmaValueHandle(GetHashCode()), this);
    }

    public static explicit operator RuntimeObject(WellKnownObject type) {
      return RuntimeRealm.Current.GetRuntimeObject(type);
    }

    [EcmaSpecification("TestIntegrityLevel", EcmaSpecificationKind.AbstractOperations)]
    protected virtual RuntimeObjectIntegrityLevel TestIntegrityLevel() {
      if (this.IsExtensible) {
        return RuntimeObjectIntegrityLevel.Default;
      }
      foreach (EcmaPropertyKey key in GetOwnPropertyKeys()) {
        EcmaPropertyDescriptor desc = GetOwnProperty(key);
        if (desc.Configurable != false) {
          return RuntimeObjectIntegrityLevel.ExtensionPrevented;
        }
        if (desc.IsDataDescriptor && desc.Writable != false) {
          return RuntimeObjectIntegrityLevel.Sealed;
        }
      }
      return RuntimeObjectIntegrityLevel.Frozen;
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
      if (!this.IsExtensible && descriptor.Configurable == false) {
        if ((descriptor.Writable == false && IntegrityLevel < RuntimeObjectIntegrityLevel.Frozen) || IntegrityLevel < RuntimeObjectIntegrityLevel.Sealed) {
          IntegrityLevel = TestIntegrityLevel();
        }
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
      RuntimeObject exoticToPrim = this.GetMethod(WellKnownSymbol.ToPrimitive);
      if (exoticToPrim != null) {
        EcmaValue result = exoticToPrim.Call(new EcmaValue(this), hintString[(int)kind]);
        if (result.Type == EcmaValueType.Object) {
          throw new EcmaTypeErrorException(InternalString.Error.NotConvertibleToPrimitive);
        }
        return result;
      }
      return this.OrdinaryToPrimitive(kind);
    }

    protected internal RuntimeObject Clone(RuntimeRealm realm) {
      Guard.ArgumentNotNull(realm, "realm");
      RuntimeObject clone = (RuntimeObject)MemberwiseClone();
      clone.Realm = realm;
      if (clone.dictionary != null) {
        clone.dictionary = new Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor>(clone.dictionary);
        clone.keys = new EcmaPropertyKeyCollection(clone.keys);
      }
      if (clone.prototype != null) {
        clone.prototype = realm.GetRuntimeObject(clone.prototype);
      }
      return clone;
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
      value = Get(name, this);
      return true;
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
