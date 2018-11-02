using Codeless.Ecma.Native;
using Codeless.Ecma.Runtime;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Codeless.Ecma {
  internal class EcmaValueUtility {
    private static readonly ConcurrentDictionary<Type, IEcmaValueBinder> binderTypes = new ConcurrentDictionary<Type, IEcmaValueBinder>(new Dictionary<Type, IEcmaValueBinder> {
      #region Entries for known types
      { typeof(bool), NativeBooleanBinder.Default },
      { typeof(byte), NativeInt32Binder.Default },
      { typeof(sbyte), NativeInt32Binder.Default },
      { typeof(char), NativeInt32Binder.Default },
      { typeof(short), NativeInt32Binder.Default },
      { typeof(ushort), NativeInt32Binder.Default },
      { typeof(int), NativeInt32Binder.Default },
      { typeof(uint), NativeInt64Binder.Default },
      { typeof(long), NativeInt64Binder.Default },
      { typeof(ulong), NativeDoubleBinder.Default },
      { typeof(float), NativeDoubleBinder.Default },
      { typeof(double), NativeDoubleBinder.Default },
      { typeof(string), NativeStringBinder.Default },
      { typeof(Symbol), SymbolBinder.Default },
      { typeof(DateTime), NativeDateTimeBinder.Default },
      { typeof(EcmaTimestamp), EcmaTimestampBinder.Default },
      { typeof(RuntimeObject), RuntimeObjectBinder.Default },
      { typeof(WellKnownSymbol), WellKnownSymbolBinder.Default },
      { typeof(WellKnownPropertyName), WellKnownPropertyNameBinder.Default },
      #endregion
    });

    public static bool IsIntrinsicPrimitiveValue(EcmaValue thisArg, EcmaValueType type) {
      if (thisArg.Type == type) {
        return true;
      }
      if (thisArg.Type == EcmaValueType.Object) {
        IEcmaIntrinsicObject obj = thisArg.GetUnderlyingObject() as IEcmaIntrinsicObject;
        if (obj != null && obj.IntrinsicValue.Type == type) {
          return true;
        }
      }
      return false;
    }

    public static EcmaValue GetIntrinsicPrimitiveValue(EcmaValue thisArg, EcmaValueType type) {
      if (thisArg.Type == type) {
        return thisArg;
      }
      if (thisArg.Type == EcmaValueType.Object) {
        IEcmaIntrinsicObject obj = thisArg.GetUnderlyingObject() as IEcmaIntrinsicObject;
        if (obj != null && obj.IntrinsicValue.Type == type) {
          return obj.IntrinsicValue;
        }
      }
      throw new EcmaTypeErrorException(InternalString.Error.IncompatibleObject);
    }

    public static T GetIntrinsicValue<T>(EcmaValue thisArg) {
      if (thisArg.Type == EcmaValueType.Object) {
        IEcmaIntrinsicObject obj = thisArg.GetUnderlyingObject() as IEcmaIntrinsicObject;
        if (obj != null) {
          try {
            return (T)obj.IntrinsicValue.GetUnderlyingObject();
          } catch (InvalidCastException) { }
        }
      }
      throw new EcmaTypeErrorException(InternalString.Error.IncompatibleObject);
    }

    public static void SetIntrinsicValue<T>(EcmaValue thisArg, T value) {
      if (thisArg.Type == EcmaValueType.Object) {
        IEcmaIntrinsicObject obj = thisArg.GetUnderlyingObject() as IEcmaIntrinsicObject;
        if (obj != null) {
          try {
            if (obj.IntrinsicValue.GetUnderlyingObject() is T) {
              obj.IntrinsicValue = new EcmaValue(value);
              return;
            }
          } catch (InvalidCastException) { }
        }
      }
      throw new EcmaTypeErrorException(InternalString.Error.IncompatibleObject);

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
      ConstructorInfo ci = typeof(EcmaValue).GetConstructors().FirstOrDefault(v => v.GetParameters()[0].ParameterType.IsAssignableFrom(value.Type)) ??
         typeof(EcmaValue).GetConstructors().FirstOrDefault(v => v.GetParameters()[0].ParameterType == typeof(object));
      return Expression.New(ci, value);
    }

    public static Expression ConvertFromEcmaValueExpression(Expression value, Type conversionType) {
      Guard.ArgumentNotNull(value, "value");
      Guard.ArgumentNotNull(conversionType, "conversionType");
      if (conversionType.IsInterface) {
        return Expression.Convert(Expression.Call(typeof(EcmaValueUtility), "GetUnderlyingObjectOfInterface", Type.EmptyTypes, value), conversionType);
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
          return Expression.Call(value, "ToString", Type.EmptyTypes);
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
      if (conversionType == typeof(RuntimeObject)) {
        return Expression.Call(value, "ToRuntimeObject", Type.EmptyTypes);
      }
      return Expression.Convert(Expression.Call(typeof(EcmaValueUtility), "ConvertToUnknownType", Type.EmptyTypes, value, Expression.Constant(conversionType)), conversionType);
    }

    public static IEcmaValueBinder GetBinder(object target) {
      Guard.ArgumentNotNull(target, "target");
      Type type = target.GetType();
      if (type.IsEnum) {
        return GetBinderForType(type);
      }
      switch (System.Type.GetTypeCode(type)) {
        case TypeCode.Boolean:
          return NativeBooleanBinder.Default;
        case TypeCode.Byte:
        case TypeCode.SByte:
        case TypeCode.Char:
        case TypeCode.Int16:
        case TypeCode.Int32:
        case TypeCode.UInt16:
          return NativeInt32Binder.Default;
        case TypeCode.Int64:
        case TypeCode.UInt32:
          return NativeInt64Binder.Default;
        case TypeCode.UInt64:
        case TypeCode.Single:
        case TypeCode.Double:
          return NativeDoubleBinder.Default;
        case TypeCode.DateTime:
          return NativeDateTimeBinder.Default;
      }
      if (type.IsValueType) {
        IEcmaValueBinder b = GetBinderForType(type);
        target = new IntrinsicObject(new EcmaValue(b.ToHandle(target), b), WellKnownObject.ObjectPrototype);
      }
      return ObjectReferenceBinder.GetBinder(target);
    }

    public static IEcmaValueBinder GetBinderForType(Type type) {
      Guard.ArgumentNotNull(type, "type");
      return binderTypes.GetOrAdd(type, t => {
        if (t.IsEnum) {
          return (IEcmaValueBinder)Activator.CreateInstance(typeof(NativeEnumBinder<>).MakeGenericType(t));
        }
        if (t.IsSubclassOf(typeof(RuntimeObject))) {
          return RuntimeObjectBinder.Default;
        }
        if (t.GetInterface(typeof(IDictionary<,>).FullName) != null) {
          Type i = t.GetInterface(typeof(IDictionary<,>).FullName);
          return (IEcmaValueBinder)Activator.CreateInstance(typeof(NativeDictionaryBinder<,>).MakeGenericType(i.GetGenericArguments()), t);
        }
        if (t.GetInterface(typeof(IDictionary).FullName) != null) {
          return new NativeDictionaryBinder(t);
        }
        if (t.GetInterface(typeof(IList).FullName) != null) {
          return new NativeListBinder(t);
        }
        if (t.GetInterface(typeof(ICollection).FullName) != null) {
          return new NativeCollectionBinder(t);
        }
        return new ReflectedObjectBinder(t);
      });
    }
  }
}
