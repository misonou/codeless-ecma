using System;
using System.Collections.Generic;

namespace Codeless.Ecma.Runtime {
  internal class SharedIntrinsicObjectBinder : IEcmaValueBinder {
    public static readonly SharedIntrinsicObjectBinder Default = new SharedIntrinsicObjectBinder();
    public const EcmaValueType SharedValue = (EcmaValueType)(-1);

    public EcmaValueType GetValueType(EcmaValueHandle handle) {
      return SharedValue;
    }

    public EcmaNumberType GetNumberType(EcmaValueHandle handle) {
      return EcmaNumberType.Int64;
    }

    public long ToInt64(EcmaValueHandle handle) {
      return handle.Value;
    }

    #region Unused interface methods
    public EcmaValue Call(EcmaValueHandle handle, EcmaValue thisValue, EcmaValue[] arguments) {
      throw new NotImplementedException();
    }

    public object FromHandle(EcmaValueHandle handle) {
      throw new NotImplementedException();
    }

    public IEnumerable<EcmaPropertyKey> GetEnumerableOwnProperties(EcmaValueHandle handle) {
      throw new NotImplementedException();
    }

    public int GetHashCode(EcmaValueHandle handle) {
      throw new NotImplementedException();
    }

    public string GetToStringTag(EcmaValueHandle handle) {
      throw new NotImplementedException();
    }

    public bool HasOwnProperty(EcmaValueHandle handle, EcmaPropertyKey name) {
      throw new NotImplementedException();
    }

    public bool HasProperty(EcmaValueHandle handle, EcmaPropertyKey name) {
      throw new NotImplementedException();
    }

    public bool IsCallable(EcmaValueHandle handle) {
      throw new NotImplementedException();
    }

    public bool IsExtensible(EcmaValueHandle handle) {
      throw new NotImplementedException();
    }

    public bool ToBoolean(EcmaValueHandle handle) {
      throw new NotImplementedException();
    }

    public double ToDouble(EcmaValueHandle handle) {
      throw new NotImplementedException();
    }

    public EcmaValueHandle ToHandle(object value) {
      throw new NotImplementedException();
    }

    public EcmaValue ToNumber(EcmaValueHandle handle) {
      throw new NotImplementedException();
    }

    public EcmaValue ToPrimitive(EcmaValueHandle handle, EcmaPreferredPrimitiveType preferredType) {
      throw new NotImplementedException();
    }

    public RuntimeObject ToRuntimeObject(EcmaValueHandle handle) {
      throw new NotImplementedException();
    }

    public string ToString(EcmaValueHandle handle) {
      throw new NotImplementedException();
    }

    public bool TryGet(EcmaValueHandle handle, EcmaPropertyKey name, out EcmaValue value) {
      throw new NotImplementedException();
    }

    public bool TrySet(EcmaValueHandle handle, EcmaPropertyKey name, EcmaValue value) {
      throw new NotImplementedException();
    }
    #endregion
  }
}
