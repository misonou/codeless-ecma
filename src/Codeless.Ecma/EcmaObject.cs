using Codeless.Ecma.Runtime;
using Codeless.Ecma.Runtime.Intrinsics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma {
  [Cloneable(true)]
  [IntrinsicObject(WellKnownObject.ObjectConstructor)]
  public class EcmaObject : RuntimeObject {
    public EcmaObject()
      : base(WellKnownObject.ObjectPrototype) { }

    public EcmaObject(Hashtable properties)
      : this() {
      foreach (DictionaryEntry entry in properties) {
        this.CreateDataProperty(EcmaPropertyKey.FromValue(new EcmaValue(entry.Key)), new EcmaValue(entry.Value));
      }
    }

    [IntrinsicMember]
    public static EcmaValue Assign(EcmaValue target, params EcmaValue[] sources) {
      Guard.RequireObjectCoercible(target);
      RuntimeObject obj = ObjectConstructor.Object(null, target).ToObject();
      foreach (EcmaValue source in sources) {
        if (!source.IsNullOrUndefined) {
          foreach (EcmaPropertyEntry e in source.ToObject().GetEnumerableOwnProperties(true)) {
            obj.CreateDataPropertyOrThrow(e.Key, e.Value);
          }
        }
      }
      return obj;
    }

    [IntrinsicMember]
    public static EcmaValue Create(EcmaValue proto, EcmaValue properties) {
      if (proto.Type != EcmaValueType.Object && proto.Type != EcmaValueType.Null) {
        throw new EcmaTypeErrorException(InternalString.Error.PrototypeMustBeObjectOrNull);
      }
      RuntimeObject obj = Create(proto == EcmaValue.Null ? null : proto.ToObject());
      if (properties == default) {
        return obj;
      }
      return DefineProperties(obj, properties);
    }

    [IntrinsicMember]
    public static EcmaValue DefineProperties(EcmaValue target, EcmaValue properties) {
      Guard.ArgumentIsObject(target);
      RuntimeObject dst = target.ToObject();
      foreach (EcmaPropertyEntry e in properties.ToObject().GetEnumerableOwnProperties(true)) {
        dst.DefinePropertyOrThrow(e.Key, EcmaPropertyDescriptor.FromValue(e.Value));
      }
      return target;
    }

    [IntrinsicMember]
    public static EcmaValue DefineProperty(EcmaValue target, EcmaValue property, EcmaValue attributes) {
      Guard.ArgumentIsObject(target);
      RuntimeObject obj = target.ToObject();
      obj.DefinePropertyOrThrow(EcmaPropertyKey.FromValue(property), EcmaPropertyDescriptor.FromValue(attributes));
      return target;
    }

    [IntrinsicMember]
    public static EcmaValue Entries(EcmaValue target) {
      List<EcmaValue> arr = new List<EcmaValue>();
      foreach (EcmaPropertyEntry e in target.ToObject().GetEnumerableOwnProperties(false)) {
        arr.Add(e.ToValue());
      }
      return new EcmaArray(arr);
    }

    [IntrinsicMember]
    public static EcmaValue Freeze(EcmaValue obj) {
      if (obj.Type == EcmaValueType.Object) {
        if (!obj.ToObject().SetIntegrityLevel(RuntimeObjectIntegrityLevel.Frozen)) {
          throw new EcmaTypeErrorException(InternalString.Error.PreventExtensionFailed);
        }
      }
      return obj;
    }

    [IntrinsicMember]
    public static EcmaValue FromEntries(EcmaValue iterator) {
      EcmaObject obj = new EcmaObject();
      foreach (EcmaValue entry in iterator.ForOf()) {
        if (entry.Type != EcmaValueType.Object) {
          throw new EcmaTypeErrorException(InternalString.Error.EntryNotObject);
        }
        EcmaValue key = entry[0];
        EcmaValue value = entry[1];
        obj.CreateDataProperty(EcmaPropertyKey.FromValue(key), value);
      }
      return obj;
    }

    [IntrinsicMember]
    public static EcmaValue GetOwnPropertyDescriptor(EcmaValue target, EcmaValue property) {
      EcmaPropertyDescriptor descriptor = target.ToObject().GetOwnProperty(EcmaPropertyKey.FromValue(property));
      if (descriptor == null) {
        return default;
      }
      return descriptor.ToValue();
    }

    [IntrinsicMember]
    public static EcmaValue GetOwnPropertyDescriptors(EcmaValue target) {
      RuntimeObject obj = target.ToObject();
      RuntimeObject result = new EcmaObject();
      foreach (EcmaPropertyKey key in obj.GetOwnPropertyKeys()) {
        EcmaPropertyDescriptor descriptor = obj.GetOwnProperty(key);
        if (descriptor != null) {
          result.CreateDataProperty(key, descriptor.ToValue());
        }
      }
      return result;
    }

    [IntrinsicMember]
    public static EcmaValue GetOwnPropertyNames(EcmaValue target) {
      return new EcmaArray(target.ToObject().GetOwnPropertyKeys().Where(v => !v.IsSymbol).Select(v => v.ToValue()).ToList());
    }

    [IntrinsicMember]
    public static EcmaValue GetOwnPropertySymbols(EcmaValue target) {
      return new EcmaArray(target.ToObject().GetOwnPropertyKeys().Where(v => v.IsSymbol).Select(v => v.ToValue()).ToList());
    }

    [IntrinsicMember]
    public static EcmaValue GetPrototypeOf(EcmaValue obj) {
      Guard.RequireObjectCoercible(obj);
      RuntimeObject proto = obj.ToObject().GetPrototypeOf();
      return proto == null ? EcmaValue.Null : proto;
    }

    [IntrinsicMember]
    public static bool Is(EcmaValue x, EcmaValue y) {
      return EcmaValue.Equals(x, y, EcmaValueComparison.SameValue);
    }

    [IntrinsicMember]
    public new static bool IsExtensible(EcmaValue obj) {
      return obj.IsExtensible;
    }

    [IntrinsicMember]
    public static bool IsFrozen(EcmaValue obj) {
      if (obj.Type != EcmaValueType.Object) {
        return true;
      }
      RuntimeObject runtimeObj = obj.ToObject();
      return runtimeObj.IntegrityLevel >= RuntimeObjectIntegrityLevel.Frozen;
    }

    [IntrinsicMember]
    public static bool IsSealed(EcmaValue obj) {
      if (obj.Type != EcmaValueType.Object) {
        return true;
      }
      RuntimeObject runtimeObj = obj.ToObject();
      return runtimeObj.IntegrityLevel >= RuntimeObjectIntegrityLevel.Sealed;
    }

    [IntrinsicMember]
    public static EcmaValue Keys(EcmaValue target) {
      return new EcmaArray(target.ToObject().GetEnumerableOwnProperties(false).Select(v => v.Key.ToValue()).ToList());
    }

    [IntrinsicMember]
    public static EcmaValue PreventExtensions(EcmaValue obj) {
      if (obj.Type == EcmaValueType.Object) {
        if (!obj.ToObject().PreventExtensions()) {
          throw new EcmaTypeErrorException(InternalString.Error.PreventExtensionFailed);
        }
      }
      return obj;
    }

    [IntrinsicMember]
    public static EcmaValue Seal(EcmaValue obj) {
      if (obj.Type == EcmaValueType.Object) {
        if (!obj.ToObject().SetIntegrityLevel(RuntimeObjectIntegrityLevel.Sealed)) {
          throw new EcmaTypeErrorException(InternalString.Error.PreventExtensionFailed);
        }
      }
      return obj;
    }

    [IntrinsicMember]
    public static EcmaValue SetPrototypeOf(EcmaValue obj, EcmaValue proto) {
      Guard.RequireObjectCoercible(obj);
      if (proto.Type != EcmaValueType.Object && proto.Type != EcmaValueType.Null) {
        throw new EcmaTypeErrorException(InternalString.Error.PrototypeMustBeObjectOrNull);
      }
      if (obj.Type != EcmaValueType.Object) {
        return obj;
      }
      if (!obj.ToObject().SetPrototypeOf(proto == EcmaValue.Null ? null : proto.ToObject())) {
        throw new EcmaTypeErrorException(InternalString.Error.SetPrototypeFailed);
      }
      return obj;
    }

    [IntrinsicMember]
    public static EcmaValue Values(EcmaValue target) {
      return new EcmaArray(target.ToObject().GetEnumerableOwnProperties(false).Select(v => v.Value).ToList());
    }
  }
}
