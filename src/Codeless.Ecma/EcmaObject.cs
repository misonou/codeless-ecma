using Codeless.Ecma.Runtime;
using Codeless.Ecma.Runtime.Intrinsics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codeless.Ecma {
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

    [IntrinsicMember(FunctionLength = 2)]
    public static EcmaValue Assign(EcmaValue target, params EcmaValue[] sources) {
      RuntimeObject obj = target.ToRuntimeObject();
      foreach (EcmaValue source in sources) {
        if (!source.IsNullOrUndefined) {
          foreach (EcmaPropertyEntry e in source.ToRuntimeObject().GetEnumerableOwnProperties()) {
            if (!obj.Set(e.Key, e.Value)) {
              throw new EcmaTypeErrorException("");
            }
          }
        }
      }
      return obj;
    }

    [IntrinsicMember]
    public static EcmaValue Create(EcmaValue proto, EcmaValue properties) {
      if (proto.Type != EcmaValueType.Object || proto.Type != EcmaValueType.Null) {
        throw new EcmaTypeErrorException(InternalString.Error.PrototypeNotObject);
      }
      RuntimeObject obj = Create(proto.ToRuntimeObject());
      return DefineProperties(obj, properties);
    }

    [IntrinsicMember]
    public static EcmaValue DefineProperties(EcmaValue target, EcmaValue properties) {
      if (target.Type != EcmaValueType.Object) {
        throw new EcmaTypeErrorException("");
      }
      RuntimeObject dst = target.ToRuntimeObject();
      foreach (EcmaPropertyEntry e in properties.ToRuntimeObject().GetEnumerableOwnProperties()) {
        dst.DefinePropertyOrThrow(e.Key, EcmaPropertyDescriptor.FromValue(e.Value));
      }
      return dst;
    }

    [IntrinsicMember]
    public static EcmaValue DefineProperty(EcmaValue target, EcmaValue property, EcmaValue attributes) {
      if (target.Type != EcmaValueType.Object) {
        throw new EcmaTypeErrorException("");
      }
      RuntimeObject obj = target.ToRuntimeObject();
      obj.DefinePropertyOrThrow(EcmaPropertyKey.FromValue(property), EcmaPropertyDescriptor.FromValue(attributes));
      return obj;
    }

    [IntrinsicMember]
    public static EcmaValue Entries(EcmaValue target) {
      List<EcmaValue> arr = new List<EcmaValue>();
      foreach (EcmaPropertyEntry e in target.ToRuntimeObject().GetEnumerableOwnProperties()) {
        arr.Add(e.ToValue());
      }
      return new EcmaArray(arr);
    }

    [IntrinsicMember]
    public static void Freeze() { }

    [IntrinsicMember]
    public static EcmaValue GetOwnPropertyDescriptor(EcmaValue target, EcmaValue property) {
      EcmaPropertyDescriptor descriptor = target.ToRuntimeObject().GetOwnProperty(EcmaPropertyKey.FromValue(property));
      if (descriptor == null) {
        return EcmaValue.Undefined;
      }
      return descriptor.ToValue();
    }

    [IntrinsicMember]
    public static EcmaValue GetOwnPropertyDescriptors(EcmaValue target) {
      RuntimeObject obj = target.ToRuntimeObject();
      RuntimeObject result = new EcmaObject();
      foreach (EcmaPropertyKey key in obj.OwnPropertyKeys) {
        EcmaPropertyDescriptor descriptor = obj.GetOwnProperty(key);
        if (descriptor != null) {
          result.CreateDataProperty(key, descriptor.ToValue());
        }
      }
      return result;
    }

    [IntrinsicMember]
    public static EcmaValue GetOwnPropertyNames(EcmaValue target) {
      return new EcmaArray(target.ToRuntimeObject().OwnPropertyKeys.Where(v => !v.IsSymbol).Select(v => v.ToValue()));
    }

    [IntrinsicMember]
    public static EcmaValue GetOwnPropertySymbols(EcmaValue target) {
      return new EcmaArray(target.ToRuntimeObject().OwnPropertyKeys.Where(v => v.IsSymbol).Select(v => v.ToValue()));
    }

    [IntrinsicMember]
    public static EcmaValue GetPrototypeOf(EcmaValue obj) {
      return new EcmaValue(obj.ToRuntimeObject().GetPrototypeOf());
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
    public static void IsFrozen() { }

    [IntrinsicMember]
    public static void IsSealed() { }

    [IntrinsicMember]
    public static EcmaValue Keys(EcmaValue target) {
      return new EcmaArray(target.ToRuntimeObject().GetEnumerableOwnProperties().Select(v => v.Key.ToValue()));
    }

    [IntrinsicMember]
    public static void PreventExtensions(EcmaValue v) { }

    [IntrinsicMember]
    public static void Seal() { }

    [IntrinsicMember]
    public static EcmaValue SetPrototypeOf(EcmaValue obj, EcmaValue proto) {
      obj.RequireObjectCoercible();
      if (proto.Type != EcmaValueType.Object || proto.Type != EcmaValueType.Null) {
        throw new EcmaTypeErrorException(InternalString.Error.PrototypeNotObject);
      }
      if (obj.Type != EcmaValueType.Object) {
        return obj;
      }
      if (!obj.ToRuntimeObject().SetPrototypeOf(proto.ToRuntimeObject())) {
        throw new EcmaTypeErrorException("");
      }
      return obj;
    }

    [IntrinsicMember]
    public static EcmaValue Values(EcmaValue target) {
      return new EcmaArray(target.ToRuntimeObject().GetEnumerableOwnProperties().Select(v => v.Value));
    }
  }
}
