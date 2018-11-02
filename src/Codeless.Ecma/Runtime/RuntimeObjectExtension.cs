using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma.Runtime {
  public static class RuntimeObjectExtension {
    public static EcmaValue Get(this RuntimeObject obj, EcmaPropertyKey propertyKey) {
      return obj.Get(propertyKey, obj);
    }

    public static bool Set(this RuntimeObject obj, EcmaPropertyKey propertyKey, EcmaValue value) {
      return obj.Set(propertyKey, value, obj);
    }

    [EcmaSpecification("GetMethod", EcmaSpecificationKind.AbstractOperations)]
    public static EcmaValue GetMethod(this RuntimeObject obj, EcmaPropertyKey propertyKey) {
      EcmaValue value = obj.Get(propertyKey, obj);
      if (value.IsCallable) {
        return value;
      }
      return default(EcmaValue);
    }

    [EcmaSpecification("CreateDataProperty", EcmaSpecificationKind.AbstractOperations)]
    public static bool CreateDataProperty(this RuntimeObject obj, EcmaPropertyKey propertyKey, EcmaValue value) {
      return obj.DefineOwnProperty(propertyKey, new EcmaPropertyDescriptor(value));
    }

    [EcmaSpecification("CreateMethodProperty", EcmaSpecificationKind.AbstractOperations)]
    public static bool CreateMethodProperty(this RuntimeObject obj, EcmaPropertyKey propertyKey, EcmaValue value) {
      return obj.DefineOwnProperty(propertyKey, new EcmaPropertyDescriptor(value, EcmaPropertyAttributes.Configurable | EcmaPropertyAttributes.Writable));
    }

    [EcmaSpecification("CreateDataPropertyOrThrow", EcmaSpecificationKind.AbstractOperations)]
    public static bool CreateDataPropertyOrThrow(this RuntimeObject obj, EcmaPropertyKey propertyKey, EcmaValue value) {
      if (!obj.CreateDataProperty(propertyKey, value)) {
        throw new EcmaTypeErrorException(InternalString.Error.CreatePropertyThrow);
      }
      return true;
    }

    [EcmaSpecification("DefinePropertyOrThrow", EcmaSpecificationKind.AbstractOperations)]
    public static bool DefinePropertyOrThrow(this RuntimeObject obj, EcmaPropertyKey propertyKey, EcmaPropertyDescriptor descriptor) {
      if (!obj.DefineOwnProperty(propertyKey, descriptor)) {
        throw new EcmaTypeErrorException(InternalString.Error.CreatePropertyThrow);
      }
      return true;
    }

    [EcmaSpecification("DeletePropertyOrThrow", EcmaSpecificationKind.AbstractOperations)]
    public static bool DeletePropertyOrThrow(this RuntimeObject obj, EcmaPropertyKey propertyKey) {
      if (!obj.Delete(propertyKey)) {
        throw new EcmaTypeErrorException(InternalString.Error.DeletePropertyThrow);
      }
      return true;
    }

    public static IEnumerable<EcmaPropertyKey> GetEnumerableOwnPropertyKeys(this RuntimeObject obj) {
      foreach (EcmaPropertyKey key in obj.OwnPropertyKeys) {
        EcmaPropertyDescriptor descriptor = obj.GetOwnProperty(key);
        if (descriptor != null && descriptor.Enumerable.Value) {
          yield return key;
        }
      }
    }

    public static IEnumerable<EcmaPropertyEntry> GetEnumerableOwnProperties(this RuntimeObject obj) {
      foreach (EcmaPropertyKey key in obj.OwnPropertyKeys) {
        EcmaPropertyDescriptor descriptor = obj.GetOwnProperty(key);
        if (descriptor != null && descriptor.Enumerable.Value) {
          yield return new EcmaPropertyEntry(key, obj.Get(key));
        }
      }
    }
  }
}
