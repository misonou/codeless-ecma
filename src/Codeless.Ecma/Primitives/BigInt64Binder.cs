#if BIGINTEGER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Codeless.Ecma.Runtime;

namespace Codeless.Ecma.Primitives {
  internal class BigInt64Binder : PrimitiveBinder<long> {
    public static readonly BigInt64Binder Default = new BigInt64Binder();

    protected BigInt64Binder() { }

    public override EcmaNumberType NumberType {
      get { return EcmaNumberType.BigInt64; }
    }

    public override string ToStringTag {
      get { return InternalString.ObjectTag.BigInt; }
    }

    public override EcmaValueType ValueType {
      get { return EcmaValueType.BigInt; }
    }

    public override long FromHandle(EcmaValueHandle handle) {
      return handle.Value;
    }

    public override EcmaValueHandle ToHandle(long value) {
      return new EcmaValueHandle(value);
    }

    public override RuntimeObject ToRuntimeObject(long value) {
      return new TransientPrimitiveObject(value, WellKnownObject.BigIntPrototype);
    }

    public override double ToDouble(long value) {
      throw new EcmaTypeErrorException(InternalString.Error.BigIntNotConvertibleToNumber);
    }

    public override int ToInt32(long value) {
      throw new EcmaTypeErrorException(InternalString.Error.BigIntNotConvertibleToNumber);
    }

    public override long ToInt64(long value) {
      return value;
    }

    public override string ToString(long value) {
      return value.ToString();
    }

    public override bool ToBoolean(long value) {
      return value != 0;
    }

    public override EcmaValue ToNumber(long value) {
      throw new EcmaTypeErrorException(InternalString.Error.BigIntNotConvertibleToNumber);
    }
  }
}
#endif
