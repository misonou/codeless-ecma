using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma {
  public interface IEcmaValueBinder {
    object FromHandle(EcmaValueHandle handle);
    EcmaValueHandle ToHandle(object value);
    string GetToStringTag(EcmaValueHandle handle);
    EcmaValueType GetValueType(EcmaValueHandle handle);
    EcmaNumberType GetNumberType(EcmaValueHandle handle);
    int GetHashCode(EcmaValueHandle handle);
    bool IsExtensible(EcmaValueHandle handle);
    bool IsCallable(EcmaValueHandle handle);
    bool ToBoolean(EcmaValueHandle handle);
    long ToInt64(EcmaValueHandle handle);
    double ToDouble(EcmaValueHandle handle);
    string ToString(EcmaValueHandle handle);
    EcmaValue ToPrimitive(EcmaValueHandle handle, EcmaPreferredPrimitiveType preferredType);
    EcmaValue ToNumber(EcmaValueHandle handle);
    RuntimeObject ToRuntimeObject(EcmaValueHandle handle);
    bool TryGet(EcmaValueHandle handle, EcmaPropertyKey name, out EcmaValue value);
    bool TrySet(EcmaValueHandle handle, EcmaPropertyKey name, EcmaValue value);
    EcmaValue Call(EcmaValueHandle handle, EcmaValue thisValue, EcmaValue[] arguments);
    bool HasProperty(EcmaValueHandle handle, EcmaPropertyKey name);
    bool HasOwnProperty(EcmaValueHandle handle, EcmaPropertyKey name);
    IEnumerable<EcmaPropertyKey> GetEnumerableOwnProperties(EcmaValueHandle handle);
  }
}
