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

    public static EcmaValue Construct(this RuntimeObject obj, params EcmaValue[] arguments) {
      return obj.Construct(obj, arguments);
    }

    [EcmaSpecification("GetMethod", EcmaSpecificationKind.AbstractOperations)]
    public static RuntimeFunction GetMethod(this RuntimeObject obj, EcmaPropertyKey propertyKey) {
      EcmaValue value = obj.Get(propertyKey, obj);
      return value.ToObject() as RuntimeFunction;
    }

    [EcmaSpecification("GetIterator", EcmaSpecificationKind.AbstractOperations)]
    public static EcmaIteratorEnumerator GetIterator(this RuntimeObject obj) {
      // TODO: GetIterator
      RuntimeObject method = obj.GetMethod(Symbol.Iterator);
      EcmaValue iterator = method.Call(obj);
      Guard.ArgumentIsObject(iterator);
      return new EcmaIteratorEnumerator(iterator);
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

    public static RuntimeObjectIntegrityLevel TestIntegrityLevelFast(this RuntimeObject obj) {
      RuntimeObject o = obj as RuntimeObject;
      return o != null ? o.IntegrityLevel : obj.TestIntegrityLevel();
    }

    [EcmaSpecification("TestIntegrityLevel", EcmaSpecificationKind.AbstractOperations)]
    public static RuntimeObjectIntegrityLevel TestIntegrityLevel(this RuntimeObject obj) {
      if (obj.IsExtensible) {
        return RuntimeObjectIntegrityLevel.Default;
      }
      foreach (EcmaPropertyKey key in obj.OwnPropertyKeys) {
        EcmaPropertyDescriptor desc = obj.GetOwnProperty(key);
        if (desc.Configurable != false) {
          return RuntimeObjectIntegrityLevel.ExtensionPrevented;
        }
        if (desc.IsDataDescriptor && desc.Writable != false) {
          return RuntimeObjectIntegrityLevel.Sealed;
        }
      }
      return RuntimeObjectIntegrityLevel.Frozen;
    }

    [EcmaSpecification("SetIntegrityLevel", EcmaSpecificationKind.AbstractOperations)]
    public static bool SetIntegrityLevel(this RuntimeObject obj, RuntimeObjectIntegrityLevel level) {
      if (!obj.PreventExtensions()) {
        return false;
      }
      if (obj.TestIntegrityLevelFast() < level) {
        switch (level) {
          case RuntimeObjectIntegrityLevel.Sealed:
            foreach (EcmaPropertyKey key in obj.OwnPropertyKeys) {
              obj.DefinePropertyOrThrow(key, new EcmaPropertyDescriptor { Configurable = false });
            }
            break;
          case RuntimeObjectIntegrityLevel.Frozen:
            foreach (EcmaPropertyKey key in obj.OwnPropertyKeys) {
              EcmaPropertyDescriptor desc = obj.GetOwnProperty(key);
              obj.DefinePropertyOrThrow(key, new EcmaPropertyDescriptor { Writable = desc.IsDataDescriptor ? (bool?)false : null, Configurable = false });
            }
            break;
        }
      }
      return true;
    }

    public static bool IsWellknownObject(this RuntimeObject obj, WellKnownObject type) {
      return obj == RuntimeRealm.GetRuntimeObject(type);
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
