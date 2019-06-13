using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime {
  public static class RuntimeObjectExtension {
    private static readonly EcmaPropertyKey[] convertToPrimitiveMethodString = { WellKnownProperty.ToString, WellKnownProperty.ValueOf };
    private static readonly EcmaPropertyKey[] convertToPrimitiveMethodNumber = { WellKnownProperty.ValueOf, WellKnownProperty.ToString };

    [EcmaSpecification("GetMethod", EcmaSpecificationKind.AbstractOperations)]
    public static RuntimeFunction GetMethod(this RuntimeObject obj, EcmaPropertyKey propertyKey) {
      Guard.ArgumentNotNull(obj, "obj");
      EcmaValue value = obj.Get(propertyKey, obj);
      if (value.IsNullOrUndefined) {
        return null;
      }
      if (value.ToObject() is RuntimeFunction fn) {
        return fn;
      }
      throw new EcmaTypeErrorException(InternalString.Error.NotFunction);
    }

    [EcmaSpecification("GetIterator", EcmaSpecificationKind.AbstractOperations)]
    public static EcmaIteratorEnumerator GetIterator(this RuntimeObject obj) {
      Guard.ArgumentNotNull(obj, "obj");
      RuntimeObject method = obj.GetMethod(Symbol.Iterator);
      EcmaValue iterator = method.Call(obj);
      Guard.ArgumentIsObject(iterator);
      return new EcmaIteratorEnumerator(iterator);
    }

    [EcmaSpecification("CreateDataProperty", EcmaSpecificationKind.AbstractOperations)]
    public static bool CreateDataProperty(this RuntimeObject obj, EcmaPropertyKey propertyKey, EcmaValue value) {
      Guard.ArgumentNotNull(obj, "obj");
      return obj.DefineOwnProperty(propertyKey, new EcmaPropertyDescriptor(value, EcmaPropertyAttributes.DefaultDataProperty));
    }

    [EcmaSpecification("CreateMethodProperty", EcmaSpecificationKind.AbstractOperations)]
    public static bool CreateMethodProperty(this RuntimeObject obj, EcmaPropertyKey propertyKey, EcmaValue value) {
      Guard.ArgumentNotNull(obj, "obj");
      return obj.DefineOwnProperty(propertyKey, new EcmaPropertyDescriptor(value, EcmaPropertyAttributes.DefaultMethodProperty));
    }

    [EcmaSpecification("CreateDataPropertyOrThrow", EcmaSpecificationKind.AbstractOperations)]
    public static bool CreateDataPropertyOrThrow(this RuntimeObject obj, EcmaPropertyKey propertyKey, EcmaValue value) {
      Guard.ArgumentNotNull(obj, "obj");
      if (!obj.CreateDataProperty(propertyKey, value)) {
        throw new EcmaTypeErrorException(InternalString.Error.CreatePropertyThrow);
      }
      return true;
    }

    [EcmaSpecification("DefinePropertyOrThrow", EcmaSpecificationKind.AbstractOperations)]
    public static bool DefinePropertyOrThrow(this RuntimeObject obj, EcmaPropertyKey propertyKey, EcmaPropertyDescriptor descriptor) {
      Guard.ArgumentNotNull(obj, "obj");
      if (!obj.DefineOwnProperty(propertyKey, descriptor)) {
        throw new EcmaTypeErrorException(InternalString.Error.CreatePropertyThrow);
      }
      return true;
    }

    [EcmaSpecification("DeletePropertyOrThrow", EcmaSpecificationKind.AbstractOperations)]
    public static bool DeletePropertyOrThrow(this RuntimeObject obj, EcmaPropertyKey propertyKey) {
      Guard.ArgumentNotNull(obj, "obj");
      if (!obj.Delete(propertyKey)) {
        throw new EcmaTypeErrorException(InternalString.Error.DeletePropertyThrow);
      }
      return true;
    }

    [EcmaSpecification("OrdinaryToPrimitive", EcmaSpecificationKind.AbstractOperations)]
    public static EcmaValue OrdinaryToPrimitive(this RuntimeObject obj, EcmaPreferredPrimitiveType kind) {
      Guard.ArgumentNotNull(obj, "obj");
      EcmaValue result;
      EcmaPropertyKey[] m = kind == EcmaPreferredPrimitiveType.String ? convertToPrimitiveMethodString : convertToPrimitiveMethodNumber;
      if (TryConvertToPrimitive(obj, m[0], out result) || TryConvertToPrimitive(obj, m[1], out result)) {
        return result;
      }
      throw new EcmaTypeErrorException(InternalString.Error.NotConvertibleToPrimitive);
    }

    public static void SetOrThrow(this RuntimeObject obj, EcmaPropertyKey propertyKey, EcmaValue value) {
      Guard.ArgumentNotNull(obj, "obj");
      if (!obj.Set(propertyKey, value)) {
        throw new EcmaTypeErrorException(InternalString.Error.SetPropertyFailed, propertyKey);
      }
    }

    public static bool IsWellknownObject(this RuntimeObject obj, WellKnownObject type) {
      Guard.ArgumentNotNull(obj, "obj");
      return obj == obj.Realm.GetRuntimeObject(type);
    }

    public static bool IsIntrinsicFunction(this RuntimeObject obj, WellKnownObject type, EcmaPropertyKey name) {
      return obj is IntrinsicFunction fn && fn.IsIntrinsicFunction(type, name);
    }

    public static IEnumerable<EcmaPropertyKey> GetEnumerablePropertyKeys(this RuntimeObject obj) {
      for (RuntimeObject cur = obj; cur != null; cur = cur.GetPrototypeOf()) {
        foreach (EcmaPropertyKey key in cur.GetEnumerableOwnPropertyKeys()) {
          yield return key;
        }
      }
    }

    public static IEnumerable<EcmaPropertyKey> GetEnumerableOwnPropertyKeys(this RuntimeObject obj) {
      foreach (EcmaPropertyKey key in obj.GetOwnPropertyKeys()) {
        EcmaPropertyDescriptor descriptor = obj.GetOwnProperty(key);
        if (descriptor != null && descriptor.Enumerable) {
          yield return key;
        }
      }
    }

    public static IEnumerable<EcmaPropertyEntry> GetEnumerableOwnProperties(this RuntimeObject obj, bool enumerateSymbolProp) {
      foreach (EcmaPropertyKey key in obj.GetOwnPropertyKeys()) {
        if (enumerateSymbolProp || !key.IsSymbol) {
          EcmaPropertyDescriptor descriptor = obj.GetOwnProperty(key);
          if (descriptor != null && descriptor.Enumerable) {
            yield return new EcmaPropertyEntry(key, obj.Get(key));
          }
        }
      }
    }

    private static bool TryConvertToPrimitive(RuntimeObject obj, EcmaPropertyKey name, out EcmaValue result) {
      EcmaValue value = obj.Get(name);
      if (value.IsCallable) {
        result = value.Call(obj);
        return result.Type != EcmaValueType.Object;
      }
      result = default;
      return false;
    }
  }
}
