using Codeless.Ecma.Runtime;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Codeless.Ecma.Native {
  [DebuggerDisplay("{Value.DebuggerDisplay,nq}")]
  internal class ObjectReferenceBinder : IEcmaValueBinder {
    private static readonly WeakKeyedCollection dictionary = new WeakKeyedCollection();

    private readonly GCHandle handle;
    private readonly EcmaValueHandle longHandle;
    private readonly IEcmaValueBinder binder;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private RuntimeObject runtime_;

    private ObjectReferenceBinder(object target) {
      Guard.ArgumentNotNull(target, "target");
      this.handle = GCHandle.Alloc(target);
      this.longHandle = new EcmaValueHandle(((IntPtr)handle).ToInt64());
      this.binder = EcmaValueUtility.GetBinderForType(target.GetType());
    }

    ~ObjectReferenceBinder() {
      handle.Free();
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private RuntimeObject runtime {
      get {
        if (runtime_ == null) {
          RuntimeObjectBinder b = binder as RuntimeObjectBinder;
          if (b != null) {
            runtime_ = (RuntimeObject)handle.Target;
          } else {
            runtime_ = RuntimeRealm.GetRuntimeObject(handle.Target, false);
          }
        }
        return runtime_;
      }
    }

    public static ObjectReferenceBinder GetBinder(object target) {
      if (target == null) {
        return null;
      }
      return dictionary.GetOrAdd(new RuntimeProxyWeakReference(target)).GetBinder();
    }

    public EcmaValue Value {
      get { return new EcmaValue(longHandle, this); }
    }

    public RuntimeObject RuntimeObject {
      get {
        if (runtime != null) {
          return runtime;
        }
        return binder.ToRuntimeObject(longHandle);
      }
    }

    public bool IsCallable(EcmaValueHandle handle) {
      return binder.IsCallable(handle) || (runtime != null && runtime.IsCallable);
    }

    public bool IsExtensible(EcmaValueHandle handle) {
      return binder.IsExtensible(handle) && (runtime != null && runtime.IsExtensible);
    }

    public string GetToStringTag(EcmaValueHandle handle) {
      if (runtime != null && runtime.IsWellKnownSymbolOverriden(WellKnownSymbol.ToStringTag)) {
        return runtime.Get(Symbol.ToStringTag, runtime).ToString();
      }
      return binder.GetToStringTag(handle);
    }

    public EcmaValueType GetValueType(EcmaValueHandle handle) {
      return binder.GetValueType(handle);
    }

    public EcmaNumberType GetNumberType(EcmaValueHandle handle) {
      return binder.GetNumberType(handle);
    }

    public bool TryGet(EcmaValueHandle handle, EcmaPropertyKey name, out EcmaValue value) {
      if (binder.TryGet(handle, name, out value)) {
        return true;
      }
      if (runtime != null) {
        value = runtime.Get(name, runtime);
        return true;
      }
      return false;
    }

    public bool TrySet(EcmaValueHandle handle, EcmaPropertyKey name, EcmaValue value) {
      if (!binder.IsExtensible(handle)) {
        return false;
      }
      if (binder.TrySet(handle, name, value)) {
        return true;
      }
      RequireRuntimeObject();
      runtime.Set(name, value, runtime);
      return true;
    }

    public EcmaValue Call(EcmaValueHandle handle, EcmaValue thisValue, params EcmaValue[] arguments) {
      return binder.Call(handle, thisValue, arguments);
    }

    public bool HasProperty(EcmaValueHandle handle, EcmaPropertyKey name) {
      return binder.HasOwnProperty(handle, name) || (runtime != null && runtime.HasProperty(name));
    }

    public bool HasOwnProperty(EcmaValueHandle handle, EcmaPropertyKey name) {
      return binder.HasOwnProperty(handle, name) || (runtime != null && runtime.GetOwnProperty(name) != null);
    }

    public IEnumerable<EcmaPropertyKey> GetEnumerableOwnProperties(EcmaValueHandle handle) {
      foreach (var key in binder.GetEnumerableOwnProperties(handle)) {
        yield return key;
      }
      if (runtime != null && runtime != this.handle.Target) {
        foreach (var key in runtime.OwnPropertyKeys) {
          EcmaPropertyDescriptor descriptor = runtime.GetOwnProperty(key);
          if (descriptor.Enumerable.Value) {
            yield return key;
          }
        }
      }
    }

    public bool ToBoolean(EcmaValueHandle handle) {
      return binder.ToBoolean(handle);
    }

    public int ToInt32(EcmaValueHandle handle) {
      //if (runtime != null) {
      //  return NativeBinder.RuntimeObject.ToPrimitive(runtime, EcmaPreferredPrimitiveType.Number).ToInt32();
      //}
      return binder.ToInt32(handle);
    }

    public long ToInt64(EcmaValueHandle handle) {
      //if (runtime != null) {
      //  return NativeBinder.RuntimeObject.ToPrimitive(runtime, EcmaPreferredPrimitiveType.Number).ToInt64();
      //}
      return binder.ToInt64(handle);
    }

    public double ToDouble(EcmaValueHandle handle) {
      //if (runtime != null) {
      //  return NativeBinder.RuntimeObject.ToPrimitive(runtime, EcmaPreferredPrimitiveType.Number).ToDouble();
      //}
      return binder.ToDouble(handle);
    }

    public string ToString(EcmaValueHandle handle) {
      //if (runtime != null) {
      //  return NativeBinder.RuntimeObject.ToPrimitive(runtime, EcmaPreferredPrimitiveType.String).ToString();
      //}
      return binder.ToString(handle);
    }

    public EcmaValue ToNumber(EcmaValueHandle handle) {
      //if (runtime != null) {
      //  return NativeBinder.RuntimeObject.ToPrimitive(runtime, EcmaPreferredPrimitiveType.Number).ToNumber();
      //}
      return binder.ToNumber(handle);
    }

    public EcmaValue ToPrimitive(EcmaValueHandle handle, EcmaPreferredPrimitiveType kind) {
      //if (runtime != null) {
      //  return NativeBinder.RuntimeObject.ToPrimitive(runtime, kind);
      //}
      return binder.ToPrimitive(handle, kind);
    }

    public EcmaPropertyDescriptor GetOwnProperty(EcmaPropertyKey propertyKey) {
      EcmaPropertyDescriptor value = null;
      if (runtime != null) {
        value = runtime.GetOwnProperty(propertyKey);
      }
      if (value == null) {
        if (binder.HasOwnProperty(longHandle, propertyKey)) {
          EcmaValue v;
          binder.TryGet(longHandle, propertyKey, out v);
          value = new EcmaPropertyDescriptor(v, EcmaPropertyAttributes.Writable);
        }
      }
      return value;
    }

    public bool DefineOwnProperty(EcmaPropertyKey propertyKey, EcmaPropertyDescriptor descriptor) {
      if (binder.HasOwnProperty(longHandle, propertyKey)) {
        EcmaValue v;
        binder.TryGet(longHandle, propertyKey, out v);
        EcmaPropertyDescriptor native = new EcmaPropertyDescriptor(v, EcmaPropertyAttributes.Writable);
        if (EcmaPropertyDescriptor.ValidateAndApplyPropertyDescriptor(ref descriptor, native, !binder.IsExtensible(longHandle))) {
          return binder.TrySet(longHandle, propertyKey, descriptor.Value);
        }
      }
      RequireRuntimeObject();
      return runtime.DefineOwnProperty(propertyKey, descriptor);
    }

    public bool Delete(EcmaPropertyKey propertyKey) {
      if (runtime == null) {
        return false;
      }
      return runtime.Delete(propertyKey);
    }
    
    private void RequireRuntimeObject() {
      if (runtime == null) {
        RuntimeObject proto = binder.ToRuntimeObject(longHandle);
        runtime_ = RuntimeRealm.GetRuntimeObject(handle.Target, true);
        runtime.SetPrototypeOf(proto);
      }
    }

    #region Interface
    EcmaValueHandle IEcmaValueBinder.ToHandle(object value) {
      return longHandle;
    }

    object IEcmaValueBinder.FromHandle(EcmaValueHandle handle) {
      return this.handle.Target;
    }

    int IEcmaValueBinder.GetHashCode(EcmaValueHandle handle) {
      return binder.GetHashCode(handle);
    }

    RuntimeObject IEcmaValueBinder.ToRuntimeObject(EcmaValueHandle handle) {
      return this.RuntimeObject;
    }
    #endregion

    #region Helper classes
    private class RuntimeProxyWeakReference : WeakKeyedItem {
      private readonly WeakReference binderRef = new WeakReference(null);

      public RuntimeProxyWeakReference(object target)
        : base(target) { }

      public ObjectReferenceBinder GetBinder() {
        ObjectReferenceBinder binder = (ObjectReferenceBinder)binderRef.Target;
        if (binder == null) {
          object target = this.Target;
          if (target == null) {
            throw new InvalidOperationException();
          }
          binder = new ObjectReferenceBinder(target);
          binderRef.Target = binder;
        }
        return binder;
      }
    }
    #endregion
  }
}
