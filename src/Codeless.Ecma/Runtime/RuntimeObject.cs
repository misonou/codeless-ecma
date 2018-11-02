using Codeless.Ecma.Diagnostics;
using Codeless.Ecma.Diagnostics.VisualStudio;
using Codeless.Ecma.Native;
using Codeless.Ecma.Runtime.Intrinsics;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Codeless.Ecma.Runtime {
  [DebuggerTypeProxy(typeof(RuntimeObjectDebuggerProxy))]
  [DebuggerDisplay("{DebuggerDisplay,nq}")]
  public partial class RuntimeObject : WeakKeyedItem {
    private readonly RuntimeRealm realm = RuntimeRealm.Current;
    private Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor> dictionary;
    private EcmaPropertyKeyCollection keys;
    private ObjectReferenceBinder prototype;
    private bool extensionPrevented;

    public RuntimeObject(WellKnownObject defaultProto) {
      this.prototype = ObjectReferenceBinder.GetBinder(RuntimeRealm.GetWellKnownObject(defaultProto));
    }

    [EcmaSpecification("OrdinaryCreateFromConstructor", EcmaSpecificationKind.AbstractOperations)]
    public RuntimeObject(WellKnownObject defaultProto, RuntimeObject constructor) {
      this.prototype = ObjectReferenceBinder.GetBinder(GetPrototypeFromConstructor(constructor, defaultProto));
    }

    private RuntimeObject() { }

    private RuntimeObject(object target)
      : base(target) { }

    public static RuntimeObject CreateForNativeObject(object target) {
      Guard.ArgumentNotNull(target, "target");
      RuntimeObject obj = new RuntimeObject(target);
      obj.prototype = ObjectReferenceBinder.GetBinder(RuntimeRealm.GetWellKnownObject(WellKnownObject.ObjectPrototype));
      return obj;
    }

    [EcmaSpecification("ObjectCreate", EcmaSpecificationKind.AbstractOperations)]
    public static RuntimeObject Create(RuntimeObject proto) {
      RuntimeObject obj = new RuntimeObject();
      if (proto != null) {
        obj.prototype = ObjectReferenceBinder.GetBinder(proto);
      }
      return obj;
    }

    public static RuntimeObject Create(Exception ex) {
      EcmaException ecmaException = ex as EcmaException;
      if (ecmaException == null) {
        //ecmaException = new EcmaException(ex.Message);
      }
      return ErrorConstructor.CreateError(ecmaException);
    }

    [EcmaSpecification("GetPrototypeFromConstructor", EcmaSpecificationKind.AbstractOperations)]
    public static RuntimeObject GetPrototypeFromConstructor(RuntimeObject constructor, WellKnownObject defaultProto) {
      Guard.ArgumentNotNull(constructor, "constructor");
      if (!constructor.IsCallable) {
        throw new EcmaTypeErrorException("");
      }
      EcmaValue proto = constructor.Get(WellKnownPropertyName.Prototype);
      if (proto.Type == EcmaValueType.Object) {
        return proto.ToRuntimeObject();
      }
      return RuntimeRealm.GetWellKnownObject(defaultProto);
    }

    public virtual bool IsCallable {
      get { return false; }
    }

    [EcmaSpecification("OrdinaryIsExtensible", EcmaSpecificationKind.InternalMethod)]
    public virtual bool IsExtensible {
      get { return !extensionPrevented; }
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

    protected internal RuntimeRealm Realm {
      get { return realm; }
    }

    [EcmaSpecification("OrdinaryGetPrototypeOf", EcmaSpecificationKind.InternalMethod)]
    public virtual RuntimeObject GetPrototypeOf() {
      return prototype == null ? null : prototype.RuntimeObject;
    }

    [EcmaSpecification("OrdinarySetPrototypeOf", EcmaSpecificationKind.InternalMethod)]
    public virtual bool SetPrototypeOf(RuntimeObject proto) {
      EcmaValue v = new EcmaValue(proto);
      if (v.Type != EcmaValueType.Null && v.Type != EcmaValueType.Object) {
        throw new EcmaTypeErrorException(InternalString.Error.PrototypeNotObject);
      }
      if (prototype != null && EcmaValue.Equals(v, prototype.Value, EcmaValueComparison.SameValue)) {
        return true;
      }
      if (!this.IsExtensible) {
        return false;
      }
      for (RuntimeObject p = proto; p != null; p = p.GetPrototypeOf()) {
        if (p == this) {
          return false;
        }
      }
      prototype = ObjectReferenceBinder.GetBinder(proto);
      return true;
    }

    [EcmaSpecification("OrdinaryPreventExtensions", EcmaSpecificationKind.InternalMethod)]
    public virtual bool PreventExtensions() {
      extensionPrevented = true;
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
        return default(EcmaValue);
      }
      if (descriptor.IsAccessorDescriptor) {
        return descriptor.Get ? descriptor.Get.Call(new EcmaValue(receiver)) : default(EcmaValue);
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
        EcmaValue thisArg = new EcmaValue(receiver);
        if (descriptor.Set != EcmaValue.Undefined && thisArg.Type == EcmaValueType.Object) {
          descriptor.Set.Call(thisArg, value);
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
          throw new EcmaTypeErrorException(InternalString.Error.PrototypeNotObject);
        }
        RuntimeObject protoObj = proto.ToRuntimeObject();
        for (RuntimeObject p = obj.GetPrototypeOf(); p != null; p = p.GetPrototypeOf()) {
          if (p == protoObj) {
            return true;
          }
        }
      }
      return false;
    }

    public virtual EcmaValue Call(EcmaValue thisValue, params EcmaValue[] arguments) {
      throw new EcmaTypeErrorException(InternalString.Error.NotFunction);
    }

    public virtual EcmaValue Construct(EcmaValue newTarget, params EcmaValue[] arguments) {
      throw new EcmaTypeErrorException(InternalString.Error.NotFunction);
    }
    
    public virtual bool IsWellKnownMethodOverriden(string method) {
      return false;
    }

    public virtual bool IsWellKnownSymbolOverriden(WellKnownSymbol symbol) {
      return false;
    }

    public override string ToString() {
      return "[object" + new EcmaValue(this).ToStringTag + "]";
    }

    protected void DefineOwnPropertyNoChecked(EcmaPropertyKey propertyKey, EcmaPropertyDescriptor descriptor) {
      if (dictionary == null) {
        dictionary = new Dictionary<EcmaPropertyKey, EcmaPropertyDescriptor>();
        keys = new EcmaPropertyKeyCollection();
      }
      dictionary[propertyKey] = descriptor.Clone();
      keys.Add(propertyKey);
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay {
      get { return InspectorUtility.WriteValue(this); }
    }

    private EcmaPropertyDescriptor GetProperty(EcmaPropertyKey propertyKey) {
      RuntimeObject cur = this;
      while (cur != null) {
        EcmaPropertyDescriptor property = cur.GetOwnProperty(propertyKey);
        if (property != null) {
          return property.Clone();
        }
        cur = cur.GetPrototypeOf();
      }
      return null;
    }
  }
}
