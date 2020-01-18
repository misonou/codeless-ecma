using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Codeless.Ecma.Runtime;

namespace Codeless.Ecma.Primitives {
  internal abstract class PrimitiveBinder<T> : IEcmaValueBinder {
    public abstract EcmaNumberType NumberType { get; }

    public abstract string ToStringTag { get; }

    public abstract EcmaValueType ValueType { get; }

    public abstract RuntimeObject ToRuntimeObject(T value);

    public abstract EcmaValueHandle ToHandle(T value);

    public abstract T FromHandle(EcmaValueHandle handle);

    public abstract bool ToBoolean(T value);

    public abstract int ToInt32(T value);

    public abstract long ToInt64(T value);

    public abstract double ToDouble(T value);

    public abstract string ToString(T value);

    public virtual EcmaValue ToNumber(T value) {
      return ToDouble(value);
    }

    public EcmaValue CreateValue(T value) {
      return new EcmaValue(ToHandle(value), this);
    }

    public virtual bool HasOwnProperty(T target, EcmaPropertyKey name) {
      return false;
    }

    public virtual IEnumerable<EcmaPropertyKey> GetEnumerableOwnProperties(T target) {
      yield break;
    }

    public virtual bool TryGet(T target, EcmaPropertyKey name, out EcmaValue value) {
      value = default;
      return false;
    }

    public virtual bool TrySet(T target, EcmaPropertyKey name, EcmaValue value) {
      return false;
    }

    public virtual int GetHashCode(T value) {
      return value.GetHashCode();
    }

    #region Interface
    bool IEcmaValueBinder.IsCallable(EcmaValueHandle handle) {
      return false;
    }

    bool IEcmaValueBinder.IsExtensible(EcmaValueHandle handle) {
      return false;
    }

    string IEcmaValueBinder.GetToStringTag(EcmaValueHandle handle) {
      return this.ToStringTag;
    }

    EcmaValueType IEcmaValueBinder.GetValueType(EcmaValueHandle handle) {
      return this.ValueType;
    }

    EcmaNumberType IEcmaValueBinder.GetNumberType(EcmaValueHandle handle) {
      return this.NumberType;
    }

    RuntimeObject IEcmaValueBinder.ToRuntimeObject(EcmaValueHandle value) {
      return ToRuntimeObject(FromHandle(value));
    }

    EcmaValueHandle IEcmaValueBinder.ToHandle(object value) {
      return ToHandle((T)value);
    }

    object IEcmaValueBinder.FromHandle(EcmaValueHandle handle) {
      return FromHandle(handle);
    }

    int IEcmaValueBinder.GetHashCode(EcmaValueHandle handle) {
      return GetHashCode(FromHandle(handle));
    }

    bool IEcmaValueBinder.HasProperty(EcmaValueHandle handle, EcmaPropertyKey name) {
      return HasOwnProperty(FromHandle(handle), name);
    }

    bool IEcmaValueBinder.HasOwnProperty(EcmaValueHandle handle, EcmaPropertyKey name) {
      return HasOwnProperty(FromHandle(handle), name);
    }

    IEnumerable<EcmaPropertyKey> IEcmaValueBinder.GetEnumerableOwnProperties(EcmaValueHandle handle) {
      return GetEnumerableOwnProperties(FromHandle(handle));
    }

    bool IEcmaValueBinder.TryGet(EcmaValueHandle handle, EcmaPropertyKey name, out EcmaValue value) {
      return TryGet(FromHandle(handle), name, out value);
    }

    bool IEcmaValueBinder.TrySet(EcmaValueHandle handle, EcmaPropertyKey name, EcmaValue value) {
      return TrySet(FromHandle(handle), name, value);
    }

    EcmaValue IEcmaValueBinder.Call(EcmaValueHandle handle, EcmaValue thisValue, EcmaValue[] arguments) {
      throw new EcmaTypeErrorException(InternalString.Error.NotFunction);
    }

    bool IEcmaValueBinder.ToBoolean(EcmaValueHandle handle) {
      return ToBoolean(FromHandle(handle));
    }

    long IEcmaValueBinder.ToInt64(EcmaValueHandle handle) {
      return ToInt64(FromHandle(handle));
    }

    double IEcmaValueBinder.ToDouble(EcmaValueHandle handle) {
      return ToDouble(FromHandle(handle));
    }

    string IEcmaValueBinder.ToString(EcmaValueHandle handle) {
      return ToString(FromHandle(handle));
    }

    EcmaValue IEcmaValueBinder.ToNumber(EcmaValueHandle handle) {
      return ToNumber(FromHandle(handle));
    }

    EcmaValue IEcmaValueBinder.ToPrimitive(EcmaValueHandle handle, EcmaPreferredPrimitiveType preferredType) {
      return new EcmaValue(handle, this);
    }
    #endregion
  }
}
