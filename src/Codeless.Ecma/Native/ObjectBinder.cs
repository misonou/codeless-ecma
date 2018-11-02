using Codeless.Ecma.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Codeless.Ecma.Native {
  public abstract class ObjectBinder : IEcmaValueBinder {
    public virtual EcmaValueType GetValueType(EcmaValueHandle handle) {
      return EcmaValueType.Object;
    }

    public virtual EcmaNumberType GetNumberType(EcmaValueHandle handle) {
      return EcmaNumberType.Invalid;
    }

    public virtual string GetToStringTag(EcmaValueHandle handle) {
      return InternalString.ObjectTag.Object;
    }

    public virtual bool IsExtensible(EcmaValueHandle handle) {
      return true;
    }

    public virtual bool IsCallable(EcmaValueHandle handle) {
      return false;
    }

    public object FromHandle(EcmaValueHandle handle) {
      return handle.GetTargetAsGCHandle();
    }

    public int GetHashCode(EcmaValueHandle handle) {
      return handle.GetTargetAsGCHandle().GetHashCode();
    }

    public RuntimeObject ToRuntimeObject(EcmaValueHandle handle) {
      return ToRuntimeObject(handle.GetTargetAsGCHandle());
    }

    public bool ToBoolean(EcmaValueHandle handle) {
      return true;
    }

    public int ToInt32(EcmaValueHandle handle) {
      EcmaValue primitiveValue = ToPrimitive(handle.GetTargetAsGCHandle(), EcmaPreferredPrimitiveType.Number);
      return primitiveValue.ToInt32();
    }

    public long ToInt64(EcmaValueHandle handle) {
      EcmaValue primitiveValue = ToPrimitive(handle.GetTargetAsGCHandle(), EcmaPreferredPrimitiveType.Number);
      return primitiveValue.ToInt64();
    }

    public double ToDouble(EcmaValueHandle handle) {
      EcmaValue primitiveValue = ToPrimitive(handle.GetTargetAsGCHandle(), EcmaPreferredPrimitiveType.Number);
      return primitiveValue.ToDouble();
    }

    public string ToString(EcmaValueHandle handle) {
      EcmaValue primitiveValue = ToPrimitive(handle.GetTargetAsGCHandle(), EcmaPreferredPrimitiveType.String);
      return primitiveValue.ToString();
    }

    public EcmaValue ToNumber(EcmaValueHandle handle) {
      EcmaValue primitiveValue = ToPrimitive(handle.GetTargetAsGCHandle(), EcmaPreferredPrimitiveType.Number);
      return primitiveValue.ToNumber();
    }

    public EcmaValue ToPrimitive(EcmaValueHandle handle, EcmaPreferredPrimitiveType kind) {
      return ToPrimitive(handle.GetTargetAsGCHandle(), kind);
    }
    
    public bool TryGet(EcmaValueHandle handle, EcmaPropertyKey name, out EcmaValue value) {
      return TryGet(handle.GetTargetAsGCHandle(), name, out value);
    }

    public bool TrySet(EcmaValueHandle handle, EcmaPropertyKey name, EcmaValue value) {
      return TrySet(handle.GetTargetAsGCHandle(), name, value);
    }

    public bool HasProperty(EcmaValueHandle handle, EcmaPropertyKey name) {
      return HasProperty(handle.GetTargetAsGCHandle(), name);
    }

    public bool HasOwnProperty(EcmaValueHandle handle, EcmaPropertyKey name) {
      return HasOwnProperty(handle.GetTargetAsGCHandle(), name);
    }

    public IEnumerable<EcmaPropertyKey> GetEnumerableOwnProperties(EcmaValueHandle handle) {
      return GetEnumerableOwnProperties(handle.GetTargetAsGCHandle());
    }

    public virtual EcmaValue Call(EcmaValueHandle handle, EcmaValue thisValue, EcmaValue[] arguments) {
      throw new EcmaTypeErrorException(InternalString.Error.NotFunction);
    }

    protected abstract RuntimeObject ToRuntimeObject(object target);

    protected abstract EcmaValue ToPrimitive(object target, EcmaPreferredPrimitiveType kind);

    protected abstract IEnumerable<EcmaPropertyKey> GetEnumerableOwnProperties(object target);

    protected abstract bool HasProperty(object target, EcmaPropertyKey name);

    protected abstract bool HasOwnProperty(object target, EcmaPropertyKey name);

    protected abstract bool TryGet(object target, EcmaPropertyKey name, out EcmaValue value);

    protected abstract bool TrySet(object target, EcmaPropertyKey name, EcmaValue value);
    
    EcmaValueHandle IEcmaValueBinder.ToHandle(object value) {
      throw new InvalidOperationException();
    }
  }
}
