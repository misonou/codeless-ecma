using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Codeless.Ecma.Runtime;

namespace Codeless.Ecma.Primitives {
  internal class NullBinder : IEcmaValueBinder {
    public static readonly NullBinder Default = new NullBinder();

    protected NullBinder() { }

    public object FromHandle(EcmaValueHandle value) {
      return null;
    }

    public EcmaValueHandle ToHandle(object value) {
      return EcmaValueHandle.Null;
    }

    public string GetToStringTag(EcmaValueHandle handle) {
      return InternalString.ObjectTag.Object;
    }

    public EcmaValueType GetValueType(EcmaValueHandle handle) {
      return EcmaValueType.Null;
    }

    public EcmaNumberType GetNumberType(EcmaValueHandle handle) {
      return EcmaNumberType.Invalid;
    }

    public int GetHashCode(EcmaValueHandle value) {
      return value.GetHashCode();
    }

    public bool IsExtensible(EcmaValueHandle value) {
      return false;
    }

    public bool IsCallable(EcmaValueHandle value) {
      return false;
    }

    public bool ToBoolean(EcmaValueHandle value) {
      return false;
    }

    public int ToInt32(EcmaValueHandle value) {
      return 0;
    }

    public long ToInt64(EcmaValueHandle value) {
      return 0;
    }

    public double ToDouble(EcmaValueHandle value) {
      return 0;
    }

    public string ToString(EcmaValueHandle value) {
      return "null";
    }

    public EcmaValue ToPrimitive(EcmaValueHandle handle, EcmaPreferredPrimitiveType preferredType) {
      return ToString(handle);
    }

    public EcmaValue ToNumber(EcmaValueHandle handle) {
      return ToDouble(handle);
    }

    public RuntimeObject ToRuntimeObject(EcmaValueHandle handle) {
      return null;
    }

    public bool TryGet(EcmaValueHandle handle, EcmaPropertyKey name, out EcmaValue value) {
      value = default;
      return false;
    }

    public bool TrySet(EcmaValueHandle handle, EcmaPropertyKey name, EcmaValue value) {
      throw new EcmaTypeErrorException(InternalString.Error.SetPropertyNullOrUndefined);
    }

    public EcmaValue Call(EcmaValueHandle handle, EcmaValue thisValue, EcmaValue[] arguments) {
      throw new EcmaTypeErrorException(InternalString.Error.NotFunction);
    }

    public bool HasProperty(EcmaValueHandle handle, EcmaPropertyKey name) {
      return false;
    }

    public bool HasOwnProperty(EcmaValueHandle handle, EcmaPropertyKey name) {
      return false;
    }

    public IEnumerable<EcmaPropertyKey> GetEnumerableOwnProperties(EcmaValueHandle handle) {
      yield break;
    }
  }
}
