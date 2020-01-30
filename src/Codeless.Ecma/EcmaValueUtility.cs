using Codeless.Ecma.Native;
using Codeless.Ecma.Primitives;
using Codeless.Ecma.Runtime;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Codeless.Ecma {
  public static class EcmaValueUtility {
    private static readonly ConcurrentDictionary<Type, IEcmaValueBinder> binderTypes = new ConcurrentDictionary<Type, IEcmaValueBinder>(new Dictionary<Type, IEcmaValueBinder> {
      [typeof(WellKnownSymbol)] = WellKnownSymbolBinder.Default
    });
    private static Dictionary<Type, ConstructorInfo> ecmaValueConstructors;

    public static T GetUnderlyingObject<T>(this EcmaValue thisValue) {
      if (thisValue.Type != EcmaValueType.Object) {
        throw new EcmaTypeErrorException(InternalString.Error.NotObject);
      }
      object obj = thisValue.GetUnderlyingObject();
      if (obj is T value) {
        return value;
      }
      if (obj is INativeObjectWrapper wrapper && wrapper.Target is T value2) {
        return value2;
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
      throw new EcmaTypeErrorException(InternalString.Error.IncompatibleObject);
    }

    public static EcmaValue ConvertFromObject(object target) {
      switch (target) {
        case null:
          return EcmaValue.Undefined;
        case EcmaValue value:
          return value;
        case string str:
          return new EcmaValue(str);
        case Symbol sym:
          return new EcmaValue(sym);
        case RuntimeObject obj:
          return new EcmaValue(obj);
        case DateTime dt:
          return new EcmaDate(dt);
        case Delegate del:
          return DelegateRuntimeFunction.FromDelegate(del);
        case Task task:
          return Promise.FromTask(task);
        case Exception ex:
          return GetValueFromException(ex);
      }
      Type type = target.GetType();
      IEcmaValueBinder binder = null;
      if (type.IsEnum) {
        binder = binderTypes.GetOrAdd(type, t => (IEcmaValueBinder)Activator.CreateInstance(typeof(EnumBinder<>).MakeGenericType(t)));
      } else {
        switch (Type.GetTypeCode(type)) {
          case TypeCode.Boolean:
            return new EcmaValue(BooleanBinder.Default.ToHandle((bool)target), BooleanBinder.Default);
          case TypeCode.Byte:
          case TypeCode.SByte:
          case TypeCode.Char:
          case TypeCode.Int16:
          case TypeCode.UInt16:
            return new EcmaValue(Int32Binder.Default.ToHandle(Convert.ToInt32(target)), Int32Binder.Default);
          case TypeCode.Int32:
            return new EcmaValue(Int32Binder.Default.ToHandle((int)target), Int32Binder.Default);
          case TypeCode.UInt32:
            return new EcmaValue(Int64Binder.Default.ToHandle(Convert.ToInt64(target)), Int64Binder.Default);
          case TypeCode.Int64:
            return new EcmaValue(Int64Binder.Default.ToHandle((long)target), Int64Binder.Default);
          case TypeCode.UInt64:
          case TypeCode.Single:
            return new EcmaValue(DoubleBinder.Default.ToHandle(Convert.ToDouble(target)), DoubleBinder.Default);
          case TypeCode.Double:
            return new EcmaValue(DoubleBinder.Default.ToHandle((double)target), DoubleBinder.Default);
        }
        binder = RuntimeRealm.Current.GetRuntimeObject(target);
      }
      return new EcmaValue(binder.ToHandle(target), binder);
    }

    public static EcmaValue GetValueFromException(Exception ex) {
      return ex is EcmaException ex1 ? ex1.ToValue() : new EcmaError(ex);
    }

    [EcmaSpecification("CreateIterResultObject", EcmaSpecificationKind.AbstractOperations)]
    public static EcmaValue CreateIterResultObject(EcmaValue value, bool done) {
      EcmaObject o = new EcmaObject();
      o.CreateDataProperty(WellKnownProperty.Value, value);
      o.CreateDataProperty(WellKnownProperty.Done, done);
      return o;
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
      if (ecmaValueConstructors == null) {
        ecmaValueConstructors = typeof(EcmaValue).GetConstructors().Where(v => v.GetParameters().Length == 1 && v.GetParameters()[0].ParameterType.IsSealed).ToDictionary(v => v.GetParameters()[0].ParameterType);
      }
      if (ecmaValueConstructors.TryGetValue(value.Type, out ConstructorInfo ci)) {
        return Expression.New(ci, value);
      }
      if (typeof(RuntimeObject).IsAssignableFrom(value.Type)) {
        return Expression.Call(value, "ToValue", Type.EmptyTypes);
      }
      if (typeof(Task).IsAssignableFrom(value.Type)) {
        return Expression.Call(Expression.Call(typeof(Promise), "FromTask", Type.EmptyTypes, value), "ToValue", Type.EmptyTypes);
      }
      if (typeof(Exception).IsAssignableFrom(value.Type)) {
        return Expression.Call(typeof(EcmaValueUtility), "GetValueFromException", Type.EmptyTypes, Expression.Convert(value, typeof(Exception)));
      }
      return Expression.Call(typeof(EcmaValueUtility), "ConvertFromObject", Type.EmptyTypes, Expression.Convert(value, typeof(object)));
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
          return Expression.Call(value, "ToStringOrThrow", Type.EmptyTypes);
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
      if (!conversionType.IsValueType) {
        return Expression.Call(typeof(EcmaValueUtility), "GetUnderlyingObject", new[] { conversionType }, value);
      }
      return Expression.Convert(Expression.Call(typeof(EcmaValueUtility), "ConvertToUnknownType", Type.EmptyTypes, value, Expression.Constant(conversionType)), conversionType);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void Deconstruct(this EcmaValue value, out EcmaValue item1, out EcmaValue rest) {
      EcmaValue[] arr = CreateListFromArrayLike(value);
      item1 = arr.ElementAtOrDefault(0);
      rest = new EcmaArray(new List<EcmaValue>(arr.Skip(1)));
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void Deconstruct(this EcmaValue value, out EcmaValue item1, out EcmaValue item2, out EcmaValue rest) {
      EcmaValue[] arr = CreateListFromArrayLike(value);
      item1 = arr.ElementAtOrDefault(0);
      item2 = arr.ElementAtOrDefault(1);
      rest = new EcmaArray(new List<EcmaValue>(arr.Skip(2)));
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void Deconstruct(this EcmaValue value, out EcmaValue item1, out EcmaValue item2, out EcmaValue item3, out EcmaValue rest) {
      EcmaValue[] arr = CreateListFromArrayLike(value);
      item1 = arr.ElementAtOrDefault(0);
      item2 = arr.ElementAtOrDefault(1);
      item3 = arr.ElementAtOrDefault(2);
      rest = new EcmaArray(new List<EcmaValue>(arr.Skip(3)));
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void Deconstruct(this EcmaValue value, out EcmaValue item1, out EcmaValue item2, out EcmaValue item3, out EcmaValue item4, out EcmaValue rest) {
      EcmaValue[] arr = CreateListFromArrayLike(value);
      item1 = arr.ElementAtOrDefault(0);
      item2 = arr.ElementAtOrDefault(1);
      item3 = arr.ElementAtOrDefault(2);
      item4 = arr.ElementAtOrDefault(3);
      rest = new EcmaArray(new List<EcmaValue>(arr.Skip(4)));
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void Deconstruct(this EcmaValue value, out EcmaValue item1, out EcmaValue item2, out EcmaValue item3, out EcmaValue item4, out EcmaValue item5, out EcmaValue rest) {
      EcmaValue[] arr = CreateListFromArrayLike(value);
      item1 = arr.ElementAtOrDefault(0);
      item2 = arr.ElementAtOrDefault(1);
      item3 = arr.ElementAtOrDefault(2);
      item4 = arr.ElementAtOrDefault(3);
      item5 = arr.ElementAtOrDefault(4);
      rest = new EcmaArray(new List<EcmaValue>(arr.Skip(5)));
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void Deconstruct(this EcmaValue value, out EcmaValue item1, out EcmaValue item2, out EcmaValue item3, out EcmaValue item4, out EcmaValue item5, out EcmaValue item6, out EcmaValue rest) {
      EcmaValue[] arr = CreateListFromArrayLike(value);
      item1 = arr.ElementAtOrDefault(0);
      item2 = arr.ElementAtOrDefault(1);
      item3 = arr.ElementAtOrDefault(2);
      item4 = arr.ElementAtOrDefault(3);
      item5 = arr.ElementAtOrDefault(4);
      item6 = arr.ElementAtOrDefault(5);
      rest = new EcmaArray(new List<EcmaValue>(arr.Skip(6)));
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static void Deconstruct(this EcmaValue value, out EcmaValue item1, out EcmaValue item2, out EcmaValue item3, out EcmaValue item4, out EcmaValue item5, out EcmaValue item6, out EcmaValue item7, out EcmaValue rest) {
      EcmaValue[] arr = CreateListFromArrayLike(value);
      item1 = arr.ElementAtOrDefault(0);
      item2 = arr.ElementAtOrDefault(1);
      item3 = arr.ElementAtOrDefault(2);
      item4 = arr.ElementAtOrDefault(3);
      item5 = arr.ElementAtOrDefault(4);
      item6 = arr.ElementAtOrDefault(5);
      item7 = arr.ElementAtOrDefault(6);
      rest = new EcmaArray(new List<EcmaValue>(arr.Skip(7)));
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
