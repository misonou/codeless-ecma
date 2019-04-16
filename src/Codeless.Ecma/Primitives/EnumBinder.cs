using Codeless.Ecma.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Codeless.Ecma.Primitives {
  internal class EnumBinder<T> : PrimitiveBinder<T> where T : struct {
    private readonly static Func<T, long> getHandleValue;
    private readonly static Func<long, T> getObject;

    static EnumBinder() {
      ParameterExpression t = Expression.Parameter(typeof(T), "obj");
      ParameterExpression p = Expression.Parameter(typeof(long), "obj");
      getHandleValue = Expression.Lambda<Func<T, long>>(Expression.Convert(t, typeof(long)), t).Compile();
      getObject = Expression.Lambda<Func<long, T>>(Expression.Convert(p, typeof(T)), p).Compile();
    }

    public override EcmaNumberType NumberType {
      get { return EcmaNumberType.Int64; }
    }

    public override string ToStringTag {
      get { return InternalString.ObjectTag.Number; }
    }

    public override EcmaValueType ValueType {
      get { return EcmaValueType.Number; }
    }

    public override T FromHandle(EcmaValueHandle handle) {
      return getObject(handle.Value);
    }

    public override EcmaValueHandle ToHandle(T value) {
      return new EcmaValueHandle(getHandleValue(value));
    }

    public override RuntimeObject ToRuntimeObject(T value) {
      return new TransientIntrinsicObject(new EcmaValue(ToHandle(value), this), WellKnownObject.NumberPrototype);
    }

    public override bool ToBoolean(T value) {
      return ToHandle(value).Value != 0;
    }

    public override double ToDouble(T value) {
      return ToHandle(value).Value;
    }

    public override int ToInt32(T value) {
      return (int)ToHandle(value).Value;
    }

    public override long ToInt64(T value) {
      return ToHandle(value).Value;
    }

    public override string ToString(T value) {
      return value.ToString();
    }
  }
}
