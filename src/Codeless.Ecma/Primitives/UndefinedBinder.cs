using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Codeless.Ecma.Runtime;

namespace Codeless.Ecma.Primitives {
  internal class UndefinedBinder : IEcmaValueBinder {
    public static readonly UndefinedBinder Default = new UndefinedBinder();

    protected UndefinedBinder() { }

    public object FromHandle(EcmaValueHandle value) {
      return EcmaValue.Undefined;
    }

    public EcmaValueHandle ToHandle(object value) {
      return EcmaValueHandle.Undefined;
    }

    public string GetToStringTag(EcmaValueHandle handle) {
      return InternalString.ObjectTag.Undefined;
    }

    public EcmaValueType GetValueType(EcmaValueHandle handle) {
      return EcmaValueType.Undefined;
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
      return Double.NaN;
    }

    public string ToString(EcmaValueHandle value) {
      return "undefined";
    }

    public EcmaValue ToPrimitive(EcmaValueHandle handle, EcmaPreferredPrimitiveType preferredType) {
      return ToString(handle);
    }

    public EcmaValue ToNumber(EcmaValueHandle handle) {
      return ToDouble(handle);
    }

    public RuntimeObject ToRuntimeObject(EcmaValueHandle handle) {
      throw new EcmaTypeErrorException(InternalString.Error.NotCoercibleAsObject);
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
