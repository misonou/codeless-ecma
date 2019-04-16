using Codeless.Ecma.Runtime;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Codeless.Ecma.Primitives {
  [DebuggerStepThrough]
  [DebuggerDisplay("{Value.DebuggerDisplay,nq}")]
  internal class PrimitiveBinderWrapper<T> : IEcmaValueBinder where T : class {
    private static readonly WeakKeyedCollection dictionary = new WeakKeyedCollection();

    private readonly T value;
    private readonly PrimitiveBinder<T> binder;

    private PrimitiveBinderWrapper(T value, PrimitiveBinder<T> binder) {
      Guard.ArgumentNotNull(value, "target");
      Guard.ArgumentNotNull(binder, "binder");
      this.value = value;
      this.binder = binder;
    }

    public static IEcmaValueBinder GetBinder(T target, PrimitiveBinder<T> binder) {
      return dictionary.GetOrAdd(new RuntimeProxyWeakReference(target)).GetBinder(binder);
    }

    public bool IsCallable(EcmaValueHandle handle) {
      return false;
    }

    public bool IsExtensible(EcmaValueHandle handle) {
      return false;
    }

    public string GetToStringTag(EcmaValueHandle handle) {
      return binder.ToStringTag;
    }

    public EcmaValueType GetValueType(EcmaValueHandle handle) {
      return binder.ValueType;
    }

    public EcmaNumberType GetNumberType(EcmaValueHandle handle) {
      return binder.NumberType;
    }

    public bool TryGet(EcmaValueHandle handle, EcmaPropertyKey name, out EcmaValue value) {
      return binder.TryGet(this.value, name, out value);
    }

    public bool TrySet(EcmaValueHandle handle, EcmaPropertyKey name, EcmaValue value) {
      return binder.TrySet(this.value, name, value);
    }

    public EcmaValue Call(EcmaValueHandle handle, EcmaValue thisValue, params EcmaValue[] arguments) {
      throw new EcmaTypeErrorException(InternalString.Error.NotFunction);
    }

    public bool HasProperty(EcmaValueHandle handle, EcmaPropertyKey name) {
      return binder.HasOwnProperty(value, name);
    }

    public bool HasOwnProperty(EcmaValueHandle handle, EcmaPropertyKey name) {
      return binder.HasOwnProperty(value, name);
    }

    public IEnumerable<EcmaPropertyKey> GetEnumerableOwnProperties(EcmaValueHandle handle) {
      return binder.GetEnumerableOwnProperties(value);
    }

    public bool ToBoolean(EcmaValueHandle handle) {
      return binder.ToBoolean(value);
    }

    public int ToInt32(EcmaValueHandle handle) {
      return binder.ToInt32(value);
    }

    public long ToInt64(EcmaValueHandle handle) {
      return binder.ToInt64(value);
    }

    public double ToDouble(EcmaValueHandle handle) {
      return binder.ToDouble(value);
    }

    public string ToString(EcmaValueHandle handle) {
      return binder.ToString(value);
    }

    public EcmaValue ToNumber(EcmaValueHandle handle) {
      return binder.ToNumber(value);
    }

    public EcmaValue ToPrimitive(EcmaValueHandle handle, EcmaPreferredPrimitiveType kind) {
      return binder.ToPrimitive(value, kind);
    }

    public EcmaValueHandle ToHandle(object value) {
      return new EcmaValueHandle(value.GetHashCode());
    }

    public object FromHandle(EcmaValueHandle handle) {
      return this.value;
    }

    public int GetHashCode(EcmaValueHandle handle) {
      return binder.GetHashCode(value);
    }

    public RuntimeObject ToRuntimeObject(EcmaValueHandle handle) {
      return binder.ToRuntimeObject(value);
    }

    public EcmaValue ToValue() {
      return new EcmaValue(ToHandle(value), this);
    }

    #region Helper classes
    private class RuntimeProxyWeakReference : WeakKeyedItem {
      private readonly WeakReference binderRef = new WeakReference(null);

      public RuntimeProxyWeakReference(T target)
        : base(target) { }

      public PrimitiveBinderWrapper<T> GetBinder(PrimitiveBinder<T> b) {
        PrimitiveBinderWrapper<T> binder = (PrimitiveBinderWrapper<T>)binderRef.Target;
        if (binder == null) {
          T target = (T)this.Target;
          if (target == null) {
            throw new InvalidOperationException();
          }
          binder = new PrimitiveBinderWrapper<T>(target, b);
          binderRef.Target = binder;
        }
        return binder;
      }
    }
    #endregion
  }
}
