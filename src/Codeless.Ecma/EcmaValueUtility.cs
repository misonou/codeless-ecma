using Codeless.Ecma.Primitives;
using Codeless.Ecma.Runtime;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Codeless.Ecma {
  public static class EcmaValueUtility {
    public static T GetUnderlyingObject<T>(this EcmaValue thisValue) {
      if (thisValue.Type != EcmaValueType.Object) {
        throw new EcmaTypeErrorException(InternalString.Error.NotObject);
      }
      if (thisValue.GetUnderlyingObject() is T value) {
        return value;
      }
      throw new EcmaTypeErrorException(InternalString.Error.IncompatibleObject);
    }

    public static EcmaValue GetIntrinsicPrimitiveValue(this EcmaValue thisValue, EcmaValueType type) {
      if (thisValue.Type == type) {
        return thisValue;
      }
      if (thisValue.Type == EcmaValueType.Object) {
        if (thisValue.GetUnderlyingObject() is PrimitiveObject obj && obj.PrimitiveValue.Type == type) {
          return obj.PrimitiveValue;
        }
      }
      throw new EcmaTypeErrorException(InternalString.Error.IncompatibleObject);
    }

    public static int ToInteger(this EcmaValue thisValue, int min, int max) {
      EcmaValue value = thisValue.ToInteger();
      return value > max ? max : value < min ? min : (int)value;
    }

    [EcmaSpecification("InstanceofOperator", EcmaSpecificationKind.RuntimeSemantics)]
    public static bool InstanceOf(this EcmaValue thisValue, EcmaValue constructor) {
      if (constructor.Type != EcmaValueType.Object) {
        throw new EcmaTypeErrorException(InternalString.Error.NotFunction);
      }
      RuntimeObject obj = constructor.ToObject();
      RuntimeObject instOfHandler = obj.GetMethod(Symbol.HasInstance);
      if (instOfHandler != null) {
        return instOfHandler.Call(constructor, thisValue).ToBoolean();
      }
      if (!constructor.IsCallable) {
        throw new EcmaTypeErrorException(InternalString.Error.NotFunction);
      }
      return obj.HasInstance(thisValue.ToObject());
    }

    [EcmaSpecification("Relational Operators `in`", EcmaSpecificationKind.RuntimeSemantics)]
    public static bool In(this EcmaValue thisValue, EcmaValue other) {
      if (other.Type != EcmaValueType.Object) {
        throw new EcmaTypeErrorException(InternalString.Error.NotObject);
      }
      return other.HasProperty(EcmaPropertyKey.FromValue(thisValue));
    }

    [EcmaSpecification("CreateListFromArrayLike", EcmaSpecificationKind.AbstractOperations)]
    public static EcmaValue[] CreateListFromArrayLike(EcmaValue value) {
      Guard.ArgumentIsObject(value);
      long len = value[WellKnownProperty.Length].ToLength();
      EcmaValue[] arr = new EcmaValue[len];
      for (long i = 0; i < len; i++) {
        arr[i] = value[i];
      }
      return arr;
    }

    public static bool TryIndexByPropertyKey(string str, EcmaPropertyKey propertyKey, out EcmaValue value) {
      if (propertyKey.IsArrayIndex && str != null) {
        long index = propertyKey.ToArrayIndex();
        if (index < str.Length) {
          value = new EcmaValue(new String(str[(int)index], 1));
          return true;
        }
      }
      value = default;
      return false;
    }

    public static bool TryIndexByPropertyKey(IList list, EcmaPropertyKey propertyKey, out EcmaValue value) {
      if (propertyKey.IsArrayIndex && list != null) {
        long index = propertyKey.ToArrayIndex();
        if (index < list.Count) {
          value = new EcmaValue(list[(int)index]);
          return true;
        }
      }
      value = default;
      return false;
    }

    public static EcmaValue UnboxPrimitiveObject(EcmaValue value) {
      if (value.Type == EcmaValueType.Object && value.GetUnderlyingObject() is PrimitiveObject obj) {
        return obj.PrimitiveValue;
      }
      return value;
    }

    public static object ConvertToUnknownType(EcmaValue value, Type conversionType) {
      object obj = value.GetUnderlyingObject();
      if (obj == null) {
        return !conversionType.IsValueType ? null : Activator.CreateInstance(conversionType);
      }
      if (conversionType.IsAssignableFrom(obj.GetType())) {
        return obj;
      }
      throw new InvalidCastException();
    }

    public static Expression ConvertToEcmaValueExpression(Expression value) {
      Guard.ArgumentNotNull(value, "value");
      if (value.Type == typeof(EcmaValue)) {
        return value;
      }
      if (value.Type == typeof(void)) {
#if NET35
        return Expression.Call(((Func<Action, EcmaValue>)InvokeAction).Method, Expression.Lambda<Action>(value));
#else
        return Expression.Block(value, Expression.Constant(EcmaValue.Undefined));
#endif
      }
      ConstructorInfo ci = typeof(EcmaValue).GetConstructors().FirstOrDefault(v => v.GetParameters()[0].ParameterType.IsAssignableFrom(value.Type)) ??
         typeof(EcmaValue).GetConstructors().FirstOrDefault(v => v.GetParameters()[0].ParameterType == typeof(object));
      return Expression.New(ci, value);
    }

    public static Expression ConvertFromEcmaValueExpression(Expression value, Type conversionType) {
      Guard.ArgumentNotNull(value, "value");
      Guard.ArgumentNotNull(conversionType, "conversionType");
      if (conversionType == value.Type) {
        return value;
      }
      if (conversionType.IsAssignableFrom(value.Type)) {
        return Expression.Convert(value, conversionType);
      }
      switch (Type.GetTypeCode(conversionType)) {
        case TypeCode.Boolean:
          return Expression.Call(value, "ToBoolean", Type.EmptyTypes);
        case TypeCode.Byte:
          return Expression.Call(value, "ToUInt8", Type.EmptyTypes);
        case TypeCode.Char:
          return Expression.Call(value, "ToChar", Type.EmptyTypes);
        case TypeCode.Double:
          return Expression.Call(value, "ToDouble", Type.EmptyTypes);
        case TypeCode.Int16:
          return Expression.Call(value, "ToInt16", Type.EmptyTypes);
        case TypeCode.Int32:
          return Expression.Call(value, "ToInt32", Type.EmptyTypes);
        case TypeCode.Int64:
          return Expression.Call(value, "ToInt64", Type.EmptyTypes);
        case TypeCode.SByte:
          return Expression.Call(value, "ToInt8", Type.EmptyTypes);
        case TypeCode.Single:
          return Expression.ConvertChecked(Expression.Call(value, "ToDouble", Type.EmptyTypes), typeof(float));
        case TypeCode.String:
          return Expression.Call(value, "ToString", Type.EmptyTypes, Expression.Constant(true));
        case TypeCode.UInt16:
          return Expression.Call(value, "ToUInt16", Type.EmptyTypes);
        case TypeCode.UInt32:
          return Expression.Call(value, "ToUInt32", Type.EmptyTypes);
        case TypeCode.UInt64:
          return Expression.ConvertChecked(Expression.Call(value, "ToInt64", Type.EmptyTypes), typeof(ulong));
      }
      if (conversionType == typeof(EcmaValue)) {
        return value;
      }
      if (conversionType == typeof(EcmaValue?)) {
        return Expression.Call(conversionType, "op_implicit", Type.EmptyTypes, value);
      }
      if (conversionType == typeof(RuntimeObject)) {
        return Expression.Call(value, "ToObject", Type.EmptyTypes);
      }
      if (conversionType == typeof(Symbol)) {
        return Expression.Call(value, "ToSymbol", Type.EmptyTypes);
      }
      if (conversionType == typeof(EcmaPropertyKey)) {
        return Expression.Call(typeof(EcmaPropertyKey), "FromValue", Type.EmptyTypes, value);
      }
      return Expression.Convert(Expression.Call(typeof(EcmaValueUtility), "ConvertToUnknownType", Type.EmptyTypes, value, Expression.Constant(conversionType)), conversionType);
    }

    [EcmaSpecification("StringNumericLiteral", EcmaSpecificationKind.RuntimeSemantics)]
    public static EcmaValue ParseStringNumericLiteral(string inputString) {
      Guard.ArgumentNotNull(inputString, "inputString");
      inputString = inputString.Trim();
      if (inputString.Length == 0) {
        return 0;
      }
      if (inputString.Length > 2 && inputString[0] == '0') {
        switch (inputString[1]) {
          case 'x':
          case 'X':
            return ParseIntInternal(inputString.Substring(2), 16, false);
          case 'o':
          case 'O':
            return ParseIntInternal(inputString.Substring(2), 8, false);
          case 'b':
          case 'B':
            return ParseIntInternal(inputString.Substring(2), 2, false);
        }
      }
      return ParseFloatInternal(inputString, false);
    }

    internal static EcmaValue ParseIntInternal(string inputString, int radix, bool allowTrail) {
      Guard.ArgumentNotNull(inputString, "inputString");
      inputString = inputString.Trim();
      int len = inputString.Length;
      if (len == 0) {
        return EcmaValue.NaN;
      }
      int charIndex = 0;
      int sign = 1;
      if (inputString[0] == '-') {
        sign = -1;
        charIndex = 1;
      } else if (inputString[0] == '+') {
        charIndex = 1;
      }
      if (charIndex == len) {
        return EcmaValue.NaN;
      }
      if ((radix == 16 || radix == 0) && len > charIndex + 1) {
        if (inputString[charIndex] == '0' && (inputString[charIndex + 1] == 'x' || inputString[charIndex + 1] == 'X')) {
          charIndex += 2;
          radix = 16;
        }
      }
      if (radix == 0) {
        radix = 10;
      }
      if (radix < 2 || radix > 36) {
        return EcmaValue.NaN;
      }
      int maxDigit = '0' + Math.Min(radix - 1, 9);
      int maxAlphaL = 'a' + (radix - 11);
      int maxAlphaC = 'A' + (radix - 11);
      double m = 0;
      int i = charIndex;
      for (; i < len; i++) {
        char ch = inputString[i];
        if (ch >= '0' && ch <= maxDigit) {
          m = m * radix + (ch - '0');
          continue;
        }
        if (ch >= 'a' && ch <= maxAlphaL) {
          m = m * radix + (ch - 'a' + 10);
          continue;
        }
        if (ch >= 'A' && ch <= maxAlphaC) {
          m = m * radix + (ch - 'A' + 10);
          continue;
        }
        if (!allowTrail) {
          return EcmaValue.NaN;
        }
        break;
      }
      if (i == charIndex) {
        return EcmaValue.NaN;
      }
      if (m == 0 && sign == -1) {
        return EcmaValue.NegativeZero;
      }
      if (m < Int32.MaxValue) {
        return (int)m * sign;
      }
      if (m < Int64.MaxValue) {
        return (long)m * sign;
      }
      return m * sign;
    }

    internal static EcmaValue ParseFloatInternal(string inputString, bool allowTrail) {
      Guard.ArgumentNotNull(inputString, "inputString");
      inputString = inputString.Trim();
      Match m = Regex.Match(inputString, "^[+-]?(Infinity|(\\d+\\.?\\d*|\\.\\d+)([eE][+-]?\\d+)?)");
      if (m.Success && (allowTrail || m.Length == inputString.Length)) {
        return m.Groups[1].Value != "Infinity" ? Double.Parse(m.Value) : m.Value[0] == '-' ? EcmaValue.NegativeInfinity : EcmaValue.Infinity;
      }
      return EcmaValue.NaN;
    }

#if NET35
    private static EcmaValue InvokeAction(Action action) {
      action();
      return default;
    }
#endif
  }
}
