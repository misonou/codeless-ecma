#if BIGINTEGER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Codeless.Ecma.Runtime;

namespace Codeless.Ecma.Primitives {
  internal class BigIntBinder : IEcmaValueBinder {
    private readonly BigInteger value;

    public BigIntBinder(BigInteger value) {
      this.value = value;
    }

    public EcmaValue ToValue() {
      return new EcmaValue(new EcmaValueHandle(this.value.GetHashCode()), this);
    }

    #region interface
    object IEcmaValueBinder.FromHandle(EcmaValueHandle handle) {
      return value;
    }

    EcmaValueHandle IEcmaValueBinder.ToHandle(object value) {
      return new EcmaValueHandle(this.value.GetHashCode());
    }

    int IEcmaValueBinder.GetHashCode(EcmaValueHandle handle) {
      return value.GetHashCode();
    }

    EcmaNumberType IEcmaValueBinder.GetNumberType(EcmaValueHandle handle) {
      return EcmaNumberType.BigInt;
    }

    EcmaValueType IEcmaValueBinder.GetValueType(EcmaValueHandle handle) {
      return EcmaValueType.BigInt;
    }

    string IEcmaValueBinder.GetToStringTag(EcmaValueHandle handle) {
      return InternalString.ObjectTag.BigInt;
    }

    bool IEcmaValueBinder.ToBoolean(EcmaValueHandle handle) {
      return value != BigInteger.Zero;
    }

    double IEcmaValueBinder.ToDouble(EcmaValueHandle handle) {
      throw new EcmaTypeErrorException(InternalString.Error.BigIntNotConvertibleToNumber);
    }

    long IEcmaValueBinder.ToInt64(EcmaValueHandle handle) {
      throw new EcmaTypeErrorException(InternalString.Error.BigIntNotConvertibleToNumber);
    }

    string IEcmaValueBinder.ToString(EcmaValueHandle handle) {
      return value.ToString();
    }

    EcmaValue IEcmaValueBinder.ToNumber(EcmaValueHandle handle) {
      throw new EcmaTypeErrorException(InternalString.Error.BigIntNotConvertibleToNumber);
    }

    EcmaValue IEcmaValueBinder.ToPrimitive(EcmaValueHandle handle, EcmaPreferredPrimitiveType preferredType) {
      return new EcmaValue(handle, this);
    }

    RuntimeObject IEcmaValueBinder.ToRuntimeObject(EcmaValueHandle handle) {
      return new TransientPrimitiveObject(ToValue(), WellKnownObject.BigIntPrototype);
    }

    bool IEcmaValueBinder.IsCallable(EcmaValueHandle handle) {
      return false;
    }

    bool IEcmaValueBinder.IsExtensible(EcmaValueHandle handle) {
      return false;
    }

    bool IEcmaValueBinder.HasOwnProperty(EcmaValueHandle handle, EcmaPropertyKey name) {
      return false;
    }

    bool IEcmaValueBinder.HasProperty(EcmaValueHandle handle, EcmaPropertyKey name) {
      return false;
    }

    IEnumerable<EcmaPropertyKey> IEcmaValueBinder.GetEnumerableOwnProperties(EcmaValueHandle handle) {
      yield break;
    }

    EcmaValue IEcmaValueBinder.Call(EcmaValueHandle handle, EcmaValue thisValue, EcmaValue[] arguments) {
      throw new EcmaTypeErrorException(InternalString.Error.NotFunction);
    }

    bool IEcmaValueBinder.TryGet(EcmaValueHandle handle, EcmaPropertyKey name, out EcmaValue value) {
      value = default;
      return false;
    }

    bool IEcmaValueBinder.TrySet(EcmaValueHandle handle, EcmaPropertyKey name, EcmaValue value) {
      return false;
    }
    #endregion
  }
}
#endif
