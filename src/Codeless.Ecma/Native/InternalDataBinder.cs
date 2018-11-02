using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Codeless.Ecma.Runtime;
using System.Linq.Expressions;

namespace Codeless.Ecma.Native {
  internal class InternalDataBinder<T> : PrimitiveBinder<T> {
    private readonly static Func<T, long> getHandleValue;
    private readonly static Func<long, T> getObject;

    static InternalDataBinder() {
      ParameterExpression t = Expression.Parameter(typeof(T), "obj");
      ParameterExpression p = Expression.Parameter(typeof(long), "obj");
      getHandleValue = Expression.Lambda<Func<T, long>>(Expression.Convert(t, typeof(long)), t).Compile();
      getObject = Expression.Lambda<Func<long, T>>(Expression.Convert(p, typeof(T)), p).Compile();
    }

    public override EcmaNumberType NumberType {
      get { return EcmaNumberType.Invalid; }
    }

    public override string ToStringTag {
      get { return InternalString.ObjectTag.Object; }
    }

    public override EcmaValueType ValueType {
      get { return EcmaValueType.Object; }
    }

    public override T FromHandle(EcmaValueHandle handle) {
      return getObject(handle.Value);
    }

    public override EcmaValueHandle ToHandle(T value) {
      return new EcmaValueHandle(getHandleValue(value));
    }

    public override RuntimeObject ToRuntimeObject(T value) {
      throw new InvalidOperationException();
    }

    public override bool ToBoolean(T value) {
      throw new InvalidOperationException();
    }

    public override double ToDouble(T value) {
      throw new InvalidOperationException();
    }

    public override int ToInt32(T value) {
      throw new InvalidOperationException();
    }

    public override long ToInt64(T value) {
      throw new InvalidOperationException();
    }

    public override string ToString(T value) {
      return value.ToString();
    }
  }
}
